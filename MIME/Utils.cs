using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace derIgel.MIME
{
	// helper class
	public class Util
	{
		/// <summary>
		/// line delimeter
		/// </summary>
		public const string CRLF = "\r\n";
		/// <summary>
		/// maximum length of line
		/// </summary>
		public const int lineLength = 76;
			
		/// <summary>
		/// text in base64 encoding in specified text encoding
		/// </summary>
		public static string Encode(string text, bool header, Encoding encoding)
		{
			StringBuilder builder = new StringBuilder();
			if (header)
				builder.Append("=?").Append(encoding.HeaderName).Append("?b?").
					Append(Convert.ToBase64String(encoding.GetBytes(text))).Append("?=");
			else
				builder.Append(Convert.ToBase64String(encoding.GetBytes(text)));
			
			return builder.ToString();
		}

		public static bool OnlyASCIISymbols(string text)
		{
			bool result = true;
			foreach (char symbol in text)
				if (symbol > 0x7f)
				{
					result = false;
					break;
				}
			return result;
		}

		public static bool OnlyASCIISymbols(byte[] bytes)
		{
			bool result = true;
			foreach (byte symbol in bytes)
				if (symbol > 0x7f)
				{
					result = false;
					break;
				}
			return result;
		}

		static protected readonly Regex quotedPrintableEncodedSymbol =
			new Regex(@"(?i)=(?<code>[0-9a-f]{2})", RegexOptions.Compiled);

		static protected readonly Regex quotedPrintableSoftBreaks =
			new Regex(@"=" + CRLF, RegexOptions.Compiled);
		
		public static byte[] FromQuotedPrintableString(string encodedText)
		{
			string decodedSymbols = quotedPrintableSoftBreaks.Replace(
				quotedPrintableEncodedSymbol.Replace(encodedText,
					new MatchEvaluator(quotedPrintableEncodedSymbolMatchEvaluator)), "");
		
			return StringToBytes(decodedSymbols);
		}

		static protected string quotedPrintableEncodedSymbolMatchEvaluator(Match match)
		{
		 return ((char)Convert.ToInt32(match.Groups["code"].Value, 16)).ToString();
		}

		static protected readonly Regex quotedPrintableDecodedSymbol =
			new Regex(@"[^\x09\x20\x21-\x3c\x3e-\x7e]", RegexOptions.Compiled);

		static protected readonly Regex insertQuotedPrintableSoftBreaks =
			new Regex(@".{1,76}(?<!=.?)", RegexOptions.Compiled);

		public static string ToQuotedPrintableString(byte[] bytes)
		{
			return insertQuotedPrintableSoftBreaks.Replace(
				quotedPrintableDecodedSymbol.Replace(BytesToString(bytes),
					new MatchEvaluator(quotedPrintableDecodedSymbolMatchEvaluator)),
				"$&=" + Util.CRLF);
		}
			
		static protected string quotedPrintableDecodedSymbolMatchEvaluator(Match match)
		{
			return "=" + ((int)match.Value[0]).ToString("X2"); // 2 hexadeximal digits
		}

		public static string ExpandException(Exception exception)
		{
			StringBuilder result = new StringBuilder();
			while (exception != null)
			{
				result.Append(exception.Message).Append(Environment.NewLine);
				exception = exception.InnerException;
			}
			return result.ToString();
		}

		public static byte[] StringToBytes(string text)
		{
			ArrayList result = new ArrayList();
			for (int i = 0; i < text.Length; i++)
			{
				result.Add((byte)text[i]);
				if (((short)text[i])  > 0xFF)
					result.Add((byte)(((short)text[i]) >> 8));
			}
			return (byte[])result.ToArray(typeof(byte));
		}

		public static string BytesToString(byte[] input)
		{
			return BytesToString(input, input.Length);
		}
	
		public static string BytesToString(byte[] input, int length)
		{
			StringBuilder sb = new StringBuilder(length);
			for (int i = 0; i < length; i++)
				sb.Append((char)input[i]);
			return sb.ToString();
		}
	}
}