using System;
using System.Collections.Generic;
using System.Text;

using Antlr.StringTemplate;
using log4net;

namespace Rsdn.RsdnNntp
{
	internal class TemplateLogger : IStringTemplateErrorListener
	{
		private ILog _logger;
		public TemplateLogger(ILog logger)
		{
			_logger = logger;
		}

		public void Error(string msg, Exception e)
		{
			_logger.Error(msg, e);
		}

		public void Warning(string msg)
		{
			_logger.Warn(msg);
		}
	}
}
