using System;
using System.Diagnostics;
using derIgel.NNTP;
using derIgel.RsdnNntp;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

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
			try
			{
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
	}
}
