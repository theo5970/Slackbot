using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;


namespace Slackbot
{
    public class ScriptOutputStream : Stream
    {
        #region Fields
        public delegate void StreamEventHandler(string line);
        public event StreamEventHandler StreamOutput;
        #endregion
        #region Constructors
        public ScriptOutputStream()
        {

        }
        #endregion
        #region Properties
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        #endregion
        #region Exposed Members
        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public StringBuilder sb = new StringBuilder();
        public override void Write(byte[] buffer, int offset, int count)
        {
            string str = Encoding.Unicode.GetString(buffer, offset, count);
            sb.Append(str);
            if (str.StartsWith(Environment.NewLine))
            {
                string line = sb.ToString();
                if (line.StartsWith("?"))
                {
                    line = line.Remove(0, 1);
                }
                line = line.Replace(Environment.NewLine, string.Empty);
                StreamOutput(line);
                sb.Clear();

            }
        }
        #endregion
    }
    public class PythonRunner
    {
        private static ScriptEngine engine;
        private static ScriptScope scope;
        private static Dictionary<string, StringBuilder> codes;
        public delegate void PyEventHandler(string data);
        public static event PyEventHandler Print;
        public static void Init()
        {
            if (codes == null) codes = new Dictionary<string, StringBuilder>();
            engine = Python.CreateEngine();
            scope = engine.CreateScope();
            ScriptOutputStream stream = new ScriptOutputStream();
            stream.StreamOutput += Stream_StreamOutput;
            engine.Runtime.IO.SetOutput(stream, Encoding.Unicode);
            codes.Clear();
        }

        // 스트림에서 출력 이벤트가 있으면 이 클래스의 이벤트로 넘기기
        private static void Stream_StreamOutput(string line)
        {
            Print(line);
        }

        // 코드 추가 (닉네임, 코드)
        public static void Add(string nickname, string code)
        {
            code = code.Replace("\t", "    ");
            code = code.Replace(@"\t", "    ");
            if (!codes.ContainsKey(nickname))
            {
                codes.Add(nickname, new StringBuilder());
            }
            codes[nickname].AppendLine(code);
        }

        // 변수 리셋
        public static void Reset()
        {
            for (int i = 0; i < scope.GetVariableNames().Count(); i++)
            {
                scope.RemoveVariable(scope.GetVariableNames().ElementAt(i));
            }
        }

        // 코드 초기화 (닉네임)
        public static void Clear(string nickname)
        {
            codes[nickname].Clear();
        }
        
        // 실행 (닉네임)
        public static void Run(string nickname)
        {
            try
            {
                engine.Execute(codes[nickname].ToString(), scope);
            }
            catch (Exception ex)
            {
                // 오류 발생하면 코드 클리어하고 오류 출력
                Print(ex.Message);
                Clear(nickname);
            }
        }
    }
}
