from fastapi import APIRouter
from app.core.config import settings

router = APIRouter(prefix="/health", tags=["Health"])

@router.get("")
def check_health():
    """Health check endpoint to probe service status and OpenAI readiness."""
    key_status = settings.openai_key_status
    key = settings.OPENAI_API_KEY
    prefix = (key[:12] + "...") if key.startswith("sk-proj-") else ("invalid_format" if key else "none")

    msg = "Microservice online and OpenAI API key format is valid."
    if key_status == "invalid_format":
        msg = "OpenAI API key does not start with 'sk-proj-' or is invalid format."
    elif key_status == "missing":
        msg = "OpenAI API key is missing or not configured in .env."

    return {
        "status": "healthy",
        "service": "online",
        "openai_key_status": key_status,
        "openai_key_valid": settings.is_openai_configured,
        "openai_key_prefix": prefix,
        "models": {
            "vision": settings.OPENAI_VISION_MODEL,
            "evaluator": settings.OPENAI_EVAL_MODEL
        },
        "message": msg
    }
