using System;

namespace Rsdn.Mime
{
	/// <summary>
	/// MIME message body interface.
	/// </summary>
	public interface IBody
	{
		/// <summary>
		/// Get body of MIME message.
		/// </summary>
		/// <returns></returns>
		string GetBody();
	}
}
