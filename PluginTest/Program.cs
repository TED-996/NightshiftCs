using System;

namespace PluginTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("So far, so good.");
            IntPtr ptr = IntPtr.Zero;
            NightshiftPlugin.Plugin.Initialize(ref ptr, IntPtr.Zero);
        }
    }
}
