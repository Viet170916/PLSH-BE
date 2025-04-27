import json
import sys
from datetime import datetime

def convert_to_html(input_file, output_file):
    with open(input_file) as f:
        data = json.load(f)

    html = f"""
    <html>
    <head>
        <title>Trivy Report - {datetime.now().strftime('%Y-%m-%d')}</title>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            table {{ border-collapse: collapse; width: 100%; margin-bottom: 30px; }}
            th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
            tr:nth-child(even) {{ background-color: #f9f9f9; }}
            th {{ background-color: #f2f2f2; }}
            .CRITICAL {{ color: red; font-weight: bold; }}
            .HIGH {{ color: orange; font-weight: bold; }}
            .MEDIUM {{ color: #e69500; }}
            .LOW {{ color: green; }}
            .UNKNOWN {{ color: gray; }}
        </style>
    </head>
    <body>
        <h1>🛡️ Trivy Full Vulnerability Report</h1>
        <p><strong>Generated at:</strong> {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}</p>
    """

    for result in data.get('Results', []):
        target = result.get('Target', 'Unknown Target')

        # Vulnerabilities section
        if result.get('Vulnerabilities'):
            html += f"<h2>📦 Vulnerabilities - {target}</h2>"
            html += """
            <table>
                <tr>
                    <th>Vulnerability ID</th>
                    <th>Severity</th>
                    <th>Package</th>
                    <th>Installed Version</th>
                    <th>Fixed Version</th>
                    <th>Description</th>
                </tr>
            """
            for vuln in result['Vulnerabilities']:
                html += f"""
                <tr>
                    <td>{vuln.get('VulnerabilityID', 'N/A')}</td>
                    <td class="{vuln.get('Severity', 'UNKNOWN')}">{vuln.get('Severity', 'UNKNOWN')}</td>
                    <td>{vuln.get('PkgName', 'N/A')}</td>
                    <td>{vuln.get('InstalledVersion', 'N/A')}</td>
                    <td>{vuln.get('FixedVersion', 'Not fixed')}</td>
                    <td>{vuln.get('Description', 'No description')[:120]}...</td>
                </tr>
                """
            html += "</table>"

        # Secrets section
        if result.get('Secrets'):
            html += f"<h2>🔐 Secrets - {target}</h2>"
            html += """
            <table>
                <tr>
                    <th>Rule ID</th>
                    <th>Severity</th>
                    <th>Category</th>
                    <th>Title</th>
                </tr>
            """
            for secret in result['Secrets']:
                html += f"""
                <tr>
                    <td>{secret.get('RuleID', 'N/A')}</td>
                    <td class="{secret.get('Severity', 'UNKNOWN')}">{secret.get('Severity', 'UNKNOWN')}</td>
                    <td>{secret.get('Category', 'N/A')}</td>
                    <td>{secret.get('Title', 'N/A')}</td>
                </tr>
                """
            html += "</table>"

    html += "</body></html>"

    with open(output_file, 'w') as f:
        f.write(html)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python convert_json.py <input.json> <output.html>")
        sys.exit(1)
    convert_to_html(sys.argv[1], sys.argv[2])
