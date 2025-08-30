from __future__ import annotations
from typing import Dict


def recommend(site: str, intent_id: str, horizon: int) -> Dict[str, object]:
    return {"site": site, "intentId": intent_id, "recommendation": "scale_out", "horizonDays": horizon}
