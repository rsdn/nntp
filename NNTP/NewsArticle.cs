using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace derIgel
{
	namespace NNTP
	{
		/// <summary>
		/// Newsgroup article
		/// </summary>
		public class NewsArticle
		{
			public NewsArticle(string messageID, int messageNumber, Hashtable header, string body) :
				this(messageID, messageNumber, header, body, Encoding.UTF8) {	}
				
			public NewsArticle(string messageID, int messageNumber, Hashtable header, string body, Encoding encoding)
			{
				this.encoding = encoding;
				this.messageNumber = messageNumber;
				this.body = body;
				headers = (header == null) ? CollectionsUtil.CreateCaseInsensitiveHashtable() :
					CollectionsUtil.CreateCaseInsensitiveHashtable(header);
			
				//detect content
				if (header == null)
					if (body != null)
						content = Content.Body;
					else
						content = Content.None;
				else
					if (body != null)
					content = Content.HeaderAndBody;
				else
					content = Content.Header;

				headers["Message-ID"] = messageID;
				headers["MIME-Version"] = "1.0";
				headers["Content-type"] = "text/html; charset=" + encoding.HeaderName;
				headers["Content-Transfer-Encoding"] = "base64";
			}

			protected Hashtable headers;
			protected	int messageNumber;
			protected	string body;

			public enum Content {None, Header, Body, HeaderAndBody }

			public int Number
			{
				get
				{
					return messageNumber;
				}
			}

			public string Body
			{
				get
				{
					return body;
				}
			}

			public override string ToString()
			{
				string result = null;
				if ((content == Content.Header) || (content == Content.HeaderAndBody))
				{
					// header
					foreach (DictionaryEntry header in headers)
						result += header.Key + ": " + EncodedHeader(header.Key.ToString())	+ Util.CRLF;
				}

				if ((content == Content.Body) || (content == Content.HeaderAndBody))
				{
					if (result != null)
						// delimeter
						result += Util.CRLF;

					// body (encode & breaked in lines)
					string encodedBody = Util.Encode(body, false, encoding);
					for (int i = 0; i < encodedBody.Length; i += Util.lineLength)
						result += encodedBody.Substring(i,
							(i + Util.lineLength < encodedBody.Length) ?
							Util.lineLength :
							encodedBody.Length - i) + Util.CRLF;
				}
				return result;
			}

			public string this[string headerName]
			{
				get
				{
					return headers[headerName] as string;
				}
				set
				{
					headers[headerName] = value.ToString();
				}
			}

			/// <summary>
			/// text encoding
			/// </summary>
			protected Encoding encoding;

			public string EncodedHeader(string name)
			{
				string value = headers[name] as string;
				if (value == null)
					return null;

				return (Util.OnlyASCIISymbols(value) ? value :	Util.Encode(value, true, encoding));
			}

			/// <summary>
			/// type of content of article
			/// </summary>
			protected Content content = Content.None;
		}
	}
}