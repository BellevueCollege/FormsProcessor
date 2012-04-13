using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using Common.Logging;
using FormProcessor.Config;

namespace FormProcessor
{
	public class Global : HttpApplication
	{
		private ILog _log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// 
		/// </summary>
		static public FormsSettings FormsConfig
		{
			get {return (HttpContext.Current.Application["FormsSettings"] as FormsSettings);}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Start(object sender, EventArgs e)
		{
			// Maintain a list of IP connection attempts
			Application["RequestAttempts"] = new List<RequestAttempt>();

			// Load at application start so we don't have to keep loading on every page load.
			Application["FormsSettings"] = ConfigurationManager.GetSection("forms") as FormsSettings;
			_log.Debug(m => m("Starting FormProcessor application, version {0}", Assembly.GetExecutingAssembly().GetName().Version));
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			string clientIP = Request.UserHostAddress;
			IList<RequestAttempt> attempts = (Application["RequestAttempts"] as IList<RequestAttempt>);

			if (attempts != null)
			{
				attempts.Expire();	// clean out old attempts (so this List doesn't grow too large)

				if (!string.IsNullOrWhiteSpace(clientIP))
				{
					int count = attempts.Count(clientIP);
					_log.Trace(m => m("IP: [{0}] attempts: {1}", clientIP, count));

					if (count > RequestAttempt.MaxAllowed)
					{
						_log.Warn(m => m("IP Address [{0}] has made too many consecutive requests. Access temporarily locked.", clientIP));
						// TODO: abort the request
					}
				}
				else
				{
					_log.Warn(m => m("Unable to determine Client IP."));
				}
			}
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();
			if (ex != null)
			{
				_log.Fatal(m => m("Application-level error: {0}", ex));
			}
			else
			{
				_log.Warn(m => m("Global Application_Error() event handler was invoked, but GetLastError() returned null."));
			}
		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}