

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Slackbot
{
    public class CSharpRunner
    {
        public static Process process;
        public delegate void RunnerHandler(string output);
        public static event RunnerHandler OutputData;
        public static Process prc;
        private static ProcessStartInfo startInfo;
        public static List<string> filter_list = new List<string>();

        public static void Load()
        {
            filter_list = new List<string>(File.ReadAllLines("cs_filter.txt"));
        }
        public static void Init()
        {
            startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\Program Files (x86)\MSBuild\14.0\Bin\csi.exe";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
        }
        public static void Start()
        {
            Load();
            Init();
            prc = new Process();
            prc.EnableRaisingEvents = false;
            prc.StartInfo = startInfo;
            prc.Start();
            prc.BeginErrorReadLine();
            prc.BeginOutputReadLine();
            prc.OutputDataReceived += Prc_OutputDataReceived;
        }
        public static void Input(string line)
        {
            prc.StandardInput.WriteLine(line);
        }
        private static void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputData(e.Data);
        }
    }
}
