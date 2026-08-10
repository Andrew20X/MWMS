import json

with open('D:/MWMS/emp_dump.json', 'r', encoding='utf-16') as f:
    dump = json.load(f)
    if 'value' in dump: dump = dump['value']

lines = []
for d in dump:
    fp_id = d.get('deviceUserId')
    fname = d.get('firstName', '')
    lname = d.get('lastName', '')
    uname = d.get('username', '')
    lines.append(f"ID: {fp_id} | First: {fname} | Last: {lname} | User: {uname}")

with open('D:/MWMS/fingerprint_list.txt', 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines))
