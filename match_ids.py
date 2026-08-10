import json
import re

with open('D:/MWMS/emp_dump.json', 'r', encoding='utf-16') as f:
    dump = json.load(f)
    if 'value' in dump: dump = dump['value']

with open('D:/MWMS/FB_cleaned.json', 'r', encoding='utf-8') as f:
    fb = json.load(f)

# Build a searchable list of ZKTeco names
zk_emps = []
for d in dump:
    raw_name = d.get('username', '')
    if raw_name.startswith('EMP-SYNC'):
        raw_name = d.get('lastName', '')
    if raw_name == '.' or not raw_name:
        raw_name = d.get('firstName', '')
        
    if d.get('firstName') == 'Unknown' and d.get('lastName') != '.':
        raw_name = d.get('lastName')
        
    # Build a combined name safely
    fn = d.get('firstName', '') or ''
    ln = d.get('lastName', '') or ''
    combined = f"{fn} {ln}".replace('Unknown', '').replace('.', '').strip()
    if not combined: combined = raw_name.replace('.', '').strip()
    
    clean = re.sub('([a-z])([A-Z])', r'\1 \2', combined).lower()
    
    zk_emps.append({
        'id': d.get('deviceUserId'),
        'search_name': clean,
        'raw_first': str(fn).lower(),
        'raw_last': str(ln).lower(),
        'raw_user': str(d.get('username', '')).lower()
    })

matched_count = 0
for emp in fb:
    full_name = emp['FullName'].lower()
    parts = full_name.split()
    
    best_match = None
    
    for zk in zk_emps:
        if zk['id'] == 0: continue
        
        zk_compact = zk['search_name'].replace(' ', '')
        if len(zk_compact) > 3 and zk_compact in full_name.replace(' ', ''):
            best_match = zk
            break
            
        if len(zk['raw_last']) > 3 and zk['raw_last'].replace('.', '') in full_name.replace(' ', ''):
            best_match = zk
            break
            
        if len(zk['raw_user']) > 3 and not zk['raw_user'].startswith('emp-sync') and zk['raw_user'].replace('.', '') in full_name.replace(' ', ''):
            best_match = zk
            break

    if best_match:
        emp['DeviceUserId'] = best_match['id']
        matched_count += 1
        print(f"Matched: {emp['FullName']} -> ZKTeco ID: {best_match['id']} ({best_match['search_name']})")
    else:
        emp['DeviceUserId'] = 0
        print(f"NO MATCH: {emp['FullName']}")

with open('D:/MWMS/FB_cleaned.json', 'w', encoding='utf-8') as f:
    json.dump(fb, f, ensure_ascii=False, indent=2)

print(f"Matched {matched_count} out of {len(fb)} employees.")
