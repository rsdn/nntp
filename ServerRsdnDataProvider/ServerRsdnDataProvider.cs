using System;

using Rsdn.RsdnNntp;

namespace Rsdn.ServerRsdnNntp
{
	/// <summary>
	/// Summary description for ServerRsdnDataProvider.
	/// </summary>
	public class ServerRsdnDataProvider : RsdnDataProvider
	{
		public ServerRsdnDataProvider() : base()
		{
		}
	
		public override Rsdn.RsdnNntp.RsdnService.auth_info RsdnAuthentificate(string user, string pass, System.Net.IPAddress ip)
		{
			return base.RsdnAuthentificate (user, pass, ip);
		}
	}
}
