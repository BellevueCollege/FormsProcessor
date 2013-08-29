/*
Copyright (C) 2011 Bellevue College

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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