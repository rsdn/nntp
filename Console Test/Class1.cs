using System;
using System.Diagnostics;
using derIgel.NNTP;
using derIgel.RsdnNntp;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using derIgel.MIME;

namespace ForumTest
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Message message = new Message();
			message.From = "test";
			message["fRom"] = "oo!";

			message.TransferEncoding = ContentTransferEncoding.Base64;
			message["Content-Transfer-Encoding"] = "x-zip";

			Message.Parse(@"MIME-Version: 1.0
Content-type: multipart/alternative; boundary=""8382efa5-c88e-418b-9512-d2d05955c316""
Message-ID: <145299@news.rsdn.ru>
From: promko <>
Date: Tue, 03 Dec 2002 14:52:15 GMT
Subject: =?koi8-r?b?9NLFwtXF1NPRIMXL2sXbzsnLIE5OVFAgc2VydmVy?=
Newsgroups: rsdn.desktop

This is a multi-part message in MIME format.

--8382efa5-c88e-418b-9512-d2d05955c316
MIME-Version: 1.0
Content-Transfer-Encoding: base64
Content-type: text/plain; charset=koi8-r

+sTSwdfT1NfVytTFLCBwcm9ta28sIPfZINDJ08HMyToKClA+8M/T1MHXycwg08XCxSBSU0ROIHYx
LjAgLSDTzc/HINrBx9LV1sHU2CDUz8zYy88g1MXN2QpQPsEg08/ExdLWwc7JxSDQydPFzSAtIM7F
1AoKUD7wz9zUz83VINrBx9LV2snMIE5OVFAgc2VydmVyIC0gzs8g0SDOxSDJzcXAIC5ORVQgRnJh
bWV3b3JrLSAKUD7HxMUgzc/WzsEgxM/T1MHU2CDF2tvF287JyyA/Cg==
--8382efa5-c88e-418b-9512-d2d05955c316
MIME-Version: 1.0
Content-Transfer-Encoding: base64
Content-type: text/html; charset=koi8-r

PCFET0NUWVBFIEhUTUwgUFVCTElDICItLy9XM0MvL0RURCBIVE1MIDQuMCBUcmFuc2l0aW9uYWwv
L0VOIiA+DQo8aHRtbD4NCgk8aGVhZD4NCgkJPE1FVEEgaHR0cC1lcXVpdj0iQ29udGVudC1UeXBl
IiBjb250ZW50PSJ0ZXh0L2h0bWw7IGNoYXJzZXQ9dXRmLTgiPg0KCQk8YmFzZSBocmVmPSJodHRw
Oi8vcnNkbi5ydS9mb3J1bS8iPg0KCQk8TElOSyBocmVmPSIvRm9ydW0vRm9ydW0uY3NzIiB0eXBl
PSJ0ZXh0L2NzcyIgcmVsPSJzdHlsZXNoZWV0Ij4NCgk8L2hlYWQ+DQoJPGJvZHkgbWFyZ2lud2lk
dGg9IjAiIG1hcmdpbmhlaWdodD0iMCI+DQoJCTx0YWJsZSBpZD0iVGFibGUxIiBjZWxsU3BhY2lu
Zz0iMCIgd2lkdGg9IjEwMCUiIGJvcmRlcj0iMCI+DQoJCQk8dHI+DQoJCQkJPHRkIGNsYXNzPSJz
IiBub1dyYXA+PEZPTlQgc2l6ZT0iMiI+RnJvbTombmJzcDs8QSBocmVmPSIvVXNlcnMvUHJvZmls
ZS5hc3B4P3VpZD0xMzk3NiIgdGFyZ2V0PSJfYmxhbmsiPjxCPnByb21rbzwvQj48L0E+Jm5ic3A7
PFNNQUxMPjwvU01BTEw+PC9GT05UPjwvdGQ+DQoJCQkJPFREIGNsYXNzPSJzIiBub1dyYXAgYWxp
Z249Im1pZGRsZSIgd2lkdGg9IjEwMCUiPjxGT05UIHNpemU9IjIiPjwvRk9OVD48L1REPg0KCQkJ
CTx0ZCBjbGFzcz0icyIgbm9XcmFwPjxBIHRpdGxlPSLkz8LB18nU2CDXIMnawtLBzs7PxSIgaHJl
Zj0iL1VzZXJzL0FkZEZhdi5hc3B4P21pZD0xNDUyOTkiIHRhcmdldD0iX2JsYW5rIj48SU1HIGhl
aWdodD0iMTQiIHNyYz0iL2ltYWdlcy9mYXYuZ2lmIiBhbGlnbj0iYWJzTWlkZGxlIiBib3JkZXI9
IjAiPjwvQT4mbmJzcDs8QSB0aXRsZT0i7sHQydPB1Nggzs/Xz8Ug08/Pwt3FzsnFIiBocmVmPSJO
ZXdNc2cuYXNweD9naWQ9MzIiIHRhcmdldD0iX2JsYW5rIj48SU1HIGhlaWdodD0iMTQiIHNyYz0i
aW1hZ2VzL25ldy5naWYiIHdpZHRoPSIxOCIgYWxpZ249ImFic01pZGRsZSIgYm9yZGVyPSIwIj48
L0E+Jm5ic3A7PEEgaHJlZj0iUmF0ZS5hc3B4P21pZD0xNDUyOTkmYW1wO3JhdGU9LTIiPjwvQT48
QSB0aXRsZT0i79TXxdTJ1NggzsEg08/Pwt3FzsnFIiBocmVmPSJOZXdNc2cuYXNweD9taWQ9MTQ1
Mjk5IiB0YXJnZXQ9Il9ibGFuayI+PElNRyBoZWlnaHQ9IjE0IiBzcmM9ImltYWdlcy9yZXBsYXku
Z2lmIiB3aWR0aD0iMTgiIGFsaWduPSJhYnNNaWRkbGUiIGJvcmRlcj0iMCI+PC9BPiZuYnNwOzxB
IHRpdGxlPSLwxdLFytTJINcgxs/S1c0iIGhyZWY9Ij9taWQ9MTQ1Mjk5IiB0YXJnZXQ9Il9ibGFu
ayI+PElNRyBoZWlnaHQ9IjE0IiBzcmM9ImltYWdlcy90aHIuZ2lmIiB3aWR0aD0iMTgiIGFsaWdu
PSJhYnNNaWRkbGUiIGJvcmRlcj0iMCI+PC9BPiZuYnNwOzxBIHRpdGxlPSLw0s/Tzc/U0sXU2CDX
08Ugz9TXxdTZINTFzdkiIGhyZWY9Ik1lc3NhZ2UuYXNweD9taWQ9MTQ1Mjk5IzE0NTI5OSI+PElN
RyBoZWlnaHQ9IjE0IiBzcmM9ImltYWdlcy9mbGF0LmdpZiIgd2lkdGg9IjE4IiBhbGlnbj0iYWJz
TWlkZGxlIiBib3JkZXI9IjAiPjwvQT4mbmJzcDsmbmJzcDsmbmJzcDs8Zm9udCBzaXplPSIxIj7v
w8XOydTYDQoJCQkJCTwvZm9udD48QSB0aXRsZT0i8SDUwcsgzsUgxNXNwcAiIGhyZWY9Ii9Gb3J1
bS9SYXRlLmFzcHg/bWlkPTE0NTI5OSZhbXA7cmF0ZT0wIiB0YXJnZXQ9Il9ibGFuayI+DQoJCQkJ
CQk8SU1HIGhlaWdodD0iMTQiIHNyYz0iaW1hZ2VzL24wLmdpZiIgd2lkdGg9IjE4IiBhbGlnbj0i
YWJzTWlkZGxlIiBib3JkZXI9IjAiPjwvQT48QSB0aXRsZT0i9cTBzMnU2CDPw8XOy9UiIGhyZWY9
Ii9Gb3J1bS9SYXRlLmFzcHg/bWlkPTE0NTI5OSZhbXA7cmF0ZT0tMSIgdGFyZ2V0PSJfYmxhbmsi
PjxJTUcgaGVpZ2h0PSIxNCIgc3JjPSJpbWFnZXMvbnguZ2lmIiB3aWR0aD0iMTgiIGFsaWduPSJh
YnNNaWRkbGUiIGJvcmRlcj0iMCI+PC9BPjxBIHRpdGxlPSLpztTF0sXTzs8iIGhyZWY9Ii9Gb3J1
bS9SYXRlLmFzcHg/bWlkPTE0NTI5OSZhbXA7cmF0ZT0xIiB0YXJnZXQ9Il9ibGFuayI+PElNRyBo
ZWlnaHQ9IjE0IiBzcmM9ImltYWdlcy9uMS5naWYiIHdpZHRoPSIxOCIgYWxpZ249ImFic01pZGRs
ZSIgYm9yZGVyPSIwIj48L0E+PEEgdGl0bGU9IvPQwdPJws8iIGhyZWY9Ii9Gb3J1bS9SYXRlLmFz
cHg/bWlkPTE0NTI5OSZhbXA7cmF0ZT0yIiB0YXJnZXQ9Il9ibGFuayI+PElNRyBoZWlnaHQ9IjE0
IiBzcmM9ImltYWdlcy9uMi5naWYiIHdpZHRoPSIxOCIgYWxpZ249ImFic01pZGRsZSIgYm9yZGVy
PSIwIj48L0E+PEEgdGl0bGU9IvPV0MXSIiBocmVmPSIvRm9ydW0vUmF0ZS5hc3B4P21pZD0xNDUy
OTkmYW1wO3JhdGU9MyIgdGFyZ2V0PSJfYmxhbmsiPjxJTUcgaGVpZ2h0PSIxNCIgc3JjPSJpbWFn
ZXMvbjMuZ2lmIiB3aWR0aD0iMTgiIGFsaWduPSJhYnNNaWRkbGUiIGJvcmRlcj0iMCI+PC9BPiZu
YnNwOyZuYnNwOyZuYnNwOzxBIHRpdGxlPSLtz8TF0snSz9fBzsnFIiBocmVmPSJTZWxmLmFzcHg/
bWlkPTE0NTI5OSI+PElNRyBoZWlnaHQ9IjE0IiBzcmM9ImltYWdlcy9kZWwuZ2lmIiB3aWR0aD0i
MTgiIGFsaWduPSJhYnNNaWRkbGUiIGJvcmRlcj0iMCI+PC9BPiZuYnNwOzwvRk9OVD48L3RkPg0K
CQkJPC90cj4NCgkJPC90YWJsZT4NCgkJPGRpdiBjbGFzcz0ibSI+PGZvbnQgc2l6ZT0iMiI+8M/T
1MHXycwg08XCxSBSU0ROIHYxLjAgLSDTzc/HINrBx9LV1sHU2CDUz8zYy88g1MXN2Qo8YnI+wSDT
z8TF0tbBzsnFINDJ08XNIC0gzsXUCjxicj4KPGJyPvDP3NTPzdUg2sHH0tXaycwgTk5UUCBzZXJ2
ZXIgLSDOzyDRIM7FIMnNxcAgLk5FVCBGcmFtZXdvcmstIAo8YnI+x8TFIM3P1s7BIMTP09TB1Ngg
xdrbxdvOycsgPwoNCgkJPC9kaXY+DQoJCTx0YWJsZSBpZD0iVGFibGUzIiBjZWxsU3BhY2luZz0i
MCIgd2lkdGg9IjEwMCUiIGJvcmRlcj0iMCI+DQoJCQk8dHI+DQoJCQkJPHRkIGNsYXNzPSJzIj4m
bmJzcDs8L3RkPg0KCQkJPC90cj4NCgkJPC90YWJsZT4NCgkJPC9GT05UPg0KCTwvYm9keT4NCjwv
aHRtbD4NCg==
--8382efa5-c88e-418b-9512-d2d05955c316--");

			try
			{
				Message.Parse("sddfdgsg");

				RsdnDataProviderSettings serverSettings = (RsdnDataProviderSettings)
					RsdnDataProviderSettings.Deseriazlize("config.xml",
						typeof(RsdnDataProviderSettings));

				Manager nntpManager = new Manager(typeof(RsdnDataProvider),	serverSettings);
				nntpManager.Start();

				System.Console.ReadLine();

				nntpManager.Stop();
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e.Message);
			}
		}

		static string Test1(string value)
		{
			return value+":tested1";
		}
		static string Test2(string value)
		{
			return value+":tested2";
		}
	}
}
