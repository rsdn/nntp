using System;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text;

using System.Collections.Specialized;
using System.Net;

using Rsdn.Mime;
using Rsdn.Nntp;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// XOVER client command
	/// </summary>
	[NntpCommand("XOVER")]
	public class Xover : Generic
	{
		/// <summary>
		/// Syntaxis checker for XOVER command.
		/// </summary>
		protected static Regex XoverSyntaxisChecker =
			new Regex(@"(?in)^XOVER([ \t]+(?<startNumber>\d+)" + 
								@"([ \t]*(?<dash>-)[ \t]*(?<endNumber>\d+)?)?)?[ \t]*$",
								RegexOptions.Compiled);

		/// <summary>
		/// Create XOVER command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Xover(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = XoverSyntaxisChecker;
		}

		/// <summary>
		/// Process XOVER command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			if (session.currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			NewsArticle[] articleList;
			if (lastMatch.Groups["startNumber"].Success)
			{
				int startNumber = Convert.ToInt32(lastMatch.Groups["startNumber"].Value);
				int endNumber = startNumber;
				if (lastMatch.Groups["dash"].Success)
					if (lastMatch.Groups["endNumber"].Success)
						endNumber = Convert.ToInt32(lastMatch.Groups["endNumber"].Value);
					else
						endNumber = -1;
				articleList = session.DataProvider.
					GetArticleList(startNumber, endNumber, session.currentGroup, NewsArticle.Content.Header);
			}
			else
			{
				if (session.currentArticle == -1)
					throw new DataProviderException((session.currentGroup == null) ?
						DataProviderErrors.NoSelectedGroup : DataProviderErrors.NoSelectedArticle);

				articleList = new NewsArticle[1];
				articleList[0] = session.DataProvider.GetArticle(session.currentArticle, session.currentGroup, NewsArticle.Content.Header);
			}

			if (articleList.Length > 0)
			{
				StringBuilder output = new StringBuilder();
				foreach (NewsArticle article in articleList)
				{
					output.Append(ModifyArticle(article).MessageNumbers[session.currentGroup]);
					foreach (string headerItem in List.headerItems)
						// replace in *unfolded* header all non-good symbols to space
						output.Append('\t').Append(article[headerItem] == null ? null :
							Regex.Replace(Header.Unfold(article.EncodedHeader(headerItem)), @"\s", " "));
					output.Append(Util.CRLF);
				}
				return new Response(NntpResponse.Overview, output.ToString());
			}
			else
				return new Response(NntpResponse.NoSelectedArticle);
		}
	}

	/// <summary>
	/// QUIT client command
	/// </summary>
	[NntpCommand("QUIT")]
	public class Quit : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex QuitSyntaxisChecker =
			new Regex(@"(?in)^QUIT[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Quit(Session session) : base(session)
		{
			syntaxisChecker = QuitSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			return new Response(NntpResponse.Bye);
		}
	}

	/// <summary>
	/// SLAVE client command
	/// </summary>
	[NntpCommand("SLAVE")]
	public class Slave : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex SlaveSyntaxisChecker =
			new	Regex(@"(?in)^SLAVE[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Slave(Session session) : base(session)
		{
			syntaxisChecker = SlaveSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			return new Response(NntpResponse.Slave); // ok
		}
	}

	/// <summary>
	/// MODE READER and MODE STREAM client command
	/// </summary>
	[NntpCommand("MODE")]
	public class Mode : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex ModeSyntaxisChecker =
			new Regex(@"(?in)^MODE[ \t]+(?<mode>READER|STREAM)[ \t]*$",	RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Mode(Session session) : base(session)
		{
			syntaxisChecker = ModeSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			Response result;
			if (lastMatch.Groups["mode"].Value.ToUpper() == "READER")
				// MODE READER
				result = new Response(session.DataProvider.PostingAllowed ? NntpResponse.Ok : NntpResponse.OkNoPosting,
					null, session.Manager.NamedServerID);
			else
				// MODE STREAM
				result = new Response(NntpResponse.NotRecognized);
			return result;
		}
	}

	/// <summary>
	/// AUTHINFO USER and AUTHINFO PASS client commands
	/// </summary>
	[NntpCommand("AUTHINFO")]
	public class AuthInfo : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex AuthInfoSyntaxisChecker = 
			new	Regex(@"(?in)^AUTHINFO[ \t]+(?<mode>USER|PASS)[ \t]+(?<param>\S+)[ \t]*$",
								RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public AuthInfo(Session session) : base(session)
		{
			syntaxisChecker = AuthInfoSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			Response result = null;
			if (lastMatch.Groups["mode"].Value.ToUpper() == "USER")
				// AUTHINFO USER
				switch (session.sessionState)
				{
					case	Session.States.Normal	:
					case	Session.States.AuthRequired	:
						session.Username	=	lastMatch.Groups["param"].Value;
						session.sessionState = Session.States.MoreAuthRequired;
						result = new Response(NntpResponse.MoreAuthentificationRequired);
						break;
					case Session.States.MoreAuthRequired	:
						session.sessionState = Session.States.AuthRequired;
						session.Username = "";
						result = new Response(NntpResponse.AuthentificationRejected);
						break;
				}
			else
				// AUTHINFO PASS
				switch (session.sessionState)
				{
					case	Session.States.Normal	:
					case	Session.States.AuthRequired	:
						result = new Response(NntpResponse.AuthentificationRejected);
						break;
					case Session.States.MoreAuthRequired	:
						session.Password	=	lastMatch.Groups["param"].Value;
						if (session.DataProvider.Authentificate(session.Username, session.Password))
						{
							session.sessionState = Session.States.Normal;
							result = new Response(281);
							string remoteHost = ((IPEndPoint)session.client.RemoteEndPoint).Address.ToString();
							try
							{
								remoteHost = Dns.GetHostByAddress(remoteHost).HostName;
							}
							catch (SocketException) {}
							session.sender = session.Username + "@" + remoteHost;
						}
						else
						{
							session.Username = "";
							session.Password = "";
							session.sessionState = Session.States.AuthRequired;
							result = new Response(NntpResponse.NoPermission);
							session.sender = null;
						}
						break;
				}
			return result;
		}
	}
}