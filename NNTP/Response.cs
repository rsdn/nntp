using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using derIgel.MIME;

namespace derIgel.NNTP
{
	using Util = derIgel.MIME.Util;

	// NNTP Response's codes
	public enum NntpResponse
	{
		Help = 100,
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
		PostedOk = 240,
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
		public class ParamsException : System.ArgumentException
		{
			public ParamsException(string error, Exception innerException)	:
				base(error, innerException) {}				
		}

		// Init NNTP Server's answer array
		static Response()
		{
			answers = new Hashtable();
			answers[100] = "100 help text follows";
			answers[200] = "200 RSDN NNTP Server -- posting allowed";
			answers[201] = "201 RSDN NNTP Server -- no posting allowed";
			answers[202] = "202 slave status noted";
			answers[205] = "205 RSDN NNTP Server closing connection - goodbye!";
			answers[211] = "211 {0} {1} {2} {3} group selected";
			answers[215] = "215 list of newsgroups follows";
			answers[220] = "220 {0} {1} article retrivied - head and body follow";
			answers[221] = "221 {0} {1} article retrivied - head follows";
			answers[222] = "222 {0} {1} article retrivied - body follows";
			answers[223] = "223 {0} {1} article retrivied - request text separately";
			answers[224] = "224 overview information follows";
			answers[230] = "230 list of new articles by message-id follows";
			answers[231] = "231 list of new newsgroup follows";
			answers[240] = "240 article posted ok";
			answers[281] = "281 authentification accepted";
			answers[340] = "340 send article to be posted. End with <CR-LF>.<CR-LF>";
			answers[381] = "381 more authentification information required";
			answers[400] = "400 service discontinued";
			answers[401] = "401 service temporarily unavailable - try later";
			answers[402] = "402 connection timeout - bye!";
			answers[403] = "403 command not allowed now"; //not standard
			answers[411] = "411 no such news group";
			answers[412] = "412 no newsgroup has been selected";
			answers[420] = "420 no current article has been selected";
			answers[421] = "421 no next article in this group";
			answers[422] = "422 no previous article in this group";
			answers[423] = "423 no such article number in this group";
			answers[430] = "430 no such article found";
			answers[440] = "440 posting not allowed";
			answers[441] = "441 posting failed";
			answers[480] = "480 authentification required";
			answers[482] = "482 authentification rejected";
			answers[500] = "500 command not recognized";
			answers[501] = "501 command syntax error";
			answers[502] = "502 no permission";
			answers[503] = "503 program fault - command not performed";
		}

		public Response(int code, byte[] body, params object[] parameters)
		{
			this.code = code;
			this.parameters = parameters;
			bodyResponse = body;
		}
		public Response(NntpResponse code, byte[] body, params object[] parameters) :
			this((int)code, body, parameters)	{	}
		public Response(int code) : this(code, null) {}
		public Response(NntpResponse code) : this(code, null) {}
		public Response() : this(NntpResponse.NotRecognized) {} //default - error code 500 (not recognized command)

		/// <summary>
		/// Associative arrays of NNTP server answers
		/// </summary>
		static protected Hashtable answers;
		/// <summary>
		/// response code
		/// </summary>
		protected int code;
		/// <summary>
		/// response parameters
		/// </summary>
		protected object[] parameters;

		public static byte[] GetResponse(int code, byte[] bodyResponse, params object[] parameters)
		{
			try
			{
				byte[] modifiedBody = ModifyTextResponse(bodyResponse);
				string answer = string.Format(answers[code] as string + Util.CRLF, parameters);
				int answerBytes = Encoding.ASCII.GetByteCount(answer);
				byte[] result = new byte[answerBytes + modifiedBody.Length];
				Encoding.ASCII.GetBytes(answer).CopyTo(result, 0);
				modifiedBody.CopyTo(result, answerBytes);
				return (result);
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
			socket.Send(GetResponse(code, null));
		}

		public static void Answer(NntpResponse code, Socket socket)
		{
			Answer((int)code, socket);
		}

		public byte[] GetResponse()
		{
			return GetResponse(code, bodyResponse, parameters);
		}

		/// <summary>
		/// body of response (only after status response may be)
		/// </summary>
		protected byte[] bodyResponse;

		protected static readonly Regex EncodeNNTPMessage =
			new Regex(@"(?m)^\.", RegexOptions.Compiled);

		/// <summary>
		/// Modiffy textual response as double first dot
		/// </summary>
		public static byte[] ModifyTextResponse(byte[] response)
		{
			if (response == null)
				return new byte[0];
	
			StringBuilder textRepresentation = new StringBuilder(
				// double start points
				EncodeNNTPMessage.Replace(Util.BytesToString(response), ".."));
			if (textRepresentation.Length > 0)
				if (!textRepresentation.ToString().EndsWith(Util.CRLF))
					textRepresentation.Append(Util.CRLF);
			textRepresentation.Append(".").Append(Util.CRLF);
			return Util.StringToBytes(textRepresentation.ToString());
		}

		protected static readonly Regex DecodeNNTPMessage =
			new Regex(@"(?m)^\.\.", RegexOptions.Compiled);

		/// <summary>
		/// Modiffy textual response with removing double first dot
		/// </summary>
		public static byte[] DemodifyTextResponse(byte[] response)
		{
			if (response != null)
			{
				StringBuilder textRepresentation = new StringBuilder(
					DecodeNNTPMessage.Replace(Util.BytesToString(response), "."));
				return Util.StringToBytes(textRepresentation.ToString());
			}
			else
				return new byte[0];
		}

		public int Code
		{
			get
			{
				return code;
			}
		}
	}
}