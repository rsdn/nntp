using System;
using System.Xml.Serialization;

using Rsdn.Nntp;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Enumeration for service's start mode
	/// </summary>
	public enum StartupType {Auto, Manual, Disabled}

	public class Settings : NntpSettings
	{
		public Settings() : base()	{	}

		public Settings(NntpSettings serverSettings) : base(serverSettings)	{	}

		protected StartupType startupMode;
		[XmlIgnore]
		public StartupType StartupMode
		{
			get { return startupMode; }
			set { startupMode = value; }
		}
	}
}
