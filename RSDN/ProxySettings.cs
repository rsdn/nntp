using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Serialization;
using System.Text;
using System.Security.Cryptography;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// Helper class for XML Serializatiob of WebProxy class
	/// </summary>
	public class ProxySettings
	{
		protected UriBuilder uriBuilder;
		public ProxySettings()	{	}

		public ProxySettings(WebProxy proxy)
		{
			if ((proxy != null) && (proxy.Address != null))
			{
				uriBuilder = new UriBuilder(proxy.Address);
				if ((proxy.Credentials as NetworkCredential) != null)
				{
					uriBuilder.UserName = ((NetworkCredential)proxy.Credentials).UserName;
					uriBuilder.Password = ((NetworkCredential)proxy.Credentials).Password;
				}
			}
		}

		public string Address
		{
			get { return (uriBuilder == null) ? null : uriBuilder.Scheme + Uri.SchemeDelimiter + uriBuilder.Uri.Authority; }
			set { uriBuilder = new UriBuilder(value); }
		}

		public string Username
		{
			get { return (uriBuilder == null) ? null : uriBuilder.UserName; }
			set { uriBuilder.UserName = value; }
		}

		/// <summary>
		/// encrypted password
		/// </summary>
		[XmlElement(DataType = "base64Binary")]
		public byte[] Password
		{
			get
			{
				byte[] result = null;
				if ((uriBuilder != null) && (uriBuilder.Password != ""))
				{
					RijndaelManaged myRijndael = new RijndaelManaged();
					byte[] Key = myRijndael.Key;
					Encoding.UTF8.GetBytes(Address, 0, Math.Min(Key.Length, Address.Length), Key, 0);
					byte[] IV = myRijndael.IV;
					Encoding.UTF8.GetBytes(Username, 0, Math.Min(IV.Length, Username.Length), IV, 0);
					ICryptoTransform encryptor =
						myRijndael.CreateEncryptor(Key, IV);
					byte[] source = Encoding.UTF8.GetBytes(uriBuilder.Password);
					result = encryptor.TransformFinalBlock(source, 0, source.Length);
					encryptor.Dispose();
				}
				return result;
			}
			set
			{
				byte[] result = value;
				if (result.Length > 0)
				{
					RijndaelManaged myRijndael = new RijndaelManaged();
					byte[] Key = myRijndael.Key;
					Encoding.UTF8.GetBytes(Address, 0, Math.Min(Key.Length, Address.Length), Key, 0);
					byte[] IV = myRijndael.IV;
					Encoding.UTF8.GetBytes(Username, 0, Math.Min(IV.Length, Username.Length), IV, 0);
					ICryptoTransform decryptor =
						myRijndael.CreateEncryptor(Key, IV);
					result = decryptor.TransformFinalBlock(value, 0, value.Length);
					decryptor.Dispose();
				}

				if (uriBuilder != null)
					uriBuilder.Password = Encoding.UTF8.GetString(result);
			}
		}

		[XmlIgnore]
		public WebProxy Proxy
		{
			get
			{
				return (uriBuilder == null) ? new WebProxy() :
					new WebProxy(uriBuilder.Uri, false, null,	new NetworkCredential(uriBuilder.UserName, uriBuilder.Password));
			}		
		}
	}
}