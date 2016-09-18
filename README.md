# __Slackbot__ - *theo5970*
간단한 SSL통신 Slack(슬렉) IRC 봇입니다.

* * *
## 1. 소개
[phillyai](http://github.com/phillyai)님의 코드를 참고하여 만든 간단한 Slack IRC 봇입니다.<br/>
개발 언어는 100% C#이고, Visual Studio 2015 Community에서 제작되었습니다.<br/>
다양한 명령어들이 있습니다<br/>
* * *
## 2. 준비
파이썬과 C# 인터프리터 기능을 사용하실려면, <br/>
[IronPython](http://ironpython.net/), [CSScript](http://www.csscript.net/) 라이브러리가 필요합니다.

프로그램이 실행되는 위치에 아래와 같은 텍스트 파일이 있어야 합니다. <br/>

>- help.txt - 자동으로 생성되지 않음. (아래의 명령어 목록에서 앞의 순서를 빼시고 수동으로 생성해주셔야 합니다)<br/>
>- admins.txt - 자동으로 생성됨. 관리자 목록입니다.<br/>
>- cs_filters.txt - 자동으로 생성되지 않음. C# 코드에서 실행되지 말아야 할 단어들을 줄로 구분하면서 작성해주시면 됩니다<br/>
>- settings.txt - 자동으로 생성되지 않음. 접속 설정에 관한 설정이므로 아래를 참고하여 만들어주세요.<br/>
>[호스트 이름]<br/>
>[닉네임]<br/>
>[채널 이름]<br/>
>[비밀번호]<br/>


* * *

## 3. 명령어
> ####중요: 접두사는 __!__ 입니다

1. reverse: 문자열을 뒤집습니다. ex) Hello! -> !olleH<br/>
2. 99: 구구단 출력<br/>
3. helloworld: 헬로월드 출력<br/>
4. time: 현재 날짜와 시간<br/>
5. beep: 빼애액합니다<br/>
6. commands [page]: 명령어 도움말<br/>
7. dec2bin [n]: 10진수 -> 2진수<br/>
8. bin2dec [n]: 2진수 -> 10진수<br/>
9. fact [n]: 팩토리얼 !n<br/>
10. fibo [n]: 피보나치 수열의 n항<br/>
11. sum_n [n]: 1부터 n까지 더한 값 <br/>
12. pi: 원주율 500자리 <br/>
13. hang: 유니코드 랜덤 한글문자 500자 출력 <br/>
14. b64enc: 문자열 -> Base64 인코딩 <br/>
15. b64dec: 문자열 -> Base64 디코딩 <br/>
16. ban: (관리자) 차단하기 <br/>
17. unban: (관리자) 차단풀기 <br/>
18. notice: 공지사항 띄우기 <br/>
19. save: 저장하기 <br/>
20. say: 저장한 걸 출력하기 <br/>
21. ~~fuck: ㅁㄴㅇㄹ~~ <br/>
22. stopwatch [start/stop/pause]: 스톱워치 기능 (시작 / 일시중지 / 중지) <br/>
23. bf [code]: 브레인퍽 인터프리터 <br/>
24. abc [code]: ABCD 인터프리터 (자작) <br/>
25. cs [code]: C# 인터프리터 <br/>
26. py [code]: 파이썬 인터프리터 <br/>
27. op [user]: (관리자) 관리자 주기 <br/>
28. deop [user]: (관리자) 관리자 권한 해제 <br/>

* * *
## 4. 소스 파일
Program.cs - 봇을 생성하고 실행시킵니다. <br/>
ABCDRunner.cs - ABCD 언어(자작)를 실행시킵니다. <br/>
Bot.cs - 봇에 관한 모든 코드들이 포함되어 있습니다. <br/>
Brainfuck.cs - 브레인퍽에 대한 인터프리터입니다. <br/>
CSharpRunner.cs - csi를 이용한 C# 인터프리터입니다. <br/>
FileMonitor.cs - 파일을 모니터링하여 잠재적인 위험으로부터 보호합니다. <br/>
PythonRunner.cs - IronPython을 이용한 파이썬 인터프리터입니다. <br/>
AdminManager.cs - 관리자 목록을 관리합니다. <br/>

* * *
## 5. 종료하는 법
콘솔창에서 !quit 하면 즉시 종료됩니다.
* * *

## 6. 마치며
감사합니다.
