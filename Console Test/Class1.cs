using System;
using System.Diagnostics;
using derIgel.NNTP;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Reflection;
using System.Configuration;

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
				Type settingsType = Assembly.LoadFrom(ConfigurationSettings.AppSettings["settings.Assembly"]).
					GetType(ConfigurationSettings.AppSettings["settings.Type"], true);

				Type dataProviderType = Assembly.Load(ConfigurationSettings.AppSettings["dataProvider.Assembly"]).
					GetType(ConfigurationSettings.AppSettings["dataProvider.Type"], true);

				object serverSettings = NNTPSettings.Deseriazlize(
					ConfigurationSettings.AppSettings["settings.ConfigFile"], settingsType);

				Manager nntpManager = new Manager(dataProviderType,	(NNTPSettings)serverSettings);
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
