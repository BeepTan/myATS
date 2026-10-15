from fastapi import APIRouter
from app.schemas.eval_schema import CandidateEvaluationRequest, CandidateEvaluationResponse
from app.services.rubric_scorer import evaluate_candidate_profile

router = APIRouter(prefix="/api/v1/evaluator", tags=["Candidate Evaluator"])

@router.post("/score", response_model=CandidateEvaluationResponse)
def evaluate_candidate(request: CandidateEvaluationRequest):
    """Evaluate and score a candidate profile against a job description, rubric, and minimum threshold."""
    return evaluate_candidate_profile(request)
