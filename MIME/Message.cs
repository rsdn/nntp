using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace Rsdn.Mime
{

	/// <summary>
	/// Priority of a mail message.
	/// </summary>
	public enum MailPriority
	{
		/// <summary>
		/// Highest priority.
		/// </summary>
		Highest = 1,
		/// <summary>
		/// High priority.
		/// </summary>
		High,
		/// <summary>
		/// Normal priority.
		/// </summary>
		Normal,
		/// <summary>
		/// Low priority.
		/// </summary>
		Low,
		/// <summary>
		/// Lowest priority.
		/// </summary>
		Lowest
	}

	/// <summary>
	/// MIME Content-Transfer encoding.
	/// </summary>
	public enum ContentTransferEncoding
	{
		/// <summary>
		/// Unknown encoding.
		/// </summary>
		Unknown,
		/// <summary>
		/// Seven bit encoding (ASCII).
		/// </summary>
		SevenBit,
		/// <summary>
		/// Eight bit encoding (not all servers supports).
		/// </summary>
		EightBit,
		/// <summary>
		/// Binary encoding (the same as eight bit).
		/// </summary>
		Binary,
		/// <summary>
		/// Qouted printable encoding.
		/// </summary>
		QoutedPrintable,
		/// <summary>
		/// Base64 encoding.
		/// </summary>
		Base64
	};

	/// <summary>
	/// MIME message class.
	/// </summary>
	[Serializable]
	public class Message : IBody
	{
		/// <summary>
		/// Construct empty message with default initial headers.
		/// </summary>
		public Message() : this(true)	{	}

		/// <summary>
		/// Construct empty message with or without default headers.
		/// </summary>
		/// <param name="useDefaultHeaders">Add default headers if true.</param>
		public Message(bool useDefaultHeaders)
		{
			// initialize entities array
			entities = new ArrayList();

			// initialize header and internal filters for it
			header = new Header();
			header.AddFilter("Content-Transfer-Encoding", new FilterHandler(ContentTransferFilter));
			header.AddFilter("Content-Type", new FilterHandler(ContentTypeFilter));
			header.AddFilter("X-Priority", new FilterHandler(PriorityFilter));
			header.AddFilter("X-MSMail-Priority", new FilterHandler(PriorityFilter));

			ContentTypeEvent += new ContentTypeHandler(MessageContentTypeHandler);

			this["Content-type"] = "text/plain; charset=us-ascii";

			// default fields
			if (useDefaultHeaders)
				this["MIME-Version"] = "1.0";
		}

		/// <summary>
		/// Message's entities
		/// </summary>
		protected ArrayList entities;
		/// <summary>
		/// Message's entities
		/// </summary>
		public ArrayList Entities
		{
			get {return entities;}
		}
		/// <summary>
		/// Header
		/// </summary>
		protected Header header;

		/// <summary>
		/// 'From' header
		/// </summary>
		public string From
		{
			get {return header["From"];}
			set {header["From"] = value;}
		}

		/// <summary>
		/// 'Subject' header
		/// </summary>
		public string Subject
		{
			get {return header["Subject"];}
			set {header["Subject"] = value;}
		}

		/// <summary>
		/// 'Date' header
		/// </summary>
		public DateTime Date
		{
			get {return DateTime.Parse(header["Date"]);}
			set {header["Date"] = value.ToUniversalTime().ToString("r");}
		}

		/// <summary>
		/// 'Content-Type' header
		/// </summary>
		public string ContentType
		{
			get {return header["Content-Type"];}
			set {header["Content-Type"] = value;}
		}

		/// <summary>
		/// Internal storage of Message's priority
		/// </summary>
		protected MailPriority priority;
		/// <summary>
		/// Message's priority
		/// </summary>
		public MailPriority Priority
		{
			get { return priority; }
			set
			{
				priority = value;
				this["X-Priority"] = string.Format("{0:d} ({0})", priority);
				this["X-MSMail-Priority"] = priority.ToString();
			}
		}

		/// <summary>
		/// get header identity from message's header by its name
		/// </summary>
		public string this[string name]
		{
			get {return header[name];}
			set {header[name] = value;}
		}

		/// <summary>
		/// Header filter for priority fileds (X-PRIORITY,X-MSMAIL-PRIORITY)
		/// </summary>
		/// <param name="headerField">Field name</param>
		/// <param name="value">Value of the field</param>
		/// <returns>Filtered value.</returns>
		protected string PriorityFilter(string headerField, string value)
		{
			switch (headerField.ToUpper())
			{
				case "X-PRIORITY" :
					priority = (MailPriority)int.Parse(Regex.Match(value, @"\d").Groups[0].Value);
					break;
				case "X-MSMAIL-PRIORITY" :
					priority = (MailPriority)Enum.Parse(typeof(MailPriority), value, true);
					break;
			}
			return value;
		}
	
		/// <summary>
		/// Internal storage of Message Content Transfer encoding.
		/// </summary>
		protected ContentTransferEncoding transferEncoding;
		/// <summary>
		/// Message Content-Transfer encoding
		/// </summary>
		public ContentTransferEncoding TransferEncoding
		{
			get	{	return transferEncoding; }
			set
			{
				transferEncoding = value;
				string transferEncodingString;
				switch (value)
				{
					case ContentTransferEncoding.Base64	:
						transferEncodingString = "base64";
						break;
					case ContentTransferEncoding.QoutedPrintable	:
						transferEncodingString = "quoted-printable";
						break;
					case ContentTransferEncoding.Binary	:
						transferEncodingString = "binary";
						break;
					case ContentTransferEncoding.EightBit :
						transferEncodingString = "8bit";
						break;
					case ContentTransferEncoding.SevenBit :
						transferEncodingString = "7bit";
						break;
					default:
						transferEncodingString = null;
						break;
				}
				header["Content-Transfer-Encoding"] = transferEncodingString;
			}
		}

		/// <summary>
		/// Content-Transfer filter for detecting MIME encoding.
		/// </summary>
		/// <param name="headerField">Header field name</param>
		/// <param name="value">Value of the field.</param>
		/// <returns>Filtered value.</returns>
		protected string ContentTransferFilter(string headerField, string value)
		{
			switch (value.ToLower())
			{
				case "base64" :
					transferEncoding = ContentTransferEncoding.Base64;
					break;
				case "quoted-printable":
					transferEncoding = ContentTransferEncoding.QoutedPrintable;
					break;
				case "binary" :
					transferEncoding = ContentTransferEncoding.Binary;
					break;
				case "8bit" :
					transferEncoding = ContentTransferEncoding.EightBit;
					break;
				case "7bit" :
					transferEncoding = ContentTransferEncoding.SevenBit;
					break;
				default:
					transferEncoding = ContentTransferEncoding.Unknown;
					break;
			}
			return value;
		}

		/// <summary>
		///  encoding for body
		/// </summary>
		protected Encoding encoding = Encoding.ASCII;
		/// <summary>
		/// Encoding for non-ascii header fields
		/// </summary>
		protected Encoding headerEncoding = Encoding.UTF8;
		/// <summary>
		/// Encoding for non-ascii header fields
		/// </summary>
		public Encoding HeaderEncoding
		{
			get {return headerEncoding;}
			set {headerEncoding = value;}
		}

		/// <summary>
		/// Regular expression for extracting 'Content-Type' header parameters.
		/// </summary>
		static readonly protected Regex contentTypeParameter = 
			new Regex(@"\s*;\s*(?<attribute>[^\s""=]+)\s*=\s*(?<quote>"")?(?<value>[^\s""]+)(?(quote)"")(?<!;)");
		/// <summary>
		/// Regular expression for extracting 'Content-Type' header parts.
		/// </summary>
		static readonly protected Regex contentTypeRegex =
			new Regex(string.Format(@"^(?<type>\S+?)\s*/\s*(?<subtype>[^;\s]+)(?<parameter>{0})*",
			contentTypeParameter),	RegexOptions.Compiled);

		/// <summary>
		/// Delegate for processing COntent-Type filter.
		/// </summary>
		protected delegate void ContentTypeHandler(string type, string subtype,
			NameValueCollection parameters);

		/// <summary>
		/// Event for Content-Type filter.
		/// </summary>
		protected event ContentTypeHandler ContentTypeEvent;

		/// <summary>
		/// Content-Type header field filter.
		/// </summary>
		/// <param name="headerField">Field name</param>
		/// <param name="value">Content of field</param>
		/// <returns>Filtered value.</returns>
		protected string ContentTypeFilter(string headerField, string value)
		{
			Match contentTypeMatch = contentTypeRegex.Match(value);
			if (!contentTypeMatch.Success)
				throw new MimeFormattingException("Content-Type header field is bad formatted.");
			NameValueCollection parameters = new NameValueCollection();
			foreach (Capture parameter in contentTypeMatch.Groups["parameter"].Captures)
			{
				Match parameterMatch = contentTypeParameter.Match(parameter.Value);
				parameters[parameterMatch.Groups["attribute"].Value] = parameterMatch.Groups["value"].Value;
			}

			type = contentTypeMatch.Groups["type"].Value.ToLower();
			subtype = contentTypeMatch.Groups["subtype"].Value.ToLower();

			if (ContentTypeEvent != null)
				ContentTypeEvent(type, subtype,	parameters);

			StringBuilder filteredValue = new StringBuilder();
			filteredValue.AppendFormat("{0}/{1}", type, subtype);
			foreach (string parameterName in parameters.AllKeys)
				filteredValue.AppendFormat("; {0}=\"{1}\"", parameterName, parameters[parameterName]);

			return filteredValue.ToString();
		}

		/// <summary>
		/// Internal storage for message MIME type.
		/// </summary>
		protected string type;
		/// <summary>
		/// Message MIME type.
		/// </summary>
		public string ContentTypeType
		{
			get { return type; }
		}

		/// <summary>
		/// Internal storage for message MIME subtype.
		/// </summary>
		protected string subtype;
		/// <summary>
		/// Message MIME subtype.
		/// </summary>
		public string ContentTypeSubtype
		{
			get { return subtype; }
		}

		/// <summary>
		/// Boundary string for multipart MIME messages.
		/// </summary>
		protected string multipartBoundary;

		/// <summary>
		/// Mesasge's COntent-Type handler.
		/// </summary>
		/// <param name="type">Message MIME type.</param>
		/// <param name="subtype">Message MIME subtype.</param>
		/// <param name="parameters">Additional parameters from Content-Type header.</param>
		protected void MessageContentTypeHandler(string type, string subtype,
			NameValueCollection parameters)
		{
			switch (type.ToLower())
			{
				case "text" :
					if (parameters["charset"] != null)
						encoding = Encoding.GetEncoding(parameters["charset"]);
					else
						encoding = Encoding.ASCII;
					break;
				case "multipart" :
					multipartBoundary = parameters["boundary"];
					if (multipartBoundary == null)
					{
						multipartBoundary = Guid.NewGuid().ToString();
						parameters.Add("boundary", multipartBoundary);
					}
					break;
			}
		}

		/// <summary>
		/// Regular expression for extracting header fileds.
		/// </summary>
		static readonly protected Regex headerField =
			new Regex(@"(?m)^(?<fieldName>\S+)[ \t]*:[ \t]*(?<fieldBody>.*?)[ \t]*" + Util.CRLF);
		/// <summary>
		/// Regular expression for select message header and body
		/// </summary>
		static readonly protected Regex headerAndBody =
			new Regex(string.Format(@"(?s)(?<header>.*?{0}){0}(?<body>.*)", Util.CRLF),
			RegexOptions.Compiled);

//		/// <summary>
//		/// Parse bytes (ASCII encoding) to MIME message.
//		/// Expect MIME-Version header.
//		/// </summary>
//		/// <param name="byteArray">Byte array</param>
//		/// <returns>MIME message object</returns>
//		static public Message Parse(byte[] byteArray)
//		{
//			return Parse(Util.BytesToString(byteArray));
//		}
//
//		/// <summary>
//		/// Parse string to MIME message.
//		/// Expect MIME-Version header.
//		/// </summary>
//		/// <param name="text">Input string</param>
//		/// <returns>MIME message object</returns>
//		static public Message Parse(string text)
//		{
//			return Parse(text, true);
//		}

		/// <summary>
		/// Regex for detercting of non-ascii chars.
		/// </summary>
		protected static Regex nonAsciiCharacter = new Regex("[^\x00-\x7F]", RegexOptions.Compiled);

		/// <summary>
		/// Parse string to MIME message.
		/// </summary>
		/// <param name="text">Input string</param>
		/// <param name="checkMime">If true - check presence of MIME-Version header.</param>
		/// <param name="checkAscii">Check using only ASCII symbols in header.</param>
		/// <param name="itemsToCheck">Regex expression to detect header's items to check for non-ascii symbols.</param>
		/// <returns>MIME message object</returns>
		static public Message Parse(string text, bool checkMime, bool checkAscii, Regex itemsToCheck)
		{
			// if need check Mime version - do not use default headers
			Message message = new Message(false);
			Match headerAndBodyMatch = headerAndBody.Match(text);
			if (!headerAndBodyMatch.Success)
				throw new MimeFormattingException("MIME message is bad formatted.");

			foreach (Match headerFieldMatch in
				headerField.Matches(Header.Unfold(headerAndBodyMatch.Groups["header"].Value)))
				if (checkAscii && itemsToCheck.IsMatch(headerFieldMatch.Groups["fieldName"].Value) &&
					nonAsciiCharacter.IsMatch(headerFieldMatch.Groups["fieldBody"].Value))
					throw new MimeFormattingException("Only ASCII symbols allowed!");
				else
					message[headerFieldMatch.Groups["fieldName"].Value] =
						headerFieldMatch.Groups["fieldBody"].Value;

			if (checkMime && message["MIME-Version"] == null)
				throw new MimeFormattingException("It's not MIME message!");

			switch (message.type)
			{
				case "multipart" :
					Regex multipartExtractor =
						new Regex(string.Format(@"(?s)--{0}{1}(?<entityBody>.*?)(?=--{0}({1}|--))",
							Regex.Escape(message.multipartBoundary), Util.CRLF));
					foreach (Match multipartMatch in multipartExtractor.Matches(
						headerAndBodyMatch.Groups["body"].Value))
							message.entities.Add(Message.Parse(multipartMatch.Groups["entityBody"].Value, false, checkAscii, itemsToCheck));
					break;
				default:
					byte[] body;
					switch (message.transferEncoding)
					{
						case ContentTransferEncoding.Base64 :
              body = Convert.FromBase64String(headerAndBodyMatch.Groups["body"].Value);
							break;
						case ContentTransferEncoding.QoutedPrintable :
							body = Util.FromQuotedPrintableString(headerAndBodyMatch.Groups["body"].Value);
							break;
						case ContentTransferEncoding.SevenBit :
							// TODO: cut 8th bit or not?
						case ContentTransferEncoding.Binary :
						case ContentTransferEncoding.EightBit :
						default :
							body = Util.StringToBytes(headerAndBodyMatch.Groups["body"].Value);
							break;
					}
					
					if (message.type == "text")
						// if content type is 'text' - interpet body as text
						message.entities.Add(message.encoding.GetString(body));
					else
						// otherwise (content type is not 'text') - interpet body as byte array
						message.entities.Add(body);
					break;
			}
			return message;
		}

		/// <summary>
		/// Get message body
		/// </summary>
		/// <returns></returns>
		public string GetBody()
		{
			StringBuilder builder = new StringBuilder();

			// headers
			builder.Append(header.Encode(headerEncoding));

			// delimeter
			builder.Append(Util.CRLF);

			bool multipart = (type == "multipart");
			if (multipart)
				builder.Append("This is a multi-part message in MIME format.").Append(Util.CRLF);

			// bodies
			foreach (object body in entities)
			{
				if (multipart)
					builder.Append(Util.CRLF).AppendFormat("--{0}", multipartBoundary).Append(Util.CRLF);

				if (body is IBody)
					builder.Append(((IBody)body).GetBody());
				else
				{
					if (body is byte[])
						builder.Append(Util.Encode((byte[])body, transferEncoding, true));
					else
						builder.Append(Util.Encode(body.ToString(), encoding, transferEncoding, false, true));
				}
			}
			if (multipart)
				builder.Append(Util.CRLF).AppendFormat("--{0}--", multipartBoundary);

			return builder.ToString();
		}
	
		/// <summary>
		/// Get text presentation of MIME message object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return GetBody();
		}

		/// <summary>
		/// Get encoded header field.
		/// </summary>
		/// <param name="name">Field name</param>
		/// <returns></returns>
		public string EncodedHeader(string name)
		{
			return header[name, headerEncoding];
		}
	}
}
