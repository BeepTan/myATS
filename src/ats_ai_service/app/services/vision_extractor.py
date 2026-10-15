import io
import base64
import logging
from typing import List
from PIL import Image
import pypdfium2 as pdfium
from openai import OpenAI
from fastapi import HTTPException
from app.core.config import settings
from app.schemas.resume_schema import ResumeDataSchema

logger = logging.getLogger(__name__)

SYSTEM_PROMPT = """You are an expert ATS resume extraction engine.
Carefully examine the resume document provided in the image(s).
Extract the candidate's personal details, professional summary, work experience, education, skills, and certifications.
Return the output strictly matching the required JSON structure.
Be thorough and preserve dates, job titles, and specific technical skills accurately.
"""

def file_bytes_to_base64_images(file_bytes: bytes, filename: str, max_pages: int = 3) -> List[str]:
    """Convert a PDF or image file into base64 JPEG strings."""
    images = []
    filename_lower = filename.lower()

    # Case 1: Direct image file (PNG, JPG, JPEG)
    if filename_lower.endswith((".png", ".jpg", ".jpeg", ".webp")):
        try:
            image = Image.open(io.BytesIO(file_bytes)).convert("RGB")
            buffer = io.BytesIO()
            image.save(buffer, format="JPEG", quality=85)
            encoded = base64.b64encode(buffer.getvalue()).decode("utf-8")
            return [encoded]
        except Exception as e:
            logger.warning(f"Could not read as direct image: {e}")

    # Case 2: PDF file
    try:
        pdf = pdfium.PdfDocument(file_bytes)
        total_pages = min(len(pdf), max_pages)
        for i in range(total_pages):
            page = pdf[i]
            image = page.render(scale=2.0).to_pil()
            buffer = io.BytesIO()
            image.save(buffer, format="JPEG", quality=85)
            encoded = base64.b64encode(buffer.getvalue()).decode("utf-8")
            images.append(encoded)
    except Exception as e:
        logger.error(f"PDFium error loading document: {e}")
        raise

    return images

def extract_resume_data(file_bytes: bytes, filename: str) -> ResumeDataSchema:
    """Extract structured resume data using OpenAI Vision without offline synthetic fallback."""
    if not settings.is_openai_configured:
        logger.warning("OpenAI API key not configured or format invalid.")
        raise HTTPException(
            status_code=503,
            detail="OpenAI API key is missing or does not match 'sk-proj-'. Automated resume extraction unavailable."
        )

    try:
        # Convert file (PDF or Image) to base64 images
        base64_images = file_bytes_to_base64_images(file_bytes, filename)
        if not base64_images:
            raise ValueError("No readable pages/images found in the uploaded file.")

        client = OpenAI(api_key=settings.OPENAI_API_KEY)

        # Build multimodal message content
        content = [{"type": "text", "text": "Extract all structured resume data from the attached resume document."}]
        for img_b64 in base64_images:
            content.append({
                "type": "image_url",
                "image_url": {
                    "url": f"data:image/jpeg;base64,{img_b64}",
                    "detail": "high"
                }
            })

        response = client.beta.chat.completions.parse(
            model=settings.OPENAI_VISION_MODEL,
            messages=[
                {"role": "system", "content": SYSTEM_PROMPT},
                {"role": "user", "content": content}
            ],
            response_format=ResumeDataSchema,
            temperature=0.0
        )

        parsed_data = response.choices[0].message.parsed
        parsed_data.parsing_mode = "ai_vision"
        return parsed_data

    except HTTPException:
        raise
    except Exception as ex:
        logger.error(f"Error during OpenAI vision parsing: {ex}")
        raise HTTPException(
            status_code=500,
            detail=f"OpenAI vision parsing error: {str(ex)}"
        )
