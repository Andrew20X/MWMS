import re
import os

txt_file = 'D:/MWMS/Managers and Employees.txt'
sql_file = 'D:/MWMS/fix_all_managers.sql'

with open(txt_file, 'r', encoding='utf-8') as f:
    lines = f.readlines()

updates = []
updates.append("-- Clear all managers first to avoid cycles")
updates.append("UPDATE Employees SET ManagerId = NULL;\n")

# To keep track of the current manager at each depth
manager_stack = {}

def get_name(line_clean):
    match = re.match(r'^(.*?)\s*\((.*?)\)$', line_clean)
    if match:
        name_part = match.group(1).strip()
    else:
        name_part = line_clean.strip()
        if "Accountant" in name_part and name_part != "Accountant":
            name_part = "Accountant"
            
    search_name = name_part
    if name_part == 'Fatma Abdelgawad': search_name = 'Fatma'
    elif name_part == 'Gaber Ammar': search_name = 'Gaber'
    elif name_part == 'Amr Mabrouk': search_name = 'Amr Mabrouk'
    return search_name.replace("'", "''")

for line in lines:
    line_raw = line.rstrip('\n')
    if not line_raw.strip():
        continue
    
    # Calculate depth based on prefix length
    # A standard prefix like "├── " is 4 characters
    # "│   ├── " is 8 characters
    match = re.match(r'^([├└│─\s]*)', line_raw)
    prefix = match.group(1)
    depth = len(prefix) // 4
    
    line_clean = line_raw[len(prefix):].strip()
    if not line_clean:
        continue
        
    search_name = get_name(line_clean)
    
    # Keep track of this person as a potential manager for the NEXT depth
    manager_stack[depth] = search_name
    
    if depth > 0:
        # The manager is the person at depth - 1
        manager_name = manager_stack.get(depth - 1)
        if manager_name:
            updates.append(f"""
-- Manager for {search_name} is {manager_name}
DECLARE @MgrId_{len(updates)} INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%{manager_name}%');
IF @MgrId_{len(updates)} IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_{len(updates)}
    WHERE FirstName + ' ' + LastName LIKE '%{search_name}%';
END
""")

with open(sql_file, 'w', encoding='utf-8') as f:
    f.write("\n".join(updates))

print(f"Generated {sql_file}")
