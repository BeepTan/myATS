from typing import List, Optional
from pydantic import BaseModel, Field
from app.schemas.resume_schema import ResumeDataSchema

class EvaluationCriterion(BaseModel):
    category: str = Field(description="Criterion name, e.g., 'Technical Skills'")
    weight_percentage: float = Field(default=25.0, description="Percentage weight (e.g. 40.0)")
    instructions: str = Field(default="", description="Guidance for evaluating this category")

class CategoryScoreItem(BaseModel):
    category: str = Field(description="Name of the criterion category, e.g., 'Technical Skills'")
    score: float = Field(description="Score awarded between 0.0 and 100.0")

class CandidateEvaluationRequest(BaseModel):
    job_id: str
    job_title: str
    job_description: str
    min_passing_score: float = Field(default=70.0, description="Minimum cutoff threshold (0-100)")
    required_skills: List[str] = Field(default_factory=list, description="Mandatory prerequisite skills")
    criteria: List[EvaluationCriterion] = Field(default_factory=list)
    candidate_profile: ResumeDataSchema

class CandidateEvaluationResponse(BaseModel):
    job_id: str
    total_score: float = Field(description="Normalized overall score between 0.0 and 100.0")
    min_passing_score: float
    threshold_passed: bool = Field(description="True if total_score >= min_passing_score")
    category_scores: List[CategoryScoreItem] = Field(default_factory=list, description="Score breakdown per evaluation category")
    hard_requirements_met: bool = Field(default=True)
    missing_prerequisites: List[str] = Field(default_factory=list)
    strengths: List[str] = Field(default_factory=list)
    weaknesses: List[str] = Field(default_factory=list)
    reasoning_summary: str = Field(default="")
    evaluation_mode: str = Field(default="ai_rubric", description="'ai_rubric' or 'heuristic_fallback'")
    warning_message: Optional[str] = None
