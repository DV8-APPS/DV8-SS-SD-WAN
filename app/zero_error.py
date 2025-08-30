from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from .db import SessionLocal, AuditLog

def init_zero_error(app: FastAPI) -> None:
    @app.middleware("http")
    async def zero_error_middleware(request: Request, call_next):
        try:
            return await call_next(request)
        except Exception as exc:
            with SessionLocal() as db:
                db.add(AuditLog(action="error", entity="system", details=str(exc)))
                db.commit()
            return JSONResponse(status_code=500, content={"detail": "internal error"})
