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
            table {{ border-collapse: collapse; width: 100%; }}
            th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
            tr:nth-child(even) {{ background-color: #f2f2f2; }}
            .CRITICAL {{ color: red; font-weight: bold; }}
            .HIGH {{ color: orange; }}
        </style>
    </head>
    <body>
        <h1>Trivy Vulnerability Report</h1>
        <p>Generated at: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}</p>
    """

    for result in data.get('Results', []):
        if not result.get('Vulnerabilities'):
            continue
            
        html += f"""
        <h2>Target: {result.get('Target', 'Unknown')}</h2>
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
                <td>{vuln.get('Description', 'No description')[:100]}...</td>
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