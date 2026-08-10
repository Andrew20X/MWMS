import os
import re

txt_file = 'D:/MWMS/Managers and Employees.txt'
sql_file = 'D:/MWMS/fix_all_positions.sql'

with open(txt_file, 'r', encoding='utf-8') as f:
    lines = f.readlines()

updates = []

# Parse lines and generate SQL
for line in lines:
    line = line.strip()
    if not line:
        continue
    
    # Remove drawing characters
    cleaned = re.sub(r'^[├└│─\s]+', '', line)
    
    # Extract name and position
    match = re.match(r'^(.*?)\s*\((.*?)\)$', cleaned)
    if match:
        name_part = match.group(1).strip()
        position = match.group(2).strip()
    else:
        # If there's no position in parentheses, the name and position are the same
        name_part = cleaned.strip()
        position = 'Employee' # fallback
        if "Team" in name_part or name_part in ["Security", "Drivers", "Couriers", "Instrumentation and Control Engineers"]:
            position = 'Employee'
        elif "Manager" in name_part:
            position = name_part
        elif "Accountant" in name_part:
            name_part = "Accountant"
            position = "Accountant"
        elif "Senior Treasury" in name_part:
            position = "Employee"
            
    # Some names need special mapping because of how they exist in the DB
    search_name = name_part
    if name_part == 'Fatma Abdelgawad': search_name = 'Fatma'
    elif name_part == 'Gaber Ammar': search_name = 'Gaber'
    elif name_part == 'Amr Mabrouk': search_name = 'Amr Mabrouk'
    
    # Escape quotes
    search_name = search_name.replace("'", "''")
    position = position.replace("'", "''")
    
    # Generate SQL
    updates.append(f"""
-- Update for {name_part}
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = '{position}') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('{position}', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = '{position}')
WHERE FirstName + ' ' + LastName LIKE '%{search_name}%';
""")

with open(sql_file, 'w', encoding='utf-8') as f:
    f.write("\n".join(updates))

print(f"Generated {sql_file}")
