import sys
import json
from zk import ZK

def main():
    ip = sys.argv[1] if len(sys.argv) > 1 else '10.10.100.102'
    port = int(sys.argv[2]) if len(sys.argv) > 2 else 4370
    output_path = sys.argv[3] if len(sys.argv) > 3 else 'zk_logs.json'
    users_output_path = sys.argv[4] if len(sys.argv) > 4 else 'zk_users.json'

    zk = ZK(ip, port=port, timeout=10, password=0, force_udp=False, ommit_ping=False)
    try:
        conn = zk.connect()
        
        # Get users
        try:
            users = conn.get_users()
            users_list = []
            for u in users:
                users_list.append({
                    'uid': u.uid,
                    'userId': str(u.user_id),
                    'name': u.name,
                    'role': u.privilege
                })
            with open(users_output_path, 'w') as f:
                json.dump(users_list, f)
        except Exception as e:
            print(f"Error fetching users: {e}")
            with open(users_output_path, 'w') as f:
                json.dump([], f)
                
        # Get attendances
        attendances = conn.get_attendance()
        logs_list = []
        for att in attendances:
            logs_list.append({
                'deviceUserId': str(att.user_id),
                'recordTime': att.timestamp.strftime('%Y-%m-%dT%H:%M:%S')
            })
            
        with open(output_path, 'w') as f:
            json.dump(logs_list, f)
            
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)
    finally:
        if 'conn' in locals():
            conn.disconnect()

if __name__ == '__main__':
    main()
