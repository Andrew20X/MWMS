import markdown
from pathlib import Path
import re

# Read markdown file
md_file = r"d:\MWMS\MWMS_Proposal_HR.md"
html_file = r"d:\MWMS\MWMS_Proposal_HR.html"

# Convert markdown to HTML
with open(md_file, 'r', encoding='utf-8') as f:
    md_content = f.read()

# Remove emojis from content (but keep checkmarks ✅ and X ❌ symbols)
emoji_pattern = re.compile("["
    u"\U0001F600-\U0001F64F"  # emoticons
    u"\U0001F300-\U0001F5FF"  # symbols & pictographs
    u"\U0001F680-\U0001F6FF"  # transport & map symbols
    u"\U0001F1E0-\U0001F1FF"  # flags (iOS)
    u"\U00002702-\U00002704"  # dingbats (exclude 2705 checkmark)
    u"\U00002706-\U0000274B"  # dingbats (exclude 274C X)
    u"\U0000274D-\U000027B0"  # dingbats
    u"\U000024C2-\U0001F251"
    u"\U0001f926-\U0001f937"
    u"\U00010000-\U0010ffff"
    u"\u2640-\u2642"
    u"\u2600-\u2B55"
    u"\u200d"
    u"\u23cf"
    u"\u23e9"
    u"\u231a"
    u"\ufe0f"  # dingbats
    u"\u3030"
"]+", flags=re.UNICODE)

md_content = emoji_pattern.sub(r'', md_content)

# Remove "لمن يناسب هذا الخيار؟" sections
md_content = re.sub(r'\*\*لمن يناسب هذا الخيار\?\*\*.*?(?=\n---|\n##|\n###|\Z)', '', md_content, flags=re.DOTALL)

html_content = markdown.markdown(md_content, extensions=['tables', 'extra'])

# Create a complete HTML document
full_html = f"""
<!DOCTYPE html>
<html dir="rtl" lang="ar">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>MWMS Proposal</title>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            line-height: 1.6;
            max-width: 1000px;
            margin: 40px;
            color: #333;
            direction: rtl;
        }}
        h1 {{
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
            font-size: 24px;
        }}
        h2 {{
            color: #34495e;
            margin-top: 30px;
            font-size: 18px;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 20px 0;
            font-size: 13px;
        }}
        th, td {{
            border: 1px solid #bdc3c7;
            padding: 8px;
            text-align: right;
        }}
        th {{
            background-color: #3498db;
            color: white;
        }}
        tr:nth-child(even) {{
            background-color: #ecf0f1;
        }}
        ul {{
            margin: 10px 0;
        }}
        li {{
            margin: 5px 0;
        }}
        .footer {{
            margin-top: 50px;
            padding-top: 20px;
            border-top: 2px solid #3498db;
            text-align: center;
            color: #7f8c8d;
            font-size: 12px;
        }}
        @media print {{
            body {{
                margin: 10px;
            }}
        }}
    </style>
</head>
<body>
{html_content}
<div class="footer">
    <p><strong>تم إعداد هذا النظام بواسطة:</strong></p>
    <p><strong>Andrew Raafat</strong> - Software Engineer & Developer</p>
    <p>المهندس والمطور الوحيد لهذا المشروع</p>
</div>
</body>
</html>
"""

# Write HTML file
with open(html_file, 'w', encoding='utf-8') as f:
    f.write(full_html)

print(f"✓ HTML file created successfully!")
print(f"📄 File saved to: {html_file}")
print(f"\nNext steps:")
print(f"1. Open the HTML file in your browser")
print(f"2. Press Ctrl+P to print")
print(f"3. Select 'Save as PDF'")
print(f"4. Choose the location and click Save")
