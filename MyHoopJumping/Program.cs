using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;

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
                List<RegistryKey> networkKeys = new List<RegistryKey>();

                foreach (string netkey in subkeys)
                {
                    if (int.TryParse(netkey, out _))
                    {
                        RegistryKey registry = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\" + netkey);
                        if (registry.GetValue("DriverDesc") != null)
                        {
                            networkKeys.Add(registry);
                        }
                    }
                }
                while (true)
                {
                select:
                    Console.Clear();
                    for (int i = 0; i < networkKeys.Count; i++)
                    {
                        Console.WriteLine("{0} [{1}]", TruncateRight(networkKeys[i].GetValue("DriverDesc").ToString(), 49, 3).PadRight(49), i + 1);
                    }
                    Console.Write("Select a network driver: ");
                    int chosenKey = -20;
                    while (true)
                    {
                        if (int.TryParse(Console.ReadLine(), out chosenKey) && chosenKey - 1 <= networkKeys.Count && chosenKey > 0)
                        {
                            chosenKey -= 1;
                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Name: {0}", networkKeys[chosenKey].GetValue("DriverDesc"));
                                Console.WriteLine("Current MAC: {0}", networkKeys[chosenKey].GetValue("NetworkAddress"));
                                if (networkKeys[chosenKey].GetValue("NetworkAddress") == null)
                                {
                                    Console.WriteLine("\nWarning!: The device selected has no existing MAC address! Creating one may cause the device to become unstable.");
                                }
                                Console.WriteLine("\nWould you like to change/add a MAC address? (y/n)");

                                switch (YesOrNo(Console.ReadLine()))
                                {
                                    case true:
                                        MACRandomizer(networkKeys[chosenKey]);
                                        break;
                                    case false:
                                        goto select;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                }
            }
            Console.ReadKey(true);
        }
        static int MACRandomizer(RegistryKey networkKeyRead)
        {
            MACRandomSettings settings = new MACRandomSettings();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Name: {0}", networkKeyRead.GetValue("DriverDesc"));
                Console.WriteLine("Current MAC: {0}", networkKeyRead.GetValue("NetworkAddress"));
                Console.WriteLine();
                Console.WriteLine("Current settings:");
                settings.DisplaySettings();
                Console.WriteLine("[Q] Quit");
                Console.WriteLine("[R] Randomize Now!");
                Console.WriteLine("[S] Settings");
                Console.WriteLine();
                switch (Console.ReadLine().Trim().ToUpper())
                {
                    case "Q":
                        return 0;
                    case "R":
                        ActuallyRandomize(networkKeyRead, settings);
                        break;
                    case "S":
                        settings.ChangeSettings();
                        break;
                    default:
                        break;
                }
            }
        }
        static int ActuallyRandomize(RegistryKey networkKeyRead, MACRandomSettings settings)
        {
            Random random = new Random();
            int genlength = 12;
            string newadress = "";
            if (settings.LeftAppend != null)
            {
                genlength -= settings.LeftAppend.Length;
                newadress += settings.LeftAppend;
            }
            if (settings.RightAppend != null)
            {
                genlength -= settings.RightAppend.Length;
            }
            // theres probably a better way to make this but im so tired mann.
            List<int> disabled = new List<int>();
            foreach (char ch in settings.CharDisable)
            {
                disabled.Add(Convert.ToInt32(ch.ToString().ToUpper(), 16));
            }
            for (int i = 0; i < genlength;)
            {
                int rand = random.Next(0, 16);
                if (!disabled.Contains(rand))
                {
                    newadress += Convert.ToString(rand, 16);
                    i++;
                }

            }
            newadress += settings.RightAppend;
            RegistryKey networkKeyWrite = Registry.LocalMachine.CreateSubKey(networkKeyRead.Name[19..]);
            networkKeyWrite.SetValue("NetworkAddress", newadress.ToUpper());
            networkKeyWrite.Close();
            return 0;
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
        static bool? YesOrNo(string ans) // literally is just a method for converting y/n to true/false cause i'm lazy.
        {
            ans = ans.ToLower();
            ans = ans.Trim();
            switch (ans)
            {
                case "y":
                    return true;
                case "n":
                    return false;
                case "yes":
                    return true;
                case "no":
                    return false;
                default:
                    return null;
            }
        }
    }
    public class MACRandomSettings
    {

        /// <summary>
        /// Appended on the left side of the MAC address.
        /// </summary>
        public string LeftAppend;
        /// <summary>
        /// Appended on the right side of the MAC address.
        /// </summary>
        public string RightAppend;
        /// <summary>
        /// List of disabled characters.
        /// </summary>
        public List<char> CharDisable;

        /// <summary>
        /// Creates an empty settings object.
        /// </summary>
        public MACRandomSettings()
        {
            LeftAppend = null;
            RightAppend = null;
            CharDisable = new List<char>();
        }
        public MACRandomSettings(string left, string right, List<char> disable)
        {
            LeftAppend = left;
            RightAppend = right;
            CharDisable = disable;
        }
        public int AddChar(char added)
        {
            if (CharDisable.Contains(added))
            {
                return 1;
            }
            else
            {
                CharDisable.Add(added);
                CharDisable.Sort();
                return 0;
            }
        }
        public int DisplaySettings()
        {
            if (LeftAppend == null)
            {
                Console.WriteLine("Required on left: None");
            }
            else
            {
                Console.WriteLine("Required on left: {0}", LeftAppend);
            }
            if (RightAppend == null)
            {
                Console.WriteLine("Required on right: None");
            }
            else
            {
                Console.WriteLine("Required on right: {0}", RightAppend);
            }
            Console.Write("Disabled characters: [");
            bool containschar = false;
            foreach (char ch in CharDisable)
            {
                if (ch != '\0')
                {
                    Console.Write("{0}, ", ch);
                    containschar = true;
                }
            }
            if (containschar) { Console.CursorLeft -= 2; }
            Console.WriteLine("]");
            return 0;
        }
        public int ChangeSettings()
        {
            while (true)
            {
                Console.Clear();
                System.Console.WriteLine("Current settings:");
                this.DisplaySettings();
                Console.WriteLine("[Q] Go Back");
                Console.WriteLine("[L] Change left");
                Console.WriteLine("[R] Change right");
                System.Console.WriteLine("[D] Disabled characters");
                System.Console.WriteLine();
                switch (Console.ReadLine().Trim().ToUpper())
                {
                    case "Q":
                        return 0;
                    case "R":
                        System.Console.WriteLine("Please type new string (q to cancel):");
                        string inpot = Console.ReadLine().Trim().ToUpper();
                        if (inpot != "Q")
                        {
                            this.RightAppend = inpot;
                        }
                        break;
                    case "L":
                        System.Console.WriteLine("Please type new string (q to cancel):");
                        string inpat = Console.ReadLine().Trim().ToUpper();
                        if (inpat != "Q")
                        {
                            this.LeftAppend = inpat;
                        }
                        break;
                    case "D":
                        break;
                    default:
                        break;
                }
            }
        }
        int RestrictCharacters()
        {
            while (true)
            {
                Console.Clear();
                this.DisplaySettings();
                System.Console.WriteLine("Would you like to [A]dd, [R]emove or r[E]set characters?");
                switch (Console.ReadLine().Trim().ToUpper())
                {
                    case "A":
                        while (true)
                        {
                            Console.Clear();
                            this.DisplaySettings();
                            System.Console.WriteLine("Type characters to add (Q to exit):");
                            
                        }
                        break;
                    case "R":
                        break;
                    case "E":
                        break;
                    default:
                        return 0;
                        break;
                }
            }
        }
    }
}
