using Microsoft.Win32;
using System;

namespace MyHoopJumping
{
    class Program
    {
        static void Main(string[] args)
        {
            // Gathering registry keys...
            Console.WriteLine("Hello Wssorld!");
            RegistryKey basekey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}"); // where network drivers are stored (fsr)
            if (basekey != null)
            {
                string[] subkeys = basekey.GetSubKeyNames();
                RegistryKey[] networkKeys = new RegistryKey[subkeys.Length];

                int i = 0;
                foreach (string netkey in subkeys) 
                {
                    if (int.TryParse(netkey,out _))
                    {
                        networkKeys[i] = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\" + netkey);
                        if (networkKeys[i].GetValue("DriverDesc") != null) 
                        {
                            Console.WriteLine("{0} [{1}]", TruncateRight(networkKeys[i].GetValue("DriverDesc").ToString(), 49, 4).PadRight(49), i + 1);
                            i++;
                        }
                    }
                }
                Console.Write("Select a network driver: ");
                int chosenKey = -20;
                while (true) 
                {
                    if (int.TryParse(Console.ReadLine(), out chosenKey) && chosenKey - 1 <= i && chosenKey > 0) 
                    {
                        chosenKey -= 1;
                        break;
                    }
                }
                Console.Clear();
                Console.WriteLine("Name: {0}", networkKeys[chosenKey].GetValue("DriverDesc"));
                Console.WriteLine("Current MAC: {0}", networkKeys[chosenKey].GetValue("NetworkAddress"));
                if (networkKeys[chosenKey].GetValue("NetworkAddress") == null) 
                {
                    Console.WriteLine("\nWarning!: The device selected has no existing MAC address! Creating one may cause the device to become unstable.");
                }
            }
            Console.ReadKey(true);
        }
        static string TruncateRight(string str, int Maxlength, int dots = 0) 
        {
            string newstr = str.Substring(0, Math.Min(str.Length, Maxlength - dots));
            for (int i = 0; i >= dots; i++) 
            {
                newstr += ".";
            }
            return newstr;
        }
    }
}
