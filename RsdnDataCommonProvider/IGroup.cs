using System;

namespace Rsdn.RsdnNntp
{
	public interface IGroup
	{
		DateTime Created
		{
			get;
		}

		string Name
		{
			get;
		}

		int FirstArticleNumber
		{
			get;
		}

		int LastArticleNumber
		{
			get;
		}
	}
}