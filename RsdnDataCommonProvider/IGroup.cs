using System;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Generic (common) group info.
	/// </summary>
	public interface IGroup
	{
		/// <summary>
		/// Time, when group was created.
		/// </summary>
		DateTime Created
		{
			get;
		}

		/// <summary>
		/// Group name.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Number of first article in this group.
		/// </summary>
		int FirstArticleNumber
		{
			get;
		}

		/// <summary>
		/// Number of last article in this group.
		/// </summary>
		int LastArticleNumber
		{
			get;
		}
	}
}