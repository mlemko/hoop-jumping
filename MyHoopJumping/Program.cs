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
                        i++;
                    }
                }
            }
        }
        static string TruncateRight(string str, int Maxlength) 
        {
            return str.Substring(0, Math.Min(str.Length, Maxlength));
        }
    }
}
