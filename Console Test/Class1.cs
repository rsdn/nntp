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
				Type settingsType = Activator.CreateInstanceFrom(
					ConfigurationSettings.AppSettings["settings.Assembly"],
					ConfigurationSettings.AppSettings["settings.Type"]).Unwrap().GetType();

				Type dataProviderType = Activator.CreateInstanceFrom(
					ConfigurationSettings.AppSettings["dataProvider.Assembly"],
					ConfigurationSettings.AppSettings["dataProvider.Type"]).Unwrap().GetType();

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
