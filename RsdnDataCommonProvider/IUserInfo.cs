using System;

namespace Rsdn.RsdnNntp
{
	public interface IUserInfo
	{
		int ID
		{
			get;
		}

		string Origin
		{
			get;
		}

		string Name
		{
			get;
		}
		
		string Password
		{
			get;
			set;
		}

		FormattingStyle MessageFormat
		{
			get;
		}
	}
}