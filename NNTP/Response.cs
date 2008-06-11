using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Rsdn.Mime;

namespace Rsdn.Nntp
{
	/// <summary>
	/// NNTP Response's codes
	/// </summary>
	public enum NntpResponse
	{
		Help = 100,
		Date = 111,
		Ok = 200,
		OkNoPosting = 201,
		Slave = 202,
		Bye = 205,
		GroupSelected = 211,
		ListOfGroups = 215,
		ArticleHeadBodyRetrivied = 220,
		ArticleHeadRetrivied = 221,
		ArticleBodyRetrivied = 222,
		ArticleNothingRetrivied = 223,
		Overview = 224,
		ListOfArticlesByMessageID = 230,
		ListOfArticles = 231,
		TransferOk = 235,
		PostedOk = 240,
		AuthentificationAccepted = 281,
		TransferArticle = 335,
		SendArticle = 340,
		MoreAuthentificationRequired = 381,
		ServiceDiscontinued = 400,
		ServiceUnaviable = 401,
		TimeOut = 402,
		NotAllowed = 403,
		NoSuchGroup = 411,
		NoSelectedGroup = 412,
		NoSelectedArticle = 420,
		NoNextArticle = 421,
		NoPrevArticle = 422,
		NoSuchArticleNumber = 423,
		NoSuchArticle = 430,
		ArticleNotWanted = 435,
		TransferFailed = 436,
		ArticleRejected = 437,
		PostingFailed = 441,
		AuthentificationRejected = 482,
		AuthentificationRequired = 480,
		NotRecognized = 500,
		SyntaxisError = 501,
		NoPermission = 502,
		ProgramFault = 503
	};

	/// <summary>
	/// NNTP Server response
	/// </summary>
	public class Response
	{
		public class ParamsException : ArgumentException
		{
			public ParamsException(string error, Exception innerException)	:
				base(error, innerException) {}				
		}

		// Init NNTP Server's answer array
		static Response()
		{
			answers = new Dictionary<int, string>();
			answers[100] = "help text follows";
			answers[111] = "{0}";
			answers[200] = "{0} -- posting allowed";
			answers[201] = "{0} -- no posting allowed";
			answers[202] = "slave status noted";
			answers[205] = "closing connection - goodbye!";
			answers[211] = "{0} {1} {2} {3} group selected";
			answers[215] = "list of newsgroups follows";
			answers[220] = "{0} {1} article retrivied - head and body follow";
			answers[221] = "{0} {1} article retrivied - head follows";
			answers[222] = "{0} {1} article retrivied - body follows";
			answers[223] = "{0} {1} article retrivied - request text separately";
			answers[224] = "overview information follows";
			answers[230] = "list of new articles by message-id follows";
			answers[231] = "list of new newsgroup follows";
			answers[235] = "article transferred ok";
			answers[240] = "article posted ok";
			answers[281] = "authentification accepted";
			answers[335] = "send article to be transfered. End with <CR-LF>.<CR-LF>";
			answers[340] = "send article to be posted. End with <CR-LF>.<CR-LF>";
			answers[381] = "more authentification information required";
			answers[400] = "service discontinued";
			answers[401] = "service temporarily unavailable - try later";
			answers[402] = "connection timeout - bye!";
			answers[403] = "command not allowed now"; //not standard
			answers[411] = "no such news group '{0}'";
			answers[412] = "no newsgroup has been selected";
			answers[420] = "no current article has been selected";
			answers[421] = "no next article in this group";
			answers[422] = "no previous article in this group";
			answers[423] = "no such article number in this group";
			answers[430] = "no such article found";
			answers[435] = "article not wanted - do not send it";
			answers[436] = "transfer failed - try again later";
			answers[437] = "article rejected - do not try again";
			answers[440] = "posting not allowed";
			answers[441] = "posting failed. {0}";
			answers[480] = "authentification required";
			answers[482] = "authentification rejected";
			answers[500] = "command not recognized";
			answers[501] = "command syntax error";
			answers[502] = "no permission";
			answers[503] = "program fault. {0}";
		}

		public Response(string description, int code, object body, params object[] parameters)
		{
			this.code = code;
			this.description = description;
			this.parameters = parameters;
			reponsesBody = body;
		}
		public Response(int code, object body, params object[] parameters)
		{
			this.code = code;
			this.parameters = parameters;
			reponsesBody = body;
		}
		public Response(string description, NntpResponse code, object body, params object[] parameters) :
			this(description, (int)code, body, parameters)	{	}
		public Response(NntpResponse code, object body, params object[] parameters) :
			this(null, code, body, parameters)	{	}
		public Response(int code) : this(code, null) {}
		public Response(NntpResponse code) : this(code, null) {}
		public Response() : this(NntpResponse.NotRecognized) {} //default - error code 500 (not recognized command)

		/// <summary>
		/// Associative arrays of NNTP server answers
		/// </summary>
		static protected IDictionary<int, string> answers;

		/// <summary>
		/// response code
		/// </summary>
		protected int code;

		/// <summary>
		/// response parameters
		/// </summary>
		protected object[] parameters;

		/// <summary>
		/// Error description.
		/// If null - get default description.
		/// </summary>
		readonly string description;

		/// <summary>
		/// Get NNTP response as text
		/// </summary>
		/// <param name="code"></param>
		/// <param name="reponsesBody"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static List<byte> GetResponse(int code, object reponsesBody, params object[] parameters)
		{
			return GetResponse(null, code, reponsesBody, parameters);
		}

		/// <summary>
		/// Get NNTP response
		/// </summary>
		/// <param name="code"></param>
		/// <param name="description"></param>
		/// <param name="reponsesBody"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static List<byte> GetResponse(string description, int code, object reponsesBody, params object[] parameters)
		{
			try
			{
				var result = new List<byte>(Util.LineLength);
				var buff = new StringBuilder(Util.LineLength);
				buff.Append(code).Append(" ")
					.AppendFormat(description ?? (answers.ContainsKey(code) ?
						answers[code] : string.Empty), parameters)
          .Append(Util.CRLF);
				result.AddRange(Encoding.ASCII.GetBytes(buff.ToString()));
				if (reponsesBody != null)
				{
					var body = (reponsesBody is IBody) ?
						((IBody)reponsesBody).GetBody() :
						new List<byte>(Encoding.ASCII.GetBytes(reponsesBody.ToString()));
					result.AddRange(ModifyForNntpResponse(body));
				}
				return result;
			}
			catch(FormatException e)
			{
				throw new ParamsException("Formatting error", e);
			}
			catch (ArgumentNullException e)
			{
				throw new ParamsException("No such response code", e);
			}
		}

		public static void Answer(int code, Socket socket)
		{
			socket.Send(GetResponse(code, null).ToArray());
		}

		public static void Answer(NntpResponse code, Socket socket)
		{
			Answer((int)code, socket);
		}

		public List<byte> GetResponse()
		{
			return GetResponse(description, code, reponsesBody, parameters);
		}

		/// <summary>
		/// options body of response (null if none)
		/// </summary>
		protected object reponsesBody;

		protected static readonly byte DotEncoded = 0x2E;

		/// <summary>
		/// Modify response as double first dot
		/// </summary>
		public static List<byte> ModifyForNntpResponse(List<byte> response)
		{
			if (response == null)
				return null;
	
			for (int i = 0; i < response.Count; i++)
			{
				bool found = (response[i] == DotEncoded) &&
				             (i >= Util.asciiCRLF.Length);
				
				if (!found) continue;

				// check that before CRLF
				for (int j = 1; found && j <= Util.asciiCRLF.Length; j++)
				{
					found = (response[i - j] == Util.asciiCRLF[Util.asciiCRLF.Length - j]);
				}
				
				if (!found) continue;

				// add double point
				response.Insert(i++, DotEncoded);
			}

			var endWithCRLF = response.Count >= Util.asciiCRLF.Length;
			for (int j = 1; endWithCRLF && j <= Util.asciiCRLF.Length; j++)
			{
				endWithCRLF = (response[response.Count - j] == Util.asciiCRLF[Util.asciiCRLF.Length - j]);
			}

			if (!endWithCRLF)
				response.AddRange(Util.asciiCRLF);

			response.Add(DotEncoded);
			response.AddRange(Util.asciiCRLF);
			return response;
		}

		protected static readonly Regex DecodeNNTPMessage =
			new Regex(@"(?m)^\.\.", RegexOptions.Compiled);

		/// <summary>
		/// Modiffy textual response with removing double first dot
		/// </summary>
		public static string DemodifyTextResponse(string response)
		{
			if (response != null)
				return DecodeNNTPMessage.Replace(response, ".");
			return null;
		}

		public int Code
		{
			get
			{
				return code;
			}
		}

		public override string ToString()
		{
			return Encoding.ASCII.GetString(GetResponse().ToArray());
		}
	}
}