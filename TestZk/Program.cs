using System;
using BiometricZktecoDeviceCommunication;

class Program
{
    static void Main(string[] args)
    {
        var device = new ZktecoDeviceCommunication();
        bool connected = device.Connect("10.10.100.102", 4370);
        Console.WriteLine($"Connected: {connected}");
        
        if (connected)
        {
            var logs = device.GetLogData();
            Console.WriteLine($"Total Logs: {logs.Count}");
            int count2026 = 0;
            foreach (var log in logs)
            {
                if (log.DateTimeRecord.Year == 2026)
                {
                    count2026++;
                }
            }
            Console.WriteLine($"Logs in 2026: {count2026}");
            device.Disconnect();
        }
    }
}
