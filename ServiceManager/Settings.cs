using System;
using System.Xml.Serialization;
using derIgel.NNTP;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// Enumeration for service's start mode
	/// </summary>
	public enum StartupType {Auto, Manual, Disabled}

	public class Settings : NNTPSettings
	{
		public Settings() : base()	{	}

		public Settings(NNTPSettings serverSettings) : base(serverSettings)	{	}

		protected StartupType startupMode;
		[XmlIgnore]
		public StartupType StartupMode
		{
			get { return startupMode; }
			set { startupMode = value; }
		}
	}
}
