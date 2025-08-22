from __future__ import annotations
from typing import Dict, List

_probes: List[Dict[str, object]] = []


def create_probe(intent_id: str, endpoints: List[str], proto: str, interval: int) -> Dict[str, object]:
    probe = {"id": len(_probes) + 1, "intentId": intent_id, "endpoints": endpoints, "proto": proto, "interval": interval}
    _probes.append(probe)
    return probe


def get_results(intent_id: str) -> List[Dict[str, object]]:
    # stubbed results
    return [{"intentId": intent_id, "endpoint": ep, "latency_ms": 10.0} for ep in ["synthetic"]]
