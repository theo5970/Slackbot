
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slackbot
{
    public class Brainfuck
    {
        public byte[] memory;
        public int pt;
        public bool isRunning;
        public string code;
        public int pos;
        public int loop_count = 0;
        public delegate void BFEventHandler(string message);
        public event BFEventHandler ShutdownRequested;
        public StringBuilder output;
        public Brainfuck()
        {
            output = new StringBuilder();
            memory = new byte[65535];
        }
        public void Init()
        {
            output.Clear();
            Array.Clear(memory, 0, 65535);
            pt = 0;
            isRunning = false;
            code = string.Empty;
            pos = 0;
            loop_count = 0;
        }
        public void Run(string _code)
        {
            Init();
            code = _code;
            isRunning = true;

            while (isRunning && pos < code.Length)
            {
                char c = code[pos];
                int v = 0;
                switch (c)
                {
                    case '+':
                        v = memory[pt];
                        if (v + 1 > 255)
                        {
                            Shutdown("값 오버로드");
                        } else
                        {
                            memory[pt] = (byte)(v + 1);
                        }
                        break;
                    case '-':
                        v = memory[pt];
                        if (v - 1 < 0)
                        {
                            Shutdown("값 오버로드");
                        } else
                        {
                            memory[pt] = (byte)(v - 1);
                        }
                        break;
                    case '>':
                        pt++;
                        if (pt > 65535 - 1)
                        {
                            Shutdown("포인터 오버로드");
                        }
                        break;
                    case '<':
                        pt--;
                        if (pt < 0)
                        {
                            Shutdown("포인터 오버로드");
                        }
                        break;
                    case '.':
                        output.Append((char)memory[pt]);
                        break;
                    case '[':
                        {
                            if (memory[pt] == 0)
                            {
                                if (loop_count++ < 10000)
                                {
                                    int loop = 1;
                                    int t = 0;
                                    while (loop > 0)
                                    {
                                        if (++pos < code.Length - 1)
                                        {
                                            char d = code[pos];
                                            if (c == '[')
                                            {
                                                loop++;
                                            }
                                            else if (c == ']')
                                            {
                                                loop--;
                                            }
                                        }
                                        if (++t > 1000)
                                        {
                                            Shutdown("오류: 무한루프에 빠질뻔.. 휴...");
                                            break;
                                        }
                                    }
                                } else
                                {
                                    Shutdown("오류: 무한루프에 빠질뻔.. 휴...");
                                }
                            }
                        }
                        break;
                    case ']':
                        {
                            if (memory[pt] != 0)
                            {
                                if (loop_count++ < 10000)
                                {
                                    int loop = 1;
                                    int t = 0;
                                    while (loop > 0)
                                    {
                                        if (--pos >= 0)
                                        {
                                            char d = code[pos];
                                            if (d == '[')
                                            {
                                                loop--;
                                            }
                                            else if (d == ']')
                                            {
                                                loop++;
                                            }
                                            if (++t > 1000)
                                            {
                                                Shutdown("오류: 무한루프에 빠질뻔.. 휴...");
                                                break;
                                            }
                                        }
                                    }
                                    pos--;
                                    if (pos < 0)
                                    {
                                        Shutdown("내부적인 포인터 오버로드.");
                                        break;
                                    }
                                } else
                                {
                                    Shutdown("오류: 무한루프에 빠질뻔.. 휴...");
                                }
                            }
                        }
                        break;
                    
                }
                pos++;
            }
            if (isRunning)
            {
                Shutdown("결과: " + output.ToString());
                isRunning = false;
            }
        }
        public void Shutdown(string message)
        {
            ShutdownRequested(message);
            isRunning = false;
        }
    }
}
