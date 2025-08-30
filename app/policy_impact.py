from typing import Dict, List

def timeline(policy_id: str, before: str, after: str) -> Dict[str, List[str]]:
    return {"policy": policy_id, "before": before, "after": after, "changes": []}
