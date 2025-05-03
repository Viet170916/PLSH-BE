import json
from datetime import datetime
import sys
import os

def json_to_html(json_file_path, output_html_path=None):
    try:
        # Mở file JSON và đọc dữ liệu
        with open(json_file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"❌ Không tìm thấy file: {json_file_path}")
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"❌ Lỗi khi đọc file JSON: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Lỗi không xác định: {e}")
        sys.exit(1)

    # Nếu không chỉ định output path thì tạo tên mới
    if not output_html_path:
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        output_html_path = f"sonarqube-report-{timestamp}.html"

    # Biến lưu trữ số lượng các mức độ severity
    severity_counts = {}

    # Tạo nội dung HTML
    html_content = f"""
    <!DOCTYPE html>
    <html><head><meta charset="utf-8"><title>Security Issues Report</title>
    <style>
        body {{ font-family: Arial; margin: 20px; }}
        .issue {{ border: 1px solid #ccc; padding: 10px; margin: 10px 0; }}
        .blocker {{ border-left: 5px solid red; background-color: #ffeeee; }}
        .critical {{ border-left: 5px solid orange; background-color: #fff3e0; }}
        .major {{ border-left: 5px solid blue; background-color: #e3f2fd; }}
        .minor {{ border-left: 5px solid green; background-color: #e8f5e9; }}
        .info {{ border-left: 5px solid gray; background-color: #f5f5f5; }}
        .summary {{ background-color: #f0f0f0; padding: 10px; margin-bottom: 20px; }}
    </style>
    </head><body>
    <h1>Security Issues Report</h1>
    <div class="summary">
        <p><strong>Report generated:</strong> {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}</p>
        <p><strong>Total Issues:</strong> {data['total']}</p>
        <p><strong>Effort:</strong> {int(data['effortTotal'])//60}h {int(data['effortTotal'])%60}m</p>
    </div>
    """

    # Duyệt qua các vấn đề bảo mật
    for issue in data['issues']:
        sev = issue['severity'].lower()
        severity_counts[sev] = severity_counts.get(sev, 0) + 1

        created = datetime.strptime(issue['creationDate'], '%Y-%m-%dT%H:%M:%S%z').strftime('%Y-%m-%d %H:%M:%S')
        
        html_content += f"""
        <div class="issue {sev}">
            <h3>{issue['severity']}: {issue['message']}</h3>
            <p><strong>Rule:</strong> {issue['rule']}<br>
            <strong>Component:</strong> {issue['component'].split(':')[-1]}<br>
            <strong>Line:</strong> {issue.get('line', 'N/A')}<br>
            <strong>Effort:</strong> {issue['effort']}<br>
            <strong>Created:</strong> {created}<br>
            <strong>Type:</strong> {issue['type']}<br>
            <strong>Tags:</strong> {', '.join(issue.get('tags', []))}</p>
        </div>
        """

    # Thêm tóm tắt mức độ severity
    html_content += "<h2>Severity Summary</h2><ul>"
    for sev, count in sorted(severity_counts.items(), key=lambda x: x[1], reverse=True):
        html_content += f"<li>{sev.upper()}: {count}</li>"
    html_content += "</ul></body></html>"

    # Lưu nội dung HTML vào file
    with open(output_html_path, 'w', encoding='utf-8') as f:
        f.write(html_content)

    print(f"✅ Đã tạo báo cáo: {output_html_path}")
    return output_html_path

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("❌ Usage: python convert_issue_json.py <input.json> [output.html]")
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2] if len(sys.argv) > 2 else None
    
    json_to_html(input_file, output_file)