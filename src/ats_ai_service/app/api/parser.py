from fastapi import APIRouter, UploadFile, File, HTTPException
from app.schemas.resume_schema import ResumeDataSchema
from app.services.vision_extractor import extract_resume_data

router = APIRouter(prefix="/api/v1/parser", tags=["Resume Parser"])

@router.post("/extract", response_model=ResumeDataSchema)
async def parse_resume(file: UploadFile = File(...)):
    """Upload a resume (PDF/image) and extract structured profile data via VLLM/OpenAI Vision."""
    if not file.filename.lower().endswith((".pdf", ".png", ".jpg", ".jpeg")):
        raise HTTPException(
            status_code=400,
            detail="Unsupported file format. Please upload a PDF or image file (.pdf, .png, .jpg, .jpeg)."
        )

    file_bytes = await file.read()
    if len(file_bytes) > 15 * 1024 * 1024:
        raise HTTPException(status_code=400, detail="File size exceeds maximum limit of 15MB.")

    return extract_resume_data(file_bytes, file.filename)
