using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// AUTHINFO USER and AUTHINFO PASS commands.
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
			if ("USER".Equals(lastMatch.Groups["mode"].Value, StringComparison.OrdinalIgnoreCase))
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
						if (session.DataProvider.Authentificate(session.Username, session.Password,
									((IPEndPoint)session.RemoteEndPoint).Address))
						{
							session.sessionState = Session.States.Normal;
							result = new Response(NntpResponse.AuthentificationAccepted);
							var remoteHost = ((IPEndPoint)session.RemoteEndPoint).Address.ToString();
							try
							{
								remoteHost = Dns.GetHostEntry(remoteHost).HostName;
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
