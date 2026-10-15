from typing import List, Optional
from pydantic import BaseModel, Field

class PersonalInfo(BaseModel):
    full_name: str = Field(default="", description="Candidate's full name")
    email: str = Field(default="", description="Email address")
    phone: str = Field(default="", description="Contact phone number")
    location: str = Field(default="", description="City, State, or Country")
    linkedin_url: Optional[str] = Field(default="", description="LinkedIn profile URL")
    github_url: Optional[str] = Field(default="", description="GitHub profile URL")

class WorkExperience(BaseModel):
    company: str = Field(default="", description="Company or organization name")
    job_title: str = Field(default="", description="Role or designation")
    start_date: str = Field(default="", description="Start date (YYYY-MM or Year)")
    end_date: str = Field(default="", description="End date (YYYY-MM, Year, or Present)")
    responsibilities: List[str] = Field(default_factory=list, description="Key achievements and duties")

class Education(BaseModel):
    institution: str = Field(default="", description="University, College, or School")
    degree: str = Field(default="", description="Degree or program (e.g. B.S. Computer Science)")
    graduation_year: str = Field(default="", description="Year of graduation or expected")

class ResumeDataSchema(BaseModel):
    personal_info: PersonalInfo = Field(default_factory=PersonalInfo)
    summary: str = Field(default="", description="Professional summary or bio")
    work_experience: List[WorkExperience] = Field(default_factory=list)
    education: List[Education] = Field(default_factory=list)
    skills: List[str] = Field(default_factory=list, description="Extracted technical and soft skills")
    certifications: List[str] = Field(default_factory=list, description="Licenses and certifications")
    parsing_mode: str = Field(default="ai_vision", description="ai_vision or heuristic_fallback")
    warning_message: Optional[str] = None
