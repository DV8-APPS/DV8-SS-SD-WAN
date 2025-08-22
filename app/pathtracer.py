from typing import Dict, List

def trace(src: str, dst: str) -> Dict[str, List[Dict[str, int]]]:
    hops = [
        {"hop": 1, "latency_ms": 5},
        {"hop": 2, "latency_ms": 10},
    ]
    return {"src": src, "dst": dst, "hops": hops}
