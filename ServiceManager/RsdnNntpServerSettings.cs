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
		}

		protected const string defaultMachine = ".";
		protected string machine = defaultMachine;
		[Browsable(false)]
		[DefaultValue(defaultMachine)]
		public string Machine
		{
			get { return machine; }
			set	{ machine = value; }
		}

		protected const string defaultServiceName = "rsdnnntp";
		protected string serviceName = defaultServiceName;
		[Browsable(false)]
		[DefaultValue(defaultServiceName)]
		public string ServiceName
		{
			get { return serviceName; }
			set {	serviceName = value; }
		}

		protected StartupType startupMode;

		[Category("Others")]
		[DefaultValue(StartupType.Auto)]
		[Description("How server starts")]
		[XmlIgnore]
		public StartupType StartupMode
		{
			get { return startupMode; }
			set { startupMode = value; }
		}
	}
}