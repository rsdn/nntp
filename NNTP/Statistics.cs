using System;
using System.Collections;
using System.Xml.Serialization;

namespace derIgel.NNTP
{
	/// <summary>
	/// Summary description for Statistics.
	/// </summary>
	[Serializable]
	public class Statistics
	{
		public Statistics()
		{
			staistics = new Hashtable();
			errors = new Hashtable();
		}
		protected Hashtable staistics;
		protected Hashtable errors;

		public void AddStatistic(string command)
		{
			if (staistics[command] == null)
				staistics[command] = 0;
			staistics[command] = (int)staistics[command] + 1;
		}
		public void AddError(int errorCode, string command)
		{
			if (errors[errorCode] == null)
				errors[errorCode] = new ArrayList();

			((ArrayList)errors[errorCode]).Add(command);
		}
	}
}
