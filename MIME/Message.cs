using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace Rsdn.Mime
{

	public enum MailPriority {Highest = 1, High, Normal, Low, Lowest}

	public enum ContentTransferEncoding {Unknown, SevenBit, EightBit, Binary, QoutedPrintable, Base64};

	/// <summary>
	/// Summary description for Message.
	/// </summary>
	[Serializable]
	public class Message : IBody
	{
		public Message() : this(true)	{	}

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

		protected ArrayList entities;
		public ArrayList Entities
		{
			get {return entities;}
		}
		protected Header header;

		public string From
		{
			get {return header["From"];}
			set {header["From"] = value;}
		}

		public string Subject
		{
			get {return header["Subject"];}
			set {header["Subject"] = value;}
		}

		public DateTime Date
		{
			get {return DateTime.Parse(header["Date"]);}
			set {header["Date"] = value.ToUniversalTime().ToString("r");}
		}

		public string ContentType
		{
			get {return header["Content-Type"];}
			set {header["Content-Type"] = value;}
		}

		protected MailPriority priority;
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
	
		protected ContentTransferEncoding transferEncoding;
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
		/// encoding for non-ascii header fields
		/// </summary>
		protected Encoding headerEncoding = Encoding.UTF8;
		public Encoding HeaderEncoding
		{
			get {return headerEncoding;}
			set {headerEncoding = value;}
		}

		static readonly protected Regex contentTypeParameter = 
			new Regex(@"\s*;\s*(?<attribute>[^\s""=]+)\s*=\s*(?<quote>"")?(?<value>[^\s""]+)(?(quote)"")(?<!;)");
		static readonly protected Regex contentTypeRegex =
			new Regex(string.Format(@"^(?<type>\S+?)\s*/\s*(?<subtype>[^;\s]+)(?<parameter>{0})*",
			contentTypeParameter),	RegexOptions.Compiled);

		protected delegate void ContentTypeHandler(string type, string subtype, NameValueCollection parameters);

		protected event ContentTypeHandler ContentTypeEvent;

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

		protected string type;
		public string ContentTypeType
		{
			get { return type; }
		}
		protected string subtype;
		public string ContentTypeSubtype
		{
			get { return subtype; }
		}

		protected string multipartBoundary;

		protected void MessageContentTypeHandler(string type, string subtype, NameValueCollection parameters)
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

		static readonly protected Regex headerField =
			new Regex(@"(?m)^(?<fieldName>\S+)\s*:\s*(?<fieldBody>.*)\s*" + Util.CRLF);
		static readonly protected Regex unfoldHeaderField =
			new Regex(Util.CRLF + @"\s", RegexOptions.Compiled);
		static readonly protected Regex headerAndBody =
			new Regex(string.Format(@"(?s)(?<header>.*?{0}){0}(?<body>.*)", Util.CRLF), RegexOptions.Compiled);

		static public Message Parse(byte[] byteArray)
		{
			return Parse(Util.BytesToString(byteArray));
		}

		static public Message Parse(string text)
		{
			return Parse(text, true);
		}

		static public Message Parse(string text, bool checkMime)
		{
			// if need check Mime version - do not use default headers
			Message message = new Message(false);
			Match headerAndBodyMatch = headerAndBody.Match(text);
			if (!headerAndBodyMatch.Success)
				throw new MimeFormattingException("MIME message is bad formatted.");

			foreach (Match headerFieldMatch in
				headerField.Matches(unfoldHeaderField.Replace(headerAndBodyMatch.Groups["header"].Value,
					string.Empty)))
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
							message.entities.Add(Message.Parse(multipartMatch.Groups["entityBody"].Value, false));
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
					builder.AppendFormat("{0}--{1}{0}", Util.CRLF, multipartBoundary);

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
				builder.AppendFormat("{0}--{1}--", Util.CRLF, multipartBoundary);

			return builder.ToString();
		}
	
		public override string ToString()
		{
			return GetBody();
		}

		public string EncodedHeader(string name)
		{
			return header[name, headerEncoding];
		}
	}
}
