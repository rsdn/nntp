using System;
using System.Xml.Serialization;
using System.ComponentModel;
using derIgel.ROOT.CIMV2;
using System.Management;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// settings for application
	/// </summary>
	[XmlRoot("Settings")]
	public class RsdnNntpSettings : RsdnDataProviderSettings
	{
		public enum StartupType {Auto, Manual, Disabled}

		public RsdnNntpSettings()
		{
			service = new Service();
			ChangePath();
		}

		protected Service service;
		
		protected const string defaultMachine = ".";
		protected string machine = defaultMachine;
		[Browsable(false)]
		[DefaultValue(defaultMachine)]
		public string Machine
		{
			get { return machine; }
			set
			{
				machine = value;
				ChangePath();
			}
		}

		protected const string defaultServiceName = "rsdnnntp";
		protected string serviceName = defaultServiceName;
		[Browsable(false)]
		[DefaultValue(defaultServiceName)]
		public string ServiceName
		{
			get { return serviceName; }
			set
			{
				serviceName = value;
				ChangePath();
			}
		}

		[Category("Others")]
		[DefaultValue(StartupType.Auto)]
		[Description("How server starts")]
		[XmlIgnore]
		public StartupType StartupMode
		{
			get
			{
				service.InterrogateService();
				return (StartupType)Enum.Parse(typeof(StartupType), service.StartMode);
			}
			set
			{
				service.ChangeStartMode((value == StartupType.Auto) ? "Automatic" : value.ToString());
				ChangePath();
			}
		}

		protected void ChangePath()
		{
			service.Path = new ManagementPath(string.Format(@"\\{0}\{1}:{2}.Name=""{3}""", Machine,
				service.OriginatingNamespace, service.ManagementClassName, ServiceName));
		}
	}
}