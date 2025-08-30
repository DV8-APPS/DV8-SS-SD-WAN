from typing import List, Dict
from collections import Counter


def average_latency(metrics: List[Dict[str, float]]) -> float:
    """Return average latency from metric list."""
    if not metrics:
        return 0.0
    return sum(m["latency_ms"] for m in metrics) / len(metrics)


def device_status_count(metrics: List[Dict[str, float]]) -> Dict[str, int]:
    """Count devices by status using Counter for efficiency."""
    return dict(Counter(m.get("status", "unknown") for m in metrics))
