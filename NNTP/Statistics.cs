using System;
using System.Collections;
using System.Web.Mail;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net;

namespace derIgel.NNTP
{
	using Util = derIgel.MIME.Util;

	/// <summary>
	/// Summary description for Statistics.
	/// </summary>
	[Serializable]
	public class Statistics
	{
		public string fromMail;
		public string fromServer;
		public string toMail;
		public TimeSpan interval;
		public string name;

		protected DateTime start;
		protected DateTime end;

		public Statistics()
		{
			name = Dns.GetHostName();
			statistics = new Hashtable();
			errors = new Hashtable();
			start = DateTime.Now;
		}
		protected Hashtable statistics;
		protected Hashtable errors;

		public void AddStatistic(string command)
		{
			if (statistics[command] == null)
				statistics[command] = 0;
			statistics[command] = (int)statistics[command] + 1;
		}
		public void AddError(int errorCode, string command)
		{
			if (errors[errorCode] == null)
				errors[errorCode] = new ArrayList();

			((ArrayList)errors[errorCode]).Add(command);
		}

		public void Clear()
		{
			statistics.Clear();
			errors.Clear();
		}

		public void Send()
		{
			if (Monitor.TryEnter(this))
			{
				try
				{
					end = DateTime.Now;
					string tempFile = Path.GetTempFileName();
					Serialize(tempFile);
					MailMessage message = new MailMessage();
					message.From = fromMail;
					message.To = toMail;
					message.Subject =
						string.Format("{0} - from {1:dd.MM.yyyy HH:mm:ss zzz} to {2:dd.MM.yyyy HH:mm:ss zzz}",
							name, start, end);
					message.BodyFormat = MailFormat.Text;
					MailAttachment mailAttachment = new MailAttachment(tempFile, MailEncoding.Base64);
					message.Attachments.Add(mailAttachment);
					SmtpMail.SmtpServer = fromServer;
					SmtpMail.Send(message);
					File.Delete(tempFile);
				}
				catch (Exception e)
				{
					#if DEBUG || SHOW
					System.Console.Error.WriteLine("\tmailsend: " + Util.ExpandException(e));
					#endif
					return;
				}
				finally
				{
					Monitor.Exit(this);
				}

				Clear();
				start = DateTime.Now;
			}
		}

		public void Serialize(string filename)
		{
			SoapFormatter formatter = new SoapFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write,	FileShare.None);
			formatter.Serialize(stream, this);
			stream.Close();
		}

		public static Statistics Deserialize(string filename)
		{
			SoapFormatter formatter = new SoapFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			Statistics stat = (Statistics)formatter.Deserialize(stream);
			stream.Close();
			return stat;
		}

		public void CheckSend()
		{
			if ((DateTime.Now - start) >= interval)
			{
				Thread mailingThread = new Thread(new ThreadStart(Send));
				mailingThread.Start();
			}
		}
	}
}
