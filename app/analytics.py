from typing import List, Dict


def average_latency(metrics: List[Dict[str, float]]) -> float:
    """Return average latency from metric list."""
    if not metrics:
        return 0.0
    return sum(m["latency_ms"] for m in metrics) / len(metrics)


def device_status_count(metrics: List[Dict[str, float]]) -> Dict[str, int]:
    """Count devices by status."""
    counts: Dict[str, int] = {}
    for m in metrics:
        status = m.get("status", "unknown")
        counts[status] = counts.get(status, 0) + 1
    return counts
