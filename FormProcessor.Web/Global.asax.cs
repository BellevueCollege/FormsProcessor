using System;
using System.Configuration;
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
			// Load at application start so we don't have to keep loading on every page load.
			Application["FormsSettings"] = ConfigurationManager.GetSection("forms") as FormsSettings;
			_log.Debug(m => m("Starting FormProcessor application, version {0}", Assembly.GetExecutingAssembly().GetName().Version));
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

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