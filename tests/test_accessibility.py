"""
Basic accessibility tests for WCAG 2.2 AA compliance
UX-REQUIREMENTS: A11Y-WCAG-* validation
"""

import pytest
from fastapi.testclient import TestClient
from app.main import app
from bs4 import BeautifulSoup
import re

client = TestClient(app)

def test_dashboard_accessibility_structure():
    """Test basic accessibility structure requirements"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-NAV01, A11Y-WCAG-NAME01
    response = client.get("/dashboard")
    assert response.status_code == 200
    
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check for skip link
    skip_link = soup.find('a', class_='skip-link')
    assert skip_link is not None, "Skip link must be present for keyboard navigation"
    assert skip_link.get('href') == '#main-content', "Skip link must point to main content"
    
    # Check for main content landmark
    main_content = soup.find(id='main-content')
    assert main_content is not None, "Main content must have id='main-content'"
    assert main_content.get('role') == 'main', "Main content must have role='main'"
    
    # Check for proper heading hierarchy
    headings = soup.find_all(['h1', 'h2', 'h3', 'h4', 'h5', 'h6'])
    assert len(headings) > 0, "Page must have headings for structure"
    
    # Check h1 exists and is unique
    h1_elements = soup.find_all('h1')
    assert len(h1_elements) == 1, "Page must have exactly one h1 element"

def test_form_accessibility():
    """Test form accessibility requirements"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-NAME01, A11Y-WCAG-ERR01
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check all form inputs have labels or aria-label
    inputs = soup.find_all(['input', 'select', 'textarea'])
    for input_elem in inputs:
        input_id = input_elem.get('id')
        aria_label = input_elem.get('aria-label')
        aria_labelledby = input_elem.get('aria-labelledby')
        
        # Check if there's a label pointing to this input
        label = soup.find('label', attrs={'for': input_id}) if input_id else None
        
        assert (label is not None or aria_label is not None or aria_labelledby is not None), \
            f"Input element must have associated label: {input_elem}"

def test_button_accessibility():
    """Test button accessibility requirements"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-NAME01
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    buttons = soup.find_all('button')
    for button in buttons:
        text_content = button.get_text(strip=True)
        aria_label = button.get('aria-label')
        aria_labelledby = button.get('aria-labelledby')
        
        # Remove icon-only content for text check
        text_without_icons = re.sub(r'\s+', ' ', text_content).strip()
        
        assert (text_without_icons or aria_label or aria_labelledby), \
            f"Button must have accessible name: {button}"

def test_aria_live_regions():
    """Test ARIA live regions for dynamic content"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-ARIA02
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check for status announcements live region
    live_region = soup.find(id='status-announcements')
    assert live_region is not None, "Status announcements live region must be present"
    assert live_region.get('aria-live') == 'polite', "Live region must have aria-live='polite'"
    
    # Check KPI values have aria-live for updates
    kpi_values = soup.find_all(id=['device-count', 'avg-latency', 'devices-up', 'devices-down'])
    for kpi in kpi_values:
        assert kpi.get('aria-live') == 'polite', f"KPI value {kpi.get('id')} must have aria-live='polite'"

def test_semantic_structure():
    """Test semantic HTML structure"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-ARIA01
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check for landmark roles
    header = soup.find('header')
    assert header is not None, "Page must have header element"
    assert header.get('role') == 'banner', "Header should have role='banner'"
    
    nav = soup.find('nav')
    assert nav is not None, "Page must have navigation element"
    assert nav.get('aria-label') is not None, "Navigation must have aria-label"
    
    main = soup.find(role='main')
    assert main is not None, "Page must have main landmark"

def test_color_contrast_indicators():
    """Test that color is not the only indicator"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-COLOR01, A11Y-WCAG-ERR02
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check status indicators have both color and text/icons
    status_elements = soup.find_all(class_=['status-up', 'status-down', 'status-warning'])
    for element in status_elements:
        # Should have accompanying text or icon
        has_text = bool(element.get_text(strip=True))
        has_icon = bool(element.find('i')) or bool(element.parent.find('i'))
        
        assert has_text or has_icon, \
            f"Status element must have text or icon, not just color: {element}"

def test_focus_management():
    """Test focus management requirements"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-NAV01
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check interactive elements can receive focus
    interactive_elements = soup.find_all(['button', 'a', 'input', 'select', 'textarea'])
    for element in interactive_elements:
        # Should not have tabindex="-1" unless specifically needed
        tabindex = element.get('tabindex')
        if tabindex:
            assert tabindex != '-1' or element.get('class') and 'sr-only' in ' '.join(element.get('class')), \
                f"Interactive element should be focusable: {element}"

def test_screen_reader_content():
    """Test screen reader specific content"""
    
    # UX-REQUIREMENTS: A11Y-WCAG-NAME01
    response = client.get("/dashboard")
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Check for screen reader only content
    sr_only_elements = soup.find_all(class_='sr-only')
    assert len(sr_only_elements) > 0, "Page should have screen reader only content for context"
    
    # Check aria-hidden on decorative icons
    icons = soup.find_all('i', class_=lambda x: x and 'fa' in x)
    decorative_icons = [icon for icon in icons if icon.get('aria-hidden') == 'true']
    assert len(decorative_icons) > 0, "Decorative icons should have aria-hidden='true'"

if __name__ == "__main__":
    pytest.main([__file__, "-v"])