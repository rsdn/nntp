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
		static void Main(string[] args)
		{
			ILog logger =
				log4net.LogManager.GetLogger("RSDN NNTP Server ConsoleApp");
			try
			{
        NntpSettings serverSettings =
					NntpSettings.Deseriazlize(ConfigurationManager.AppSettings["settings.ConfigFile"]);

				Manager nntpManager = new Manager(serverSettings);
				nntpManager.Start();

				System.Console.ReadLine();

				nntpManager.Stop();
			}
			catch (Exception e)
			{
				logger.Fatal("ConsoleApp", e);
				System.Console.ReadLine();
			}
		}
	}
}