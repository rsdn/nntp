using System;
using System.Configuration;
using Rsdn.Nntp;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch=true)]

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
		static void Main()
		{
			ILog logger = LogManager.GetLogger("RSDN NNTP Server ConsoleApp");
			try
			{
				NntpSettings serverSettings =
					NntpSettings.Deseriazlize(ConfigurationManager.AppSettings["settings.ConfigFile"]);

				Manager nntpManager = new Manager(serverSettings);
				nntpManager.Start();

				Console.ReadLine();

				nntpManager.Stop();
			}
			catch (Exception e)
			{
				logger.Fatal("ConsoleApp", e);
				Console.ReadLine();
			}
		}
	}
}