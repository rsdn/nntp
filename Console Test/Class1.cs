using System;
using System.Configuration;
using Rsdn.Nntp;
using log4net;

[assembly: log4net.Config.DOMConfigurator(Watch=true)]

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
			ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			try
			{
        NntpSettings serverSettings =
					NntpSettings.Deseriazlize(ConfigurationSettings.AppSettings["settings.ConfigFile"]);

				Manager nntpManager = new Manager(serverSettings);
				nntpManager.Start();

				System.Console.ReadLine();

				nntpManager.Stop();
			}
			catch (Exception e)
			{
				logger.Fatal("ConsoleApp", e);
			}
		}
	}
}