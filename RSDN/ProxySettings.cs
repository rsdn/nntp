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
		public ProxySettings()
		{
			uriBuilder = new UriBuilder();
		}

		public ProxySettings(WebProxy proxy)
		{
			uriBuilder = new UriBuilder(proxy.Address);
			uriBuilder.UserName = ((NetworkCredential)proxy.Credentials).UserName;
			uriBuilder.Password = ((NetworkCredential)proxy.Credentials).Password;
		}

		public string Address
		{
			get { return uriBuilder.Scheme + Uri.SchemeDelimiter + uriBuilder.Uri.Authority; }
			set { uriBuilder = new UriBuilder(value); }
		}

		public string Username
		{
<<<<<<< ProxySettings.cs
			get { return uriBuilder.UserName; }
			set { uriBuilder.UserName = value; }
=======
			get {return (host != null) ? CreateUri(protocol, host, port, username + "@" + password) : null;}
>>>>>>> 1.3
		}

		/// <summary>
		/// encrypted password
		/// </summary>
		[XmlElement(DataType = "base64Binary")]
		public byte[] Password
		{
			get
			{
				RijndaelManaged myRijndael = new RijndaelManaged();
				byte[] Key = myRijndael.Key;
				Encoding.UTF8.GetBytes(Address, 0, Math.Min(Key.Length, Address.Length), Key, 0);
				byte[] IV = myRijndael.IV;
				Encoding.UTF8.GetBytes(Username, 0, Math.Min(IV.Length, Username.Length), IV, 0);
				ICryptoTransform encryptor =
					myRijndael.CreateEncryptor(Key, IV);
				byte[] source = Encoding.UTF8.GetBytes(uriBuilder.Password);
				byte[] result = encryptor.TransformFinalBlock(source, 0, source.Length);
				encryptor.Dispose();

				return result;
			}
			set
			{
				RijndaelManaged myRijndael = new RijndaelManaged();
				byte[] Key = myRijndael.Key;
				Encoding.UTF8.GetBytes(Address, 0, Math.Min(Key.Length, Address.Length), Key, 0);
				byte[] IV = myRijndael.IV;
				Encoding.UTF8.GetBytes(Username, 0, Math.Min(IV.Length, Username.Length), IV, 0);
				ICryptoTransform decryptor =
					myRijndael.CreateEncryptor(Key, IV);
				byte[] result = decryptor.TransformFinalBlock(value, 0, value.Length);
				decryptor.Dispose();

				uriBuilder.Password = Encoding.UTF8.GetString(result);
			}
		}

<<<<<<< ProxySettings.cs
=======
		protected int port = 80;
		[DefaultValue(80)]
		[Description("Port")]
		public int Port
		{
			get	{	return port;	}
			set	{	port =  value; }
		}

		protected string username;
		[Description("username")]
		public string Username
		{
			get	{	return username;}
			set	{	username = value; }
		}

		protected string password;
		[Description("Password")]
		[EditorAttribute(typeof(PasswordEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public string Password
		{
			get	{	return password; }
			set	{password = value; }
		}

		public override string ToString()
		{
			return (ProxyUri != null) ? ProxyUri.GetLeftPart(UriPartial.Authority) :
				null;
		}

>>>>>>> 1.3
		[XmlIgnore]
		public WebProxy Proxy
		{
			get
			{
				return new WebProxy(uriBuilder.Uri, false, null,
					new NetworkCredential(uriBuilder.UserName, uriBuilder.Password));
			}		
		}
	}
}