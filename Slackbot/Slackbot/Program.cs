/*
 * 작성자: theo5970
 * 만든 날짜: 2016-09-18
 * 문의: blog.naver.com/theo5970 또는 theo5970@naver.com
 * 라이선스: MIT Lincese
 * -
 * 봇 코드 참고: phillyai > SlackBot
 */

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
