import sys
from zk import ZK
import json

ip = '10.10.100.102'
port = 4370

zk = ZK(ip, port=port, timeout=10, password=0, force_udp=False, ommit_ping=False)
try:
    conn = zk.connect()
    print('Connected successfully')
    
    # Get attendances
    attendances = conn.get_attendance()
    print(f'Total records: {len(attendances)}')
    
    # Filter for July
    july_records = []
    for att in attendances:
        if att.timestamp.year == 2026 and att.timestamp.month == 7:
            july_records.append({
                'deviceUserId': str(att.user_id),
                'recordTime': att.timestamp.strftime('%Y-%m-%dT%H:%M:%S.000Z')
            })
            
    print(f'July 2026 records: {len(july_records)}')
    if len(july_records) > 0:
        print("Sample:", july_records[:5])
        
    with open('july_logs.json', 'w') as f:
        json.dump(july_records, f)
        
except Exception as e:
    print(f"Error: {e}")
finally:
    if 'conn' in locals():
        conn.disconnect()
