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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slackbot
{
    public class ShowHelp
    {
        public static List<string> helps;
        public static int page_count;

        public delegate void HelpHandler(string ch, string msg);

        public static event HelpHandler HelpShowed;

        /// <summary>
        /// 도움말 페이지 파일을 불러옵니다.
        /// </summary>
        public static void Load()
        {
            helps = new List<string>(File.ReadAllLines("help.txt"));
            page_count = helps.Count / 5 + 1;
        }

        /// <summary>
        /// 도움말을 페이지마다 나눠서 보여줍니다.
        /// </summary>
        /// <param name="channel">메시지를 받을 채널</param>
        /// <param name="index">쪽수</param>
        public static void Show(string channel, int index)
        {
            if (index <= page_count)
            {
                int start = (index - 1) * 5;
                int end = start + 5;
                if (index == page_count)
                {
                    end = helps.Count;
                }
                HelpShowed(channel, string.Format("--- theo5970_bot Commands ({0}/{1}) ---", index, page_count));
                for (int i = start; i < end; i++)
                {
                    HelpShowed(channel, string.Format("!{0}", helps[i]));
                }
                if (index != page_count)
                {
                    HelpShowed(channel, string.Format("!commands {0}를 하시면 다음 페이지로 넘어갑니다.", index + 1));
                }
            }
            else
            {
                HelpShowed(channel, "그 페이지는 없습니다. !commands [page]");
            }
        }
    }

    public class Bot : IDisposable
    {
        #region 속성
        // 포트
        private int port;

        // 호스트, 닉네임, 채널 이름, 비밀번호
        private string host, nickname, channel, password;

        // 저장된 거 (!save 했을 때)
        private string saved = "";

        private NetworkStream stream;                       //네트워크 스트림
        private TcpClient client;                           //TCP 클라이언트
        private StreamReader reader;                        //읽기 스트림
        private StreamWriter writer;                        //쓰기 스트림
        private Thread updateThread;                        //업데이트 스레드
        private Thread timeThread;                          //시간 알려주는 스레드
        private SslStream _ssl;                             //SSL 보안 스트림
        private Brainfuck brainfuck;                        //브레인퍽 인터프리터
        private Stopwatch stopwatch = new Stopwatch();      //스톱워치
        private static Random random = new Random();        //랜덤
        public List<string> banlist = new List<string>();   //벤 리스트
        #endregion

        public Bot(int port, string host, string nickname, string channel, string password)
        {
            this.port = port;
            this.nickname = nickname;
            this.host = host;
            this.channel = channel;
            this.password = password;
        }

        /// <summary>
        /// IRC에 연결합니다.
        /// </summary>
        public void Connect()
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            _ssl = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateCert));
            _ssl.AuthenticateAsClient("Slack");

            reader = new StreamReader(_ssl);
            writer = new StreamWriter(_ssl);

            // 정보를 서버에 보내기
            send_string(string.Format("PASS {0}", password));
            send_string(string.Format("NICK {0}", nickname));
            send_string("JOIN BOT");

            // 스레드 생성 & 시작
            updateThread = new Thread(new ThreadStart(update_target));
            timeThread = new Thread(new ThreadStart(time_target));
            updateThread.Start();
            timeThread.Start();

            // C# 인터프리터 출력 이벤트 설정
            CSharpRunner.OutputData += CSharpRunner_OutputData;
            // 관리자 목록 불러오기
            AdminManager.Load();
            // 파일 모니터링 시작
            FileMonitor.Start();

            // 파이썬 인터프리터 초기화 & 설정
            PythonRunner.Init();
            PythonRunner.Print += PythonRunner_Print;

            // 도움말 로딩
            ShowHelp.Load();
            ShowHelp.HelpShowed += ShowHelp_HelpShowed;

            bool quit = false; // 종료 여부
            while (!quit)
            {
                string readline = Console.ReadLine();   // 콘솔 창에서 입력 받아고기
                if (readline != "!quit")
                {
                    // IRC에 보낸다.
                    SendMessage(readline);
                }
                else
                {
                    // 종료하기
                    quit = true;
                    SendMessage(channel, "theo5970_bot 봇을 종료합니다.");
                    // 스레드 중단
                    updateThread.Abort();
                    timeThread.Abort();
                    // 자기 자신의 프로세스를 죽이기
                    Process.GetCurrentProcess().Kill();
                    break;
                }
            }
        }

        // 시간 스레드
        private void time_target()
        {
            DateTime dt;
            while (true)
            {
                dt = DateTime.Now;
                if (dt.Minute == 0 && dt.Second == 0)
                {
                    string message = string.Format("현재 {0}시가 되었음을 알려드립니다. ", dt.Hour);
                    if (dt.Hour >= 0 && dt.Hour <= 5)
                    {
                        message += "안 주무실건가요?";
                    }
                    SendMessage("general", message);
                    SendMessage(message);
                }
                Thread.Sleep(1000);
            }
        }

        private static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        // 업데이트 스레드
        private void update_target()
        {
            string readline = reader.ReadLine();
            while (true)
            {
                if (readline == null)   // 받아들여지는 데이터가 없으면 건너뛰기
                {
                    return;
                }
                if (readline.StartsWith("PING"))    // PING 요청이 오면 PONG이라고 바꿔서 응답하기
                {
                    send_string(readline.Replace("PING", "PONG"));
                }
                if (readline[0] == ':' && readline.Split(' ')[1] == "PRIVMSG")  // 채팅 이벤트
                {
                    // 닉네임
                    string nickname = readline.Split(':')[1].Split('!')[0];
                    // 채널 이름
                    string channel = readline.Split(' ')[2];
                    // 메시지
                    string message = readline.Substring(readline.Split(':')[0].Length + readline.Split(':')[1].Length + 2);

                    // 색상 바꾸기
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[{0}] ", channel);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("{0}: ", nickname);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    // 벤 리스트에 포함 안된다면 받아주고 아니면 무시하기
                    if (!banlist.Contains(nickname))
                    {
                        // 명령어 받아주기
                        if (message.StartsWith("!"))
                        {
                            Task.Run(() => process_command(nickname, channel, message));
                        }
                        // daminbot 감지하고 짜증내기
                        if (message.Contains("코딩노예 봇 시작"))
                        {
                            SendMessage(channel, "아이씨 왜 들어온겨. 나 삐질거임. <<퍽");
                        }
                        // 신변 보호하기...
                        if (message.Contains("태환") || message.Contains("태 환") || message.ToLower().Contains("taehwan") || message.ToLower().Contains("tae hwan"))
                        {
                            SendMessage(channel, "어디서 제 이름을 함부로 부르시는지요?");
                        }
                    }
                }
                // 채널 요청에 응답하기
                if (readline.Split(' ')[1] == "001")
                {
                    send_string(string.Format("MODE {0}", nickname));
                    send_string(string.Format("JOIN {0}", channel));
                    HelloMessage();

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("* Joined Channel : {0}", channel);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    CSharpRunner.Start();
                }
                readline = reader.ReadLine();
            }
        }

        public async void HelloMessage()
        {
            await Task.Delay(10);   // 10 밀리초 대기
            SendMessage(channel, "theo5970_bot이 켜졌습니다! 안녕하세요?");
        }

        /// <summary>
        /// 설정된 채널에 메시지를 보냅니다.
        /// </summary>
        /// <param name="message">메시지</param>
        public void SendMessage(string message)
        {
            if (message.Length < 480)
            {
                send_string(string.Format("PRIVMSG {0} : {1}", channel, message));
            }
            else
            {
                SendMessage("문자열이 너무 깁니다.");
            }
        }

        /// <summary>
        /// 지정된 채널에 메시지를 보냅니다.
        /// </summary>
        /// <param name="_channel">지정할 채널</param>
        /// <param name="message">메시지</param>
        public void SendMessage(string _channel, string message)
        {
            if (message.Length < 480)
            {
                send_string(string.Format("PRIVMSG {0} : {1}", _channel, message));
            }
            else
            {
                SendMessage(_channel, "문자열이 너무 깁니다.");
            }
        }

        /// <summary>
        /// 내부적으로 스트림에 데이터를 씁니다.
        /// </summary>
        /// <param name="data">데이터 문자열</param>
        private void send_string(string data)
        {
            writer.WriteLine(data);
            writer.Flush();
        }

        /// <summary>
        /// 문자열을 지정된 크기만큼 나눕니다.
        /// </summary>
        /// <param name="str">입력 문자열</param>
        /// <param name="chunkSize">기준 크기</param>
        /// <returns>나눠진 문자열의 배열</returns>
        private IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        private static string bf_ch = "";   //브레인퍽 임시채널
        private static string cs_ch = "";   //C# 임시채널
        private static string py_ch = "";   //파이썬 임시채널

        /// <summary>
        /// 명령어를 처리합니다.
        /// </summary>
        /// <param name="nickname">닉네임</param>
        /// <param name="channel">채널 이름</param>
        /// <param name="message">메시지</param>
        private async void process_command(string nickname, string channel, string message)
        {
            string command = message.Substring(1);
            string command_name = command.Split(' ')[0];
            string args = command.Substring(command_name.Length).Trim();
            switch (command_name)
            {
                // 주어진 문자열을 뒤집는다.
                case "reverse":
                    char[] array_ = args.ToCharArray();
                    Array.Reverse(array_);
                    SendMessage(channel, new string(array_));
                    break;
                // 구구단 출력
                case "99":
                    StringBuilder sb = new StringBuilder();
                    for (int x = 2; x <= 9; x++)
                    {
                        for (int y = 1; y <= 9; y++)
                        {
                            sb.AppendFormat(channel, "{0} x {1} = {2} | ", x, y, x * y);
                        }
                        SendMessage(channel, sb.ToString());
                        sb.Clear();
                    }
                    break;
                // 헬로월드 출력
                case "helloworld":
                    SendMessage(channel, "안녕 세상아! Hello, World");
                    break;
                // 현재 날짜 / 시간 출력
                case "time":
                    SendMessage(channel, string.Format("{0} | {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString()));
                    break;
                // 지 혼자 빼애액 거림
                case "beep":
                    SendMessage(channel, "빼애애애애애애애애애액!!!");
                    break;
                // 명령어 도움말
                case "commands":
                    {
                        int n = 0;
                        if (int.TryParse(args.Trim(), out n))
                        {
                            ShowHelp.Show(channel, Convert.ToInt32(args.Trim()));
                        }
                        else
                        {
                            ShowHelp.Show(channel, 1);
                        }
                    }
                    break;
                // 10진수 -> 2진수
                case "dec2bin":
                    try
                    {
                        SendMessage(channel, Convert.ToString(Convert.ToInt32(args), 2));
                    }
                    catch { SendMessage(channel, "올바른 정수 값을 입력해주세요. "); }
                    break;
                // 2진수 -> 10진수
                case "bin2dec":
                    try
                    {
                        SendMessage(channel, Convert.ToInt32(args, 2).ToString());
                    }
                    catch { SendMessage(channel, "올바른 2진수 값을 입력해주세요. "); }
                    break;
                // 팩토리얼 !n
                case "fact":
                    BigInteger bi = new BigInteger(1);
                    try
                    {
                        int n = Convert.ToInt32(args);
                        if (n <= 5000 && n > 1)
                        {
                            for (int i = 1; i <= n; i++)
                            {
                                bi *= i;
                            }
                            string result = bi.ToString();
                            if (result.Length >= 480)
                            {
                                IEnumerable<string> array2 = Split(result, 480);
                                foreach (string str in array2)
                                {
                                    SendMessage(channel, str);
                                }
                            }
                            else
                            {
                                SendMessage(channel, result);
                            }
                        }
                        else
                        {
                            SendMessage(channel, "범위에 맞지 않습니다. (1 < n <= 5000)");
                        }
                    }
                    catch { SendMessage(channel, "올바른 정수 값을 입력해주세요. "); }
                    break;
                // 피보나치 수열 n번째 항 구하기
                case "fibo":
                    BigInteger a = new BigInteger(0);
                    BigInteger b = new BigInteger(1);
                    BigInteger temp = new BigInteger();
                    try
                    {
                        int n = Convert.ToInt32(args);
                        if (n <= 9999 && n > 1)
                        {
                            for (int i = 1; i <= n; i++)
                            {
                                temp = a;
                                a = b;
                                b = temp + b;
                                temp = 0;
                            }
                            string result = a.ToString();
                            if (result.Length >= 480)
                            {
                                IEnumerable<string> array1 = Split(result, 480);
                                foreach (string str in array1)
                                {
                                    SendMessage(channel, str);
                                }
                            }
                            else
                            {
                                SendMessage(channel, result);
                            }
                        }
                        else
                        {
                            SendMessage(channel, "범위에 맞지 않습니다. (1 < n <= 9999)");
                        }
                    }
                    catch { SendMessage(channel, "올바른 정수 값을 입력해주세요. "); }
                    break;
                // 1부터 n까지 더한 값 내보내기
                case "sum_n":
                    try
                    {
                        int n = Convert.ToInt32(args);
                        if (n >= 2 && n <= 1000000)
                        {
                            BigInteger e = new BigInteger();
                            for (int k = 1; k <= n; k++)
                            {
                                e += k;
                            }
                            SendMessage(channel, e.ToString());
                        }
                        else
                        {
                            SendMessage("범위에 맞지 않습니다. (2 <= n <= 1000000");
                        }
                    }
                    catch { SendMessage(channel, "올바른 정수 값을 입력해주세요. "); }
                    break;
                // 원주율 500자리
                case "pi":
                    string pi = "3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491298336733624406566430860213949463952247371907021798609437027705392171762931767523846748184676694051320005681271452635608277857713427577896091736371787214684409012249534301465495853710507922796892589235420199561121290219608640344181598136297747713099605187072113499999983729780499510597317328160963185950244594553469083026425223082533446850352619311881710100031378387528865875332083814206171776691473035982534904287554687311595628638823537875937519577818577805321712268066130019278766111959092164201989380952572010654858632788659361533818279682303019520353018529689957736225994138912497217752834791315155748572424541506959508295331168617278558890750983817546374649393192550604009277016711390098488240128583616035637076601047101819429555961989467678374494482553797747268471040475346462080466842590694912933136770289891521047521620569660240580381501935112533824300355876402474964732639141992726042699227967823547816360093417216412199245863150302861829745557067498385054945885869269956909272107975093029553211653449872027559602364806654991198818347977535663698074265425278625518184175746728909777727938000816470600161452491921732172147723501414419735685481613611573525521334757418494684385233239073941433345477624168625189835694855620992192221842725502542568876717904946016534668049886272327917860857843838279679766814541009538837863609506800642251252051173929848960841284886269456042419652850222106611863067442786220391949450471237137869609563643719172874677646575739624138908658326459958133904780275900994657640789512694683983525957098258226205224894077267194782684826014769909026401363944374553050682034962524517493996514314298091906592509372216964615157098583874105978859597729754989301617539284681382686838689427741559918559252459539594310499725246808459872736446958486538367362226260991246080512438843904512441365497627807977156914359977001296160894416948685558484063534220722258284886481584560285060168427394522674676788952521385225499546667278239864565961163548862305774564980355936345681743241125150760694794510965960940252288";
                    IEnumerable<string> array = Split(pi, 480);
                    foreach (string str in array)
                    {
                        SendMessage(channel, str);
                    }
                    break;
                // 유니코드 랜덤 한글문자 500자 출력
                case "hang":
                    char c = '\0';
                    StringBuilder sb2 = new StringBuilder();
                    for (int i = 0; i < 500; i++)
                    {
                        c = (char)(random.Next(0x2B9F) + 0xAC00 + 1);
                        sb2.Append(c);
                    }
                    IEnumerable<string> array3 = Split(sb2.ToString(), 100);
                    foreach (string str in array3)
                    {
                        SendMessage(channel, str);
                    }
                    break;
                // 문자열 -> Base64 인코딩
                case "b64enc":
                    string tmp = Convert.ToBase64String(Encoding.UTF8.GetBytes(args));
                    if (tmp.Length > 100)
                    {
                        IEnumerable<string> array4 = Split(tmp, 100);
                        foreach (string str in array4)
                        {
                            SendMessage(channel, str);
                        }
                    }
                    else
                    {
                        SendMessage(channel, tmp);
                    }
                    break;
                // Base64 인코딩 -> 문자열
                case "b64dec":
                    try
                    {
                        string tmp2 = Encoding.UTF8.GetString(Convert.FromBase64String(args));
                        if (tmp2.Length > 100)
                        {
                            IEnumerable<string> array4 = Split(tmp2, 100);
                            foreach (string str in array4)
                            {
                                SendMessage(channel, str);
                            }
                        }
                        else
                        {
                            SendMessage(channel, tmp2);
                        }
                    }
                    catch { SendMessage("제대로 된 Base64 문자열을 입력해주세요."); }
                    break;
                // 벤(Ban)
                case "ban":
                    if (nickname == "theo5970" && args != "theo5970")
                    {
                        if (!banlist.Contains(args))
                        {
                            banlist.Add(args);
                            SendMessage(channel, "목록에 추가하였습니다.");
                        }
                        else
                        {
                            SendMessage(channel, "이미 있습니다.");
                        }
                    }
                    break;
                // 벤 풀기(Unban)
                case "unban":
                    if (nickname == "theo5970")
                    {
                        if (banlist.Contains(args))
                        {
                            banlist.Remove(args);
                            SendMessage(channel, "벤을 풀었습니다.");
                        }
                        else
                        {
                            SendMessage(channel, "해당 닉네임이 목록에 없습니다.");
                        }
                    }
                    break;
                // 공지사항 띄우기
                case "notice":
                    string notice_msg = string.Format("[{0}님의 공지] {1}", nickname, args);
                    SendMessage(channel, notice_msg);
                    break;
                // 저장하기
                case "save":
                    if (args.Length > 0)
                    {
                        saved = args;
                        SendMessage(channel, "저장을 완료하였습니다");
                    }
                    else
                    {
                        SendMessage(channel, "0자 이상이어야 합니다.");
                    }
                    break;
                // 저장한 거 내보내기
                case "say":
                    if (saved.Length > 0)
                    {
                        SendMessage(channel, saved);
                    }
                    break;
                // 엿먹히
                case "fuck":
                    SendMessage(channel, "you");
                    break;
                // 스톱워치 기능 (시작 / 일시중지 / 중지)
                case "stopwatch":
                    switch (args.Trim())
                    {
                        case "start":
                            if (!stopwatch.IsRunning)
                            {
                                stopwatch.Start();
                                SendMessage(channel, "스톱워치가 시작되었습니다.");
                            }
                            else
                            {
                                SendMessage(channel, "이미 실행중입니다.");
                            }
                            break;

                        case "pause":
                            if (stopwatch.IsRunning)
                            {
                                stopwatch.Stop();
                                SendMessage(channel, "스톱워치가 일시중지 되었습니다.");
                                SendMessage(channel, string.Format("시간: {0:F3}초", stopwatch.ElapsedMilliseconds / 1000.0));
                            }
                            else
                            {
                                SendMessage(channel, "스톱워치가 시작되지 않았습니다.");
                            }
                            break;

                        case "stop":
                            stopwatch.Stop();
                            SendMessage(channel, string.Format("시간: {0:F3}초", stopwatch.ElapsedMilliseconds / 1000.0));
                            stopwatch.Reset();
                            SendMessage(channel, "스톱워치가 중지 되었습니다.");
                            break;

                        default:
                            SendMessage(channel, "stopwatch [start/pause/stop]");
                            SendMessage(channel, "start - 스톱워치를 시작합니다.");
                            SendMessage(channel, "pause - 스톱워치를 일시중지합니다.");
                            SendMessage(channel, "stop - 스톱워치를 중지합니다.");
                            break;
                    }
                    break;
                // 브레인퍽 인터프리터
                case "bf":
                    if (brainfuck == null)
                    {
                        brainfuck = new Brainfuck();
                        brainfuck.ShutdownRequested += Brainfuck_ShutdownRequested;
                    }
                    bf_ch = channel;
                    try
                    {
                        brainfuck.Run(args.Trim());
                    }
                    catch (Exception ex)
                    {
                        SendMessage(channel, ex.ToString());
                    }
                    break;
                // 망할 ABC 언어
                case "abc":
                    SendMessage(channel, ABCDRunner.Run(args.Trim()));
                    break;
                // C# 인터프리터
                case "cs":
                    if (AdminManager.Contains(nickname))
                    {
                        if (!CSharpRunner.filter_list.Any(s => args.Contains(s)))
                        {
                            cs_ch = channel;
                            CSharpRunner.Input(args.Trim());
                        }
                        else
                        {
                            SendMessage(channel, "보안을 위해 해당 코드는 무시됩니다.");
                        }
                    }
                    else
                    {
                        SendMessage(channel, "권한이 없습니다.");
                    }
                    break;
                // 관리자 주기
                case "op":
                    string _nickname = args.Trim();
                    if (AdminManager.Contains(nickname))
                    {
                        if (!AdminManager.Add(_nickname))
                        {
                            SendMessage(channel, "목록에 이미 존재합니다.");
                        }
                        else
                        {
                            SendMessage(channel, "관리자 목록에 추가하였습니다.");
                        }
                    }
                    else
                    {
                        SendMessage(channel, "권한이 없습니다.");
                    }
                    break;
                // 관리자 해제하기
                case "deop":
                    _nickname = args.Trim();
                    if (AdminManager.Contains(nickname))
                    {
                        if (nickname != _nickname)
                        {
                            if (!AdminManager.Remove(_nickname))
                            {
                                SendMessage(channel, "목록에 존재하지 않습니다.");
                            }
                            else
                            {
                                SendMessage(channel, "목록에서 삭제했습니다.");
                            }
                        }
                        else
                        {
                            SendMessage(channel, "자기 자신을 삭제할 수 없습니다.");
                        }
                    }
                    else
                    {
                        SendMessage(channel, "권한이 없습니다.");
                    }
                    break;
                // 파이썬 인터프리터
                case "py":
                    py_ch = channel;
                    args = args.Trim();
                    switch (args)
                    {
                        case "run":
                            PythonRunner.Run(nickname);
                            break;

                        case "reset":
                            PythonRunner.Reset();
                            break;

                        case "clear":
                            PythonRunner.Clear(nickname);
                            break;

                        default:
                            PythonRunner.Add(nickname, args);
                            break;
                    }
                    break;
            }
            // 명령어가 아무 것도 없을 때
            if (command_name.Length == 0)
            {
                SendMessage(channel, "-- theo5970의 봇입니다 --");
                SendMessage(channel, "!commands - 명령어 목록 보기");
            }
            await Task.Delay(1);
        }

        // 브레인퍽 출력
        private void Brainfuck_ShutdownRequested(string message)
        {
            SendMessage(bf_ch, message);
        }

        // C# 인터프리터 출력
        private void CSharpRunner_OutputData(string output)
        {
            SendMessage(cs_ch, output);
        }

        // IronPython 출력
        private void PythonRunner_Print(string data)
        {
            SendMessage(py_ch, data);
        }

        // ShowHelp 도움말
        private void ShowHelp_HelpShowed(string ch, string msg)
        {
            SendMessage(ch, msg);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}