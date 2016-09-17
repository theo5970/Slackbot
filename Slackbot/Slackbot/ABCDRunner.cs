using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slackbot
{
    public class ABCDRunner
    {
        private static StringBuilder sb = new StringBuilder();
        private static char a, b, c;
        private static Stack<int> pos_stack = new Stack<int>();
        private static Stack<int> stack = new Stack<int>();
        private static int reg = 0;
        private static int pos = 0;
        private static int end = 0;
        public static string Run(string code)
        {
            sb.Clear();
            reg = 0;
            pos = 0;
            a = b = c = (char)0;
            pos_stack.Clear();
            stack.Clear();

            string[] sp = code.Split(' ');
            end = sp.Length - 1;
            for (int i=0; i<sp.Length; i++)
            {
                a = sp[i][0];
                b = sp[i][1];
                c = sp[i][2];
                switch (a)
                {
                    case 'A':
                        solve_a(a, b, c);
                        break;
                    case 'B':
                        solve_b(a, b, c);
                        break;
                }
                pos++;
            }
            return sb.ToString();
        }
        private static void solve_a(char a, char b, char c)
        {
            switch (b)
            {
                case 'A':
                    solve_reg(a, b, c);
                    break;
                case 'B':
                    solve_out(a, b, c);
                    break;
            }
        }
        private static void solve_b(char a, char b, char c)
        {
            switch (b)
            {
                case 'A':
                    solve_loopstack(a, b, c);
                    break;
                case 'B':
                    solve_stackcalc(a, b, c);
                    break;
                case 'C':
                    solve_calc(a, b, c);
                    break;
            }
        }


        private static void solve_reg(char a, char b, char c)
        {
            switch (c)
            {
                case 'A':
                    reg++;
                    break;
                case 'B':
                    reg--;
                    break;
                case 'C':
                    reg = 0;
                    break;
            }
        }
        private static void solve_out(char a, char b, char c)
        {
            switch (c)
            {
                case 'A':
                    sb.Append((char)reg);
                    break;
                case 'B':
                    sb.Append(reg.ToString());
                    break;
                case 'C':
                    sb.Append(' ');
                    break;
                case 'D':
                    sb.AppendLine();
                    break;
            }
        }
        private static void solve_loopstack(char a, char b, char c)
        {
            switch (c)
            {
                case 'A':
                    pos_stack.Push(pos);
                    break;
                case 'B':
                    if (reg == 0)
                    {
                        pos = pos_stack.Pop() - 1;
                    }
                    break;
                case 'C':
                    pos = 0;
                    break;
                case 'D':
                    pos = end;
                    break;
            }
        }
        private static void solve_stackcalc(char a, char b, char c)
        {
            switch (c)
            {
                case 'A':
                    stack.Push(reg);
                    break;
                case 'B':
                    if (stack.Count > 0) reg = stack.Pop();
                    break;
                case 'C':
                    if (stack.Count >= 2)
                    {
                        int v1 = stack.Pop(), v2 = stack.Pop();
                        stack.Push(v2 + v1);
                    }
                    break;
                case 'D':
                    if (stack.Count >= 2)
                    {
                        int v1 = stack.Pop(), v2 = stack.Pop();
                        stack.Push(v2 - v1);
                    }
                    break;
            }
        }
        private static void solve_calc(char a, char b, char c)
        {
            switch (c)
            {
                case 'A':
                    if (stack.Count >= 2)
                    {
                        int v1 = stack.Pop(), v2 = stack.Pop();
                        stack.Push(v2 * v1);
                    }
                    break;
                case 'B':
                    if (stack.Count >= 2)
                    {
                        int v1 = stack.Pop(), v2 = stack.Pop();
                        stack.Push(v2 / v1);
                    }
                    break;
                case 'C':
                    if (stack.Count >= 2)
                    {
                        int v1 = stack.Pop(), v2 = stack.Pop();
                        stack.Push(v2 % v1);
                    }
                    break;
                case 'D':
                    if (stack.Count >= 2)
                    {
                        int v1 = stack.Pop(), v2 = stack.Pop();
                        stack.Push(Convert.ToInt32(Math.Pow(v2, v1)));
                    }
                    break;
            }
        }
    }
}
