import uvicorn
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api.health import router as health_router
from app.api.parser import router as parser_router
from app.api.evaluator import router as evaluator_router
from app.core.config import settings

app = FastAPI(
    title="myATS AI Microservice",
    description="VLLM/OpenAI Vision Resume Parser & Rubric-based Candidate Ranking Engine",
    version="1.0.0"
)

# Enable CORS for Blazor frontend calls
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(health_router)
app.include_router(parser_router)
app.include_router(evaluator_router)

@app.get("/")
def root():
    return {
        "message": "myATS AI Microservice is online.",
        "documentation": "/docs",
        "health": "/health"
    }

if __name__ == "__main__":
    uvicorn.run("main:app", host=settings.HOST, port=settings.PORT, reload=True)
