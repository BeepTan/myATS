import logging
from fastapi import HTTPException
from openai import OpenAI
from app.core.config import settings
from app.schemas.eval_schema import CandidateEvaluationRequest, CandidateEvaluationResponse

logger = logging.getLogger(__name__)

EVALUATION_SYSTEM_PROMPT = """You are an objective, rigorous ATS AI Candidate Evaluator.
Your goal is to evaluate a candidate's profile against a specific Job Posting and Evaluation Rubric.

Instructions:
1. Examine the Job Requirements and mandatory prerequisite skills.
2. Evaluate the Candidate Profile across the defined criteria categories.
3. Calculate category scores (each 0.0 to 100.0) based on relevance, depth, and years of experience.
4. Calculate a weighted overall total_score (0.0 to 100.0).
5. Compare total_score to min_passing_score and set threshold_passed accordingly.
6. Check if all hard requirements / prerequisites are satisfied.
7. Provide concrete strengths, weaknesses, and a concise reasoning summary.
8. Be fair, avoid hallucinating qualifications not present in the profile.
"""

def evaluate_candidate_profile(request: CandidateEvaluationRequest) -> CandidateEvaluationResponse:
    """Evaluate candidate profile against job description and rubric without synthetic offline fallback."""
    if not settings.is_openai_configured:
        logger.warning("OpenAI API key not configured or format invalid.")
        raise HTTPException(
            status_code=503,
            detail="OpenAI API key is missing or does not match 'sk-proj-'. Automated candidate evaluation unavailable."
        )

    try:
        client = OpenAI(api_key=settings.OPENAI_API_KEY)

        # Prepare evaluation payload
        criteria_text = "\n".join([
            f"- {c.category} (Weight: {c.weight_percentage}%): {c.instructions}"
            for c in request.criteria
        ]) if request.criteria else "Evaluate overall technical fit (50%) and relevant experience (50%)."

        prompt = f"""
### JOB DETAILS
Title: {request.job_title}
Description:
{request.job_description}

Minimum Passing Threshold: {request.min_passing_score}/100
Mandatory Skills: {', '.join(request.required_skills) if request.required_skills else 'None specified'}

### EVALUATION RUBRIC & CRITERIA
{criteria_text}

### CANDIDATE PROFILE
Name: {request.candidate_profile.personal_info.full_name}
Summary: {request.candidate_profile.summary}
Skills: {', '.join(request.candidate_profile.skills)}
Experience:
{request.candidate_profile.work_experience}
Education:
{request.candidate_profile.education}
Certifications:
{', '.join(request.candidate_profile.certifications)}
"""

        response = client.beta.chat.completions.parse(
            model=settings.OPENAI_EVAL_MODEL,
            messages=[
                {"role": "system", "content": EVALUATION_SYSTEM_PROMPT},
                {"role": "user", "content": prompt}
            ],
            response_format=CandidateEvaluationResponse,
            temperature=0.1
        )

        result = response.choices[0].message.parsed
        result.job_id = request.job_id
        result.min_passing_score = request.min_passing_score
        result.threshold_passed = result.total_score >= request.min_passing_score and result.hard_requirements_met
        result.evaluation_mode = "ai_rubric"
        return result

    except HTTPException:
        raise
    except Exception as ex:
        logger.error(f"Error during OpenAI candidate evaluation: {ex}")
        raise HTTPException(
            status_code=500,
            detail=f"OpenAI evaluation failed: {str(ex)}"
        )
