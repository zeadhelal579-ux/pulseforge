# =============================================================================
# File    : Dockerfile
# Purpose : Runs the CardioSense AI service (FastAPI + Uvicorn) as a small non-root container.
# Logic   : Dependencies are installed before the code is copied so Docker caches that layer until
#           requirements.txt changes. The service is INTERNAL: it is started on port 8000 and must only be reachable
#           from the backend's private network (docker-compose does not publish the port).
# Usage   : docker build -t cardiosense-ai .
# =============================================================================
FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1

WORKDIR /srv

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app

RUN useradd --system --uid 10001 --no-create-home appuser
USER appuser

EXPOSE 8000
HEALTHCHECK --interval=15s --timeout=3s --start-period=10s --retries=3 \
    CMD ["python", "-c", "import urllib.request; urllib.request.urlopen('http://127.0.0.1:8000/health', timeout=2)"]

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
