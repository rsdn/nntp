using System;
using System.Collections.Specialized;
using System.Net;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

using Rsdn.WMI.ROOT.CIMV2;
using System.Security.Cryptography.X509Certificates;
using Rsdn.Nntp.Editor;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Summary description for ServerEndPoint.
	/// </summary>
	[TypeConverterAttribute(typeof(ServerEndPointConverter))]
	public class ServerEndPoint
	{
		public ServerEndPoint() : this(IPAddress.Any, 0) {	}

		public ServerEndPoint(IPAddress address) : this(address, 0) {	}

		public ServerEndPoint(int port) : this(IPAddress.Any, port) {	}

		public ServerEndPoint(IPAddress address, int port)
		{
			endPoint = new IPEndPoint(address, port);
		}

    protected X509Certificate2 _certificate;
    [XmlIgnore]
    [TypeConverter(typeof(CertificateConverter))]
    [EditorAttribute(typeof(CertificateEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [Description("SSL certificate. When specified connection is secured.")]
    public X509Certificate2 Certificate
    {
      get { return _certificate; }
      set { _certificate = value; }
    }

    [Browsable(false)]
    public string CertificateThumb
    {
      get { return (_certificate == null) ? null : _certificate.Thumbprint; }
      set { _certificate = FindCertificate(value); }
    }

    protected X509Certificate2 FindCertificate(string certificateThumb)
    {
      X509Store localStore = new X509Store(StoreLocation.LocalMachine);
      try
      {
        localStore.Open(OpenFlags.ReadOnly);

        X509Certificate2Collection foundCertificates =
          localStore.Certificates.Find(X509FindType.FindByThumbprint, certificateThumb, false);

        if (foundCertificates.Count == 0)
          throw new ArgumentException("certificateThumb",
            string.Format("Certificate with thumb {0} is not found", certificateThumb));

        return foundCertificates[0];
      }
      finally
      {
        localStore.Close();
      }
    }

    [XmlIgnore]
    [Browsable(false)]
    public bool IsSecure
    {
      get { return _certificate != null; }
    }

    protected IPEndPoint endPoint;
    
    [XmlIgnore]
		[Browsable(false)]
		public IPEndPoint EndPoint
		{
			get { return endPoint; }
		}

		[TypeConverterAttribute(typeof(IPAddressConverter))]
		public string Address
		{
			get { return endPoint.Address.ToString(); }
			set { endPoint.Address = IPAddress.Parse(value); }
		}

		public int Port
		{
			get { return endPoint.Port; }
			set { endPoint.Port = value; }
		}

		public override string ToString()
		{
			return endPoint.ToString();
		}
	}
}
