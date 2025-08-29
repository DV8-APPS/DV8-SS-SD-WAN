# Compliance Verification Framework

## Overview

This document defines objective pass/fail CI gates and verification methods for ensuring compliance with UX requirements and international standards.

---

## CI Gates (Automated)

### 1. Accessibility Gate

**Requirement**: Zero critical/serious Axe-core violations  
**Standards**: WCAG 2.2 AA compliance  
**Implementation**:

```yaml
# .github/workflows/accessibility.yml
name: Accessibility Testing
on: [push, pull_request]
jobs:
  axe-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install dependencies
        run: |
          npm install -g @axe-core/cli
          pip install -r requirements.txt
      - name: Start application
        run: |
          python run_dv8.py &
          sleep 10
      - name: Run Axe accessibility scan
        run: |
          axe http://localhost:8000/dashboard --exit
          axe http://localhost:8000/ --exit
      - name: Keyboard navigation test
        run: |
          # Custom keyboard traversal script
          python tests/accessibility/keyboard_navigation.py
```

**Pass Criteria**:
- Zero critical violations
- Zero serious violations  
- Keyboard navigation script passes
- Screen reader spot checks pass (manual gate)

### 2. Performance Gate

**Requirement**: Core Web Vitals budgets per page type  
**Standards**: LCP ≤2.5s, INP ≤200ms, CLS <0.10  
**Implementation**:

```yaml
# .github/workflows/performance.yml
name: Performance Testing
on: [push, pull_request]
jobs:
  lighthouse:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install dependencies
        run: |
          npm install -g lighthouse-ci
          pip install -r requirements.txt
      - name: Start application
        run: |
          python run_dv8.py &
          sleep 10
      - name: Run Lighthouse CI
        run: |
          lhci autorun --config=.lighthouserc.json
```

**Lighthouse Configuration** (`.lighthouserc.json`):
```json
{
  "ci": {
    "collect": {
      "url": [
        "http://localhost:8000/",
        "http://localhost:8000/dashboard"
      ],
      "numberOfRuns": 3
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.85}],
        "largest-contentful-paint": ["error", {"maxNumericValue": 2500}],
        "max-potential-fid": ["error", {"maxNumericValue": 200}],
        "cumulative-layout-shift": ["error", {"maxNumericValue": 0.1}]
      }
    }
  }
}
```

**Pass Criteria**:
- Performance score ≥85
- LCP ≤2.5s 
- FID/INP ≤200ms
- CLS <0.10

### 3. Security Gate

**Requirement**: OWASP ASVS baseline compliance  
**Standards**: OWASP ASVS v4.0+ controls  
**Implementation**:

```yaml
# .github/workflows/security.yml
name: Security Testing
on: [push, pull_request]
jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run OWASP ZAP Baseline Scan
        uses: zaproxy/action-baseline@v0.7.0
        with:
          target: 'http://localhost:8000'
          rules_file_name: '.zap/rules.tsv'
      - name: Dependency vulnerability scan
        run: |
          pip install safety
          safety check --json
      - name: ASVS checklist validation
        run: |
          python tests/security/asvs_validation.py
```

**Pass Criteria**:
- Zero high-risk ZAP findings
- Zero critical dependency vulnerabilities
- ASVS checklist 100% compliant for implemented features

### 4. UX Testing Gate

**Requirement**: Top 5 workflows ≥90% task-success  
**Standards**: ISO 9241-210 usability validation  
**Implementation**:

```yaml
# .github/workflows/ux-testing.yml
name: UX Validation
on: [push, pull_request]
jobs:
  user-flow-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Install Playwright
        run: |
          npm install -g playwright
          playwright install
      - name: Run critical user flows
        run: |
          python tests/ux/critical_flows.py
      - name: Validate task completion rates
        run: |
          python tests/ux/task_success_validation.py
```

**Pass Criteria**:
- All critical user flows complete successfully
- Task success simulation ≥90%
- No regression in flow completion times

---

## Manual Verification Gates

### 1. Screen Reader Testing

**Frequency**: Per release  
**Requirements**: A11Y-WCAG-* compliance  
**Process**:

1. **NVDA Testing** (Windows):
   - Navigate login flow using only NVDA
   - Complete dashboard interaction with screen reader
   - Verify form completion and error handling
   - Validate table navigation and data comprehension

2. **VoiceOver Testing** (macOS):
   - Repeat NVDA test scenarios
   - Verify mobile Safari compatibility
   - Test gesture navigation on touch devices

3. **JAWS Testing** (Windows - optional):
   - Enterprise screen reader validation
   - Complex form and table interaction testing

**Pass Criteria**:
- All key tasks completable with screen reader only
- Information announced clearly and logically
- No navigation dead-ends or confusion

### 2. Cross-Browser Testing

**Frequency**: Per release  
**Requirements**: NFR-COMP-002  
**Browsers**:
- Chrome (latest 2 versions)
- Firefox (latest 2 versions)  
- Safari (latest 2 versions)
- Edge (latest 2 versions)

**Test Scenarios**:
- Login and authentication flows
- Dashboard interaction and visualization
- Form submission and validation
- Table sorting and filtering
- Responsive design across viewport sizes

**Pass Criteria**:
- Consistent functionality across all browsers
- No visual regressions or layout issues
- Performance within acceptable variance (±10%)

### 3. Usability Testing

**Frequency**: Per major release  
**Requirements**: HCD-9241-EVAL01, NFR-USAB-001  
**Process**:

1. **Participant Recruitment**:
   - n≥8 per primary persona
   - Representative of target user base
   - Mix of experience levels

2. **Task Scenarios**:
   - Primary: Login and dashboard overview
   - Primary: Device status monitoring and filtering
   - Primary: Incident investigation and response
   - Secondary: Configuration changes
   - Secondary: Report generation and export

3. **Metrics Collected**:
   - Task completion rate (target: ≥90%)
   - Time on task (baseline vs. current)
   - Error rate and recovery
   - Subjective satisfaction (SUS score target: ≥80)

**Pass Criteria**:
- Task success rate ≥90% for primary tasks
- No significant increase in task completion time
- SUS score ≥80
- Critical usability issues identified and addressed

---

## Test Implementation Examples

### Accessibility Testing Script

```python
# tests/accessibility/keyboard_navigation.py
import pytest
from playwright.sync_api import sync_playwright

def test_keyboard_navigation():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        page.goto("http://localhost:8000/dashboard")
        
        # Test tab order
        page.keyboard.press("Tab")
        focused_element = page.evaluate("document.activeElement.tagName")
        assert focused_element in ["BUTTON", "A", "INPUT"], f"First focusable element should be interactive, got {focused_element}"
        
        # Test skip link
        page.keyboard.press("Enter")
        # Verify skip link functionality
        
        # Test form navigation
        page.goto("http://localhost:8000/login")
        page.keyboard.press("Tab")
        page.keyboard.press("Tab")
        # Verify logical tab order through login form
        
        browser.close()

def test_aria_labels():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        page.goto("http://localhost:8000/dashboard")
        
        # Check for proper ARIA labels
        buttons = page.locator("button")
        for i in range(buttons.count()):
            button = buttons.nth(i)
            aria_label = button.get_attribute("aria-label")
            text_content = button.text_content()
            assert aria_label or text_content, f"Button {i} missing accessible name"
        
        browser.close()
```

### Performance Testing Script

```python
# tests/performance/core_web_vitals.py
import subprocess
import json
import pytest

def test_lighthouse_performance():
    """Test Core Web Vitals compliance using Lighthouse CI"""
    
    # Run Lighthouse
    result = subprocess.run([
        "lighthouse", 
        "http://localhost:8000/dashboard",
        "--output=json",
        "--quiet"
    ], capture_output=True, text=True)
    
    assert result.returncode == 0, f"Lighthouse failed: {result.stderr}"
    
    lighthouse_data = json.loads(result.stdout)
    audits = lighthouse_data["audits"]
    
    # Check LCP
    lcp = audits["largest-contentful-paint"]["numericValue"]
    assert lcp <= 2500, f"LCP {lcp}ms exceeds 2500ms threshold"
    
    # Check CLS
    cls = audits["cumulative-layout-shift"]["numericValue"]
    assert cls < 0.1, f"CLS {cls} exceeds 0.1 threshold"
    
    # Check performance score
    performance_score = lighthouse_data["categories"]["performance"]["score"]
    assert performance_score >= 0.85, f"Performance score {performance_score} below 0.85 threshold"
```

### Security Testing Script

```python
# tests/security/asvs_validation.py
import requests
import pytest

def test_session_cookie_security():
    """Validate OWASP ASVS session management controls"""
    
    # Test SEC-ASVS-S01: Secure cookie attributes
    response = requests.post("http://localhost:8000/auth/login", {
        "username": "test@example.com",
        "password": "testpass123"
    })
    
    cookies = response.cookies
    session_cookie = cookies.get("session")
    
    # Check secure attributes
    assert session_cookie.secure, "Session cookie must have Secure flag"
    assert session_cookie.httponly, "Session cookie must be HttpOnly"
    assert session_cookie.samesite == "Strict", "Session cookie must be SameSite=Strict"

def test_csrf_protection():
    """Validate CSRF token implementation"""
    
    session = requests.Session()
    
    # Get CSRF token
    response = session.get("http://localhost:8000/dashboard")
    assert "csrf_token" in response.text, "CSRF token must be present in forms"
    
    # Test state-changing request without token
    response = session.post("http://localhost:8000/api/device/update", {
        "device_id": "test123",
        "status": "active"
    })
    assert response.status_code == 403, "Request without CSRF token should be rejected"

def test_error_message_security():
    """Validate SEC-ASVS-S06: Error messages don't leak information"""
    
    # Test login with non-existent user
    response = requests.post("http://localhost:8000/auth/login", {
        "username": "nonexistent@example.com", 
        "password": "wrongpass"
    })
    
    error_message = response.json().get("error", "")
    
    # Should not reveal account existence
    assert "user not found" not in error_message.lower()
    assert "invalid credentials" in error_message.lower()
```

---

## Deliverables & Artifacts

### Documentation
- **Personas & Task Models**: Evidence for ISO 9241-210 compliance
- **Annotated Wireframes**: Login, MFA, dashboard, worklist, key forms  
- **Content Style Guide**: Error copy catalog and messaging standards
- **Component Library Spec**: Buttons, inputs, tables, charts with ARIA documentation

### Test Protocols
- **Accessibility Test Suite**: Automated + manual verification scripts
- **Performance Test Suite**: Core Web Vitals monitoring and budgets
- **E2E Test Scripts**: Critical user journey validation
- **Security Test Suite**: ASVS control validation scripts

### Compliance Evidence
- **ASVS Control Map**: Which UX features satisfy which security controls
- **WCAG Conformance Report**: Detailed accessibility compliance documentation
- **Performance Budget Reports**: Core Web Vitals compliance evidence
- **Usability Test Reports**: ISO 9241-210 evaluation evidence

---

## RACI Matrix

| Activity | Product/UX | Engineering | QA | AppSec/Privacy | PMO |
|----------|------------|-------------|----|--------------|----|
| UX Requirements | R/A | C | C | C | I |
| WCAG Conformance | R/A | R | R | C | I |
| Performance Budgets | C | R/A | R | I | I |
| Security Controls | C | R | R | R/A | I |
| CI Gate Implementation | C | R/A | R | C | I |
| Manual Verification | R | C | R/A | C | I |
| Compliance Reporting | I | C | C | C | R/A |
| Standards Updates | R/A | C | C | C | I |

**Legend**: R=Responsible, A=Accountable, C=Consulted, I=Informed

---

## Monitoring & Continuous Improvement

### Real User Monitoring (RUM)
- Core Web Vitals in production
- User journey completion rates
- Error tracking and classification
- Performance regression detection

### Periodic Reviews
- **Monthly**: CI gate effectiveness review
- **Quarterly**: Standards compliance audit  
- **Bi-annually**: Usability testing cycles
- **Annually**: ASVS control review and update

### Improvement Process
1. Identify gaps through monitoring data
2. Update requirements and verification methods
3. Implement enhanced CI gates
4. Validate improvements through testing
5. Document lessons learned and best practices