using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Slackbot
{
    public class AdminManager
    {
        private static List<string> admins = new List<string>();
        public static void Load()
        {
            if (File.Exists("admins.txt"))
            {
                admins = new List<string>(File.ReadAllLines("admins.txt"));
            } else
            {
                File.WriteAllText("admins.txt", "theo5970\r\n");
            }
        }
        public static bool Add(string nickname)
        {
            bool result = true;
            if (admins.Contains(nickname))
            {
                result = false;
            } else
            {
                admins.Add(nickname);
                Save();
            }
            return result;
        }
        public static bool Remove(string nickname)
        {
            bool result = true;
            if (admins.Contains(nickname))
            {
                admins.Remove(nickname);
            } else
            {
                result = false;
            }
            Save();
            return result;
        }
        public static bool Contains(string nickname)
        {
            return admins.Contains(nickname);
        }
        public static void Save()
        {
            File.WriteAllLines("admins.txt", admins.ToArray());
        }
    }
}
