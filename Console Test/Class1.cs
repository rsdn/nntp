using System;
using derIgel.NNTP;
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
        NNTPSettings serverSettings =
					NNTPSettings.Deseriazlize(ConfigurationSettings.AppSettings["settings.ConfigFile"]);

				Manager nntpManager = new Manager(serverSettings);
				nntpManager.Start();

				System.Console.ReadLine();

				nntpManager.Stop();
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e);
			}
		}
	}
}