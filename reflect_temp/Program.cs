using System;
using System.Reflection;

class Program {
    static void Main() {
        var asm = Assembly.LoadFile(@"C:\Users\Andre\.nuget\packages\biometriczktecodevicecommunication\1.0.0\lib\netcoreapp2.1\BiometricZktecoDeviceCommunication.dll");
        var t = asm.GetType("Attendance_ZKTeco_Service.Models.DeviceManipulator");
        foreach(var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            Console.Write(m.Name + "(");
            var prms = m.GetParameters();
            for(int i=0; i<prms.Length; i++) {
                Console.Write(prms[i].ParameterType.Name + " " + prms[i].Name);
                if (i < prms.Length -1) Console.Write(", ");
            }
            Console.WriteLine(") -> " + m.ReturnType.Name);
        }
    }
}
