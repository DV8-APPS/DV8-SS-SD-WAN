VENDOR_COLLECTORS = {
    "meraki": ["inventory", "health"],
    "viptela": ["inventory", "health"],
    "velocloud": ["inventory", "health"],
    "prisma": ["inventory", "health"],
    "fortimanager": ["inventory", "health"],
    "aruba": ["inventory", "health"],
}


def list_collectors(vendor: str):
    return VENDOR_COLLECTORS.get(vendor.lower(), [])
