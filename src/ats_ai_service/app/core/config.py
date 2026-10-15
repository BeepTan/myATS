import os
import re
from dotenv import load_dotenv

load_dotenv()

# OpenAI project API keys begin with 'sk-proj-' followed by alphanumeric, underscores, and hyphens
OPENAI_KEY_REGEX = re.compile(r"^sk-proj-[A-Za-z0-9_\-]{20,}$")

class Settings:
    OPENAI_API_KEY: str = os.getenv("OPENAI_API_KEY", "").strip()
    OPENAI_VISION_MODEL: str = os.getenv("OPENAI_VISION_MODEL", "gpt-4o")
    OPENAI_EVAL_MODEL: str = os.getenv("OPENAI_EVAL_MODEL", "gpt-4o-mini")
    PORT: int = int(os.getenv("PORT", "8000"))
    HOST: str = os.getenv("HOST", "0.0.0.0")

    @property
    def openai_key_status(self) -> str:
        """
        Evaluate API key status:
        - 'missing': empty or placeholder
        - 'invalid_format': non-empty but does not match 'sk-proj-...'
        - 'valid_format': matches regex 'sk-proj-...'
        """
        key = (self.OPENAI_API_KEY or "").strip()
        if not key or key.startswith("your-openai"):
            return "missing"
        if not OPENAI_KEY_REGEX.match(key):
            return "invalid_format"
        return "valid_format"

    @property
    def is_openai_configured(self) -> bool:
        return self.openai_key_status == "valid_format"

settings = Settings()
