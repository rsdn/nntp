using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Rsdn.Mime
{
	/// <summary>
	/// Helper class
	/// </summary>
	public class Util
	{
		/// <summary>
		/// line delimeter
		/// </summary>
		public const string CRLF = "\r\n";

		/// <summary>
		/// binary line delimeter is ASCII encoding
		/// </summary>
		public static readonly byte[] asciiCRLF = Encoding.ASCII.GetBytes(CRLF);

		/// <summary>
		/// maximum length of line
		/// </summary>
		public const int LineLength = 76;

		/// <summary>
		/// Regular expression to split text in 998 characters chunks.
		/// </summary>
		static protected readonly Regex split998 = new Regex(@".{1,998}", RegexOptions.Compiled);

		/// <summary>
		/// Encode bytes to specified MIME encoding string.
		/// </summary>
		/// <param name="bytes">Bytes to encode.</param>
		/// <param name="contentEncoding">MIME Encoding.</param>
		/// <param name="breakLines">Split in lines if true.</param>
		/// <returns>MIME encoded byte stream.</returns>
		public static IEnumerable<byte> Encode(ArraySegment<byte> bytes, ContentTransferEncoding contentEncoding,
			bool breakLines)
		{
			return Encode(bytes.Array, bytes.Offset, bytes.Count, contentEncoding, breakLines);
		}

				/// <summary>
		/// Encode bytes to specified MIME encoding string.
		/// </summary>
		/// <param name="bytes">Bytes to encode.</param>
		/// <param name="contentEncoding">MIME Encoding.</param>
		/// <param name="breakLines">Split in lines if true.</param>
		/// <returns>MIME encoded byte stream.</returns>
		public static IEnumerable<byte> Encode(IEnumerable<byte> bytes, ContentTransferEncoding contentEncoding,
			bool breakLines)
		{
			var array = bytes is byte[] ? (byte[])bytes : new List<byte>(bytes).ToArray();
			return Encode(array, 0, array.Length, contentEncoding, breakLines);
		}

		/// <summary>
		/// Encode bytes to specified MIME encoding string.
		/// </summary>
		/// <param name="bytes">Bytes to encode.</param>
		/// <param name="offset">Offset in bytes</param>
		/// <param name="length">Length of bytes</param>
		/// <param name="contentEncoding">MIME Encoding.</param>
		/// <param name="breakLines">Split in lines if true.</param>
		/// <returns>MIME encoded byte stream.</returns>
		public static IEnumerable<byte> Encode(byte[] bytes, int offset, int length,
			ContentTransferEncoding contentEncoding, bool breakLines)
		{
			var result = new List<byte>(1024);
			switch (contentEncoding)
			{
				// TODO: is ASCII okay?
				case ContentTransferEncoding.Base64 :
					result.AddRange(Encoding.ASCII.GetBytes(
						Convert.ToBase64String(bytes, offset, length, breakLines ?
							Base64FormattingOptions.InsertLineBreaks : Base64FormattingOptions.None)));
					break;
				// TODO: is ASCII okay?
				case ContentTransferEncoding.QoutedPrintable:
					result.AddRange(Encoding.ASCII.GetBytes(
						ToQuotedPrintableString(bytes, offset, length, breakLines)));
					break;
				case ContentTransferEncoding.SevenBit :
					// TODO: cut 8th bit or not?
				case ContentTransferEncoding.EightBit :
				case ContentTransferEncoding.Binary   :
				default :
					// split per 1000 symbols (including trailing CRLF)
					//writer.Write(encoding.GetBytes(split998.Replace(body.ToString(), "$&" + Util.CRLF)));
					for (int i = offset; i < offset + length; i++ )
					{
						result.Add(bytes[i]);
					}
					break;
			}
			return result;
		}

		/// <summary>
		/// Convert text to MIME encoded string with specified text and content transfer encodings
		/// </summary>
		/// <param name="text">Text to convert</param>
		/// <param name="header">true, if convert for MIME header</param>
		/// <param name="textEncoding">Target text encoding</param>
		/// <param name="contentEncoding">Content-Transfer encoding.
		///		In header only base64 and quoted-printable encodings have meaning</param>
		/// <param name="breakLines">Split in lines if true.</param>
		/// <returns>MIME encoded text</returns>
		public static byte[] Encode(string text, Encoding textEncoding,
			ContentTransferEncoding contentEncoding, bool header, bool breakLines)
		{
			var builder = new List<byte>(1024);
			var reallyHeader = false;

			if (header)
			{
				switch (contentEncoding)
				{
					case ContentTransferEncoding.Base64 :
					case ContentTransferEncoding.QoutedPrintable :
						reallyHeader = true;
						builder.AddRange(textEncoding.GetBytes("=?"));
						builder.AddRange(textEncoding.GetBytes(textEncoding.HeaderName));
						builder.AddRange(textEncoding.GetBytes("?"));
						builder.AddRange(textEncoding.GetBytes(
							(contentEncoding == ContentTransferEncoding.Base64) ? "b" : "q"));
						builder.AddRange(textEncoding.GetBytes("?"));
						break;
				}
			}

			switch (contentEncoding)
			{
				case ContentTransferEncoding.Base64 :
				case ContentTransferEncoding.QoutedPrintable :
					builder.AddRange(Encode(textEncoding.GetBytes(text), contentEncoding, breakLines));
					break;
				case ContentTransferEncoding.SevenBit :
					// cut 8th bit
					builder.AddRange(Encoding.ASCII.GetBytes(text));
					break;
				default :
					builder.AddRange(textEncoding.GetBytes(text));
					break;
			}

			if (reallyHeader)
				builder.AddRange(textEncoding.GetBytes("?="));
			
			return builder.ToArray();
		}

		/// <summary>
		/// Check if text contains only ASCII symbols.
		/// </summary>
		/// <param name="text">Input text.</param>
		/// <returns>True if containf only ASCII symbols, false otherwise.</returns>
		public static bool OnlyASCIISymbols(string text)
		{
			var result = true;
			foreach (var symbol in text)
				if (symbol > 0x7f)
				{
					result = false;
					break;
				}
			return result;
		}
		
		/// <summary>
		/// Check if byte stream contains only ASCII symbols.
		/// </summary>
		/// <param name="bytes">Input byte stream</param>
		/// <returns>True if containf only ASCII symbols, false otherwise.</returns>
		public static bool OnlyASCIISymbols(byte[] bytes)
		{
			var result = true;
			foreach (var symbol in bytes)
				if (symbol > 0x7f)
				{
					result = false;
					break;
				}
			return result;
		}

		/// <summary>
		/// Regular expression to extract quoted-prinatble encode symbols.
		/// </summary>
		static protected readonly Regex quotedPrintableEncodedSymbol =
			new Regex(@"(?i)=(?<code>[0-9a-f]{2})", RegexOptions.Compiled);

		/// <summary>
		/// Regular expression to detect soft breaks in quoted-prinatble text.
		/// </summary>
		static protected readonly Regex quotedPrintableSoftBreaks =
			new Regex(@"=" + CRLF, RegexOptions.Compiled);
		
		/// <summary>
		/// Decode quoted-prntable text.
		/// </summary>
		/// <param name="encodedText">Encoded text.</param>
		/// <returns>Decoded text.</returns>
		public static byte[] FromQuotedPrintableString(string encodedText)
		{
			var decodedSymbols = quotedPrintableEncodedSymbol.Replace(encodedText,
				new MatchEvaluator(quotedPrintableEncodedSymbolMatchEvaluator));
			decodedSymbols = quotedPrintableSoftBreaks.Replace(decodedSymbols, "");
		
			return StringToBytes(decodedSymbols);
		}

		/// <summary>
		/// Decode quoted-printable character.
		/// </summary>
		/// <param name="match">Input character match.</param>
		/// <returns>Decoded character.</returns>
		static protected string quotedPrintableEncodedSymbolMatchEvaluator(Match match)
		{
		 return ((char)Convert.ToInt32(match.Groups["code"].Value, 16)).ToString();
		}

		/// <summary>
		/// Regular expression for detect symbols which not needed to encode in quoted printable coding.
		/// Question symbol (0x3f, '?') is not required to be encoded in general Q-encoded word,
		/// but need to be encoded in header. So, encode it always...
		/// </summary>
		static protected readonly Regex quotedPrintableDecodedSymbol =
			new Regex(@"[^\x21-\x3c\x3e\x40-\x7e]", RegexOptions.Compiled);

		/// <summary>
		/// Regular expression for breaking 'quoted-rintable' strings in lines
		/// </summary>
		static protected readonly Regex insertQuotedPrintableSoftBreaks =
			new Regex(".{1," + LineLength + "}(?<!=.?)", RegexOptions.Compiled);

				/// <summary>
		/// Encode with 'quoted-printable' encoding
		/// </summary>
		/// <param name="bytes">source bytes</param>
		/// <param name="breakLines">break in lines, if true</param>
		/// <returns>'quoted-printable' encoded string</returns>
		public static string ToQuotedPrintableString(byte[] bytes, bool breakLines)
		{
			return ToQuotedPrintableString(bytes, 0, bytes.Length, breakLines);
		}

		/// <summary>
		/// Encode with 'quoted-printable' encoding
		/// </summary>
		/// <param name="bytes">source bytes</param>
		/// <param name="offset">offset in bytes</param>
		/// <param name="length">length of bytes</param>
		/// <param name="breakLines">break in lines, if true</param>
		/// <returns>'quoted-printable' encoded string</returns>
		public static string ToQuotedPrintableString(byte[] bytes, int offset, int length, bool breakLines)
		{
			var quotedString = quotedPrintableDecodedSymbol.Replace(BytesToString(bytes, offset, length),
					new MatchEvaluator(quotedPrintableDecodedSymbolMatchEvaluator));

			return breakLines ?
				insertQuotedPrintableSoftBreaks.Replace(quotedString,	"$&=" + CRLF) : quotedString;
		}
		/// <summary>
		/// Encode with 'quoted-printable' encoding without line-breaking
		/// </summary>
		public static string ToQuotedPrintableString(byte[] bytes)
		{
			return ToQuotedPrintableString(bytes, false);
		}

		/// <summary>
		/// Encode charcter to quoted-encoded view.
		/// </summary>
		/// <param name="match">Input chracter match.</param>
		/// <returns>Encoded character.</returns>
		static protected string quotedPrintableDecodedSymbolMatchEvaluator(Match match)
		{
			return "=" + ((int)match.Value[0]).ToString("X2"); // 2 hexadeximal digits
		}

		/// <summary>
		/// Convert string to raw bytes
		/// </summary>
		public static byte[] StringToBytes(string text)
		{
			return Encoding.GetEncoding("iso8859-1").GetBytes(text);
		}

		/// <summary>
		/// Direct convert raw bytes to string
		/// </summary>
		/// <param name="input">bytes to convert</paparam>
		/// <param name="length">size of bytes</param>
		public static string BytesToString(byte[] input, int length)
		{
			return BytesToString(input, 0, length);
		}

		/// <summary>
		/// Direct convert raw bytes to string
		/// </summary>
		/// <param name="input">bytes to convert</param>
		/// <param name="offset">offset in bytes</param>
		/// <param name="length">size of bytes</param>
		public static string BytesToString(byte[] input, int offset, int length)
		{
			// TODO: is ASCII okay?
			return Encoding.ASCII.GetString(input, offset, length);
		}

		/// <summary>
		/// Convert byte array to string.
		/// </summary>
		/// <param name="input">Byte array.</param>
		/// <returns>Result string.</returns>
		public static string BytesToString(byte[] input)
		{
			return BytesToString(input, input.Length);
		}
	
	}
}