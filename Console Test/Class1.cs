using System;
using System.Diagnostics;
using derIgel.NNTP;
using System.Net.Sockets;
using derIgel.Mail;

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
			message.ContentType = "text/df; charset=\"\"; abra=cod";
			try
			{
				RsdnNntpSettings serverSettings = (RsdnNntpSettings)
					RsdnNntpSettings.Deseriazlize("config.xml", typeof(RsdnNntpSettings));

				Manager nntpManager = new Manager(typeof(RsdnDataProvider),	serverSettings);
				nntpManager.Start();
				while(true);
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e.Message);
			}
		}
	}
}
