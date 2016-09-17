using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Slackbot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Slackbot";
            string[] info = File.ReadAllLines("settings.txt");
            Bot bot = new Bot(6667, info[0], info[1], info[2], info[3]);
            bot.Connect();
            Console.ReadLine();
        }
    }
}
