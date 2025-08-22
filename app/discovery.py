from __future__ import annotations

from datetime import datetime
from typing import Dict, List
from uuid import uuid4

jobs: Dict[str, Dict[str, object]] = {}
candidates: List[Dict[str, object]] = []


def start_job(tenant: str, scopes: List[str], cidrs: List[str], passive_only: bool) -> Dict[str, object]:
    job_id = str(uuid4())
    job = {
        "id": job_id,
        "tenant": tenant,
        "scopes": scopes,
        "cidrs": cidrs,
        "status": "running",
        "started_at": datetime.utcnow().isoformat() + "Z",
        "passive_only": passive_only,
    }
    jobs[job_id] = job
    return job


def get_job(job_id: str) -> Dict[str, object] | None:
    return jobs.get(job_id)


def list_candidates(tenant: str) -> Dict[str, List[Dict[str, object]]]:
    # In real system, filter by tenant; here return all
    return {"items": candidates}
