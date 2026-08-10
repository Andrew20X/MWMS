import json
from thefuzz import fuzz
from thefuzz import process
import re

with open('D:/MWMS/emp_dump.json', 'r', encoding='utf-16') as f:
    dump = json.load(f)
    if 'value' in dump: dump = dump['value']

with open('D:/MWMS/FB_cleaned.json', 'r', encoding='utf-8') as f:
    fb = json.load(f)

# Extract ZKTeco names for fuzzy matching
zk_names = {}
for d in dump:
    id = d.get('deviceUserId', 0)
    if id == 0: continue
    
    raw_user = d.get('username', '')
    if raw_user.startswith('EMP-SYNC'):
        raw_user = d.get('lastName', '')
        
    fn = d.get('firstName', '') or ''
    ln = d.get('lastName', '') or ''
    
    if fn == 'Unknown' and ln != '.':
        combined = ln
    elif raw_user and raw_user != '.':
        combined = raw_user
    else:
        combined = f"{fn} {ln}".replace('Unknown', '').replace('.', '').strip()
        
    # Split camel case and dots
    combined = combined.replace('.', ' ')
    clean = re.sub('([a-z])([A-Z])', r'\1 \2', combined).lower().strip()
    
    zk_names[id] = clean

matched_ids = {}

print("Matching...")
for emp in fb:
    full_name = emp['FullName'].lower()
    
    best_id = 0
    best_score = 0
    best_match_str = ""
    
    for id, zk_name in zk_names.items():
        # First try token set ratio (good when strings have different lengths but share words)
        score = fuzz.token_set_ratio(full_name, zk_name)
        
        # Exact substring match without spaces gets huge boost
        if zk_name.replace(' ', '') in full_name.replace(' ', '') and len(zk_name.replace(' ', '')) > 4:
            score += 30
            
        if score > best_score:
            best_score = score
            best_id = id
            best_match_str = zk_name
            
    if best_score >= 60:
        matched_ids[emp['FullName']] = best_id
        print(f"[SCORE: {best_score}] {emp['FullName']}  =>  [{best_id}] {best_match_str}")
    else:
        matched_ids[emp['FullName']] = 0
        print(f"[FAIL: {best_score}] {emp['FullName']}")

with open('D:/MWMS/matched_ids.json', 'w', encoding='utf-8') as f:
    json.dump(matched_ids, f, ensure_ascii=False, indent=2)

print(f"Generated matched_ids.json")
