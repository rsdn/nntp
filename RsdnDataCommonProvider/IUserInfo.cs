using System;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Generic (common) user info.
	/// </summary>
	public interface IUserInfo
	{
		/// <summary>
		/// User ID.
		/// </summary>
		int ID
		{
			get;
		}

		/// <summary>
		/// User's origin.
		/// </summary>
		string Origin
		{
			get;
		}

		/// <summary>
		/// User name.
		/// </summary>
		string Name
		{
			get;
		}
		
		/// <summary>
		/// User password.
		/// </summary>
		string Password
		{
			get;
			set;
		}

		/// <summary>
		/// Preffred user's message format.
		/// </summary>
		FormattingStyle MessageFormat
		{
			get;
		}
	}
}