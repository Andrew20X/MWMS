using System;
using System.Reflection;

class Program {
    static void Main() {
        var asm = Assembly.LoadFile(@"C:\Users\Andre\.nuget\packages\biometriczktecodevicecommunication\1.0.0\lib\netcoreapp2.1\BiometricZktecoDeviceCommunication.dll");
        foreach(var t in asm.GetExportedTypes()) {
            Console.WriteLine(t.FullName);
            foreach(var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                Console.WriteLine("  " + m.Name);
            }
        }
    }
}
