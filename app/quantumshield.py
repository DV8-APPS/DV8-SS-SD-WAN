"""Minimal DV8 QuantumShield placeholder utilities."""
from typing import Dict

def risk_score(status: str, latency: float = 0.0) -> int:
    score = 0
    if status == "down":
        score += 70
    if latency > 50:
        score += 20
    return min(score, 100)
