using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BellevueCollege
{
	///<summary>
	///</summary>
	public class Toolkit
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool IsValidIntegerQuerystring(System.Object obj)
		{
			int temp;
			if (obj == null)
				return false;
			if (string.IsNullOrEmpty(obj.ToString().Trim()))
				return false;
			if (!int.TryParse(obj.ToString(), out temp))
				return false;
			if (obj.ToString().Contains("."))
				return false;

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void RedirectOnSessionTimeout()
		{
			if (SessionHelper.User == null)
				HttpContext.Current.Response.Redirect("~/Default.aspx");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strDate"></param>
		/// <returns></returns>
		public static bool IsValidSQLDate(string strDate)
		{
			// Returns True if the Date is Valid for SQL (Matches Pattern)
			strDate = strDate.Trim();
			return Regex.IsMatch(strDate, "^(?:(?:(?:0?[13578]|1[02])(\\/|-|\\.)31)\\1|(?:(?:0?[13-9]|1[0-2])(\\/|-|\\.)(?:29|30)\\2))(?:(?:1[6-9]|[2-9]\\d)?\\d{2})$|^(?:0?2(\\/|-|\\.)29\\3(?:(?:(?:1[6-9]|[2-9]\\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:(?:0?[1-9])|(?:1[0-2]))(\\/|-|\\.)(?:0?[1-9]|1\\d|2[0-8])\\4(?:(?:1[6-9]|[2-9]\\d)?\\d{2})$");
		}

		///<summary>
		///</summary>
		///<param name="strValue"></param>
		///<returns></returns>
		public static bool IsAlphaNumeric(string strValue)
		{
			return Regex.IsMatch(strValue, "[A-Za-z0-9]");
		}

		///<summary>
		///</summary>
		///<param name="strValue"></param>
		///<param name="intAllowedPrecision"></param>
		///<returns></returns>
		public static bool IsExpectedNumber(string strValue, int intAllowedPrecision)
		{
			if (string.IsNullOrEmpty(strValue))
			{
				return false;
			}
			else if (intAllowedPrecision == 0)
			{
				return Regex.IsMatch(strValue, "^d{0,3}");
			}
			else
			{
				return Regex.IsMatch(strValue, "^\\d{0,3}(\\.\\d{1," + intAllowedPrecision.ToString() + "})?$");
			}
		}

		///<summary>
		///</summary>
		///<param name="strValue"></param>
		///<returns></returns>
		public static bool IsExpectedPIN(string strValue)
		{
			return Regex.IsMatch(strValue, "^\\d{1,6}$");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strInput"></param>
		/// <returns></returns>
		public static string PrepareForDisplay(string strInput)
		{
			//return strInput.Replace(strInput.Replace(DeNull(strInput), Environment.NewLine, "<br />"), "  ", "&nbsp; ");
			return "TODO";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strEmailAddress"></param>
		/// <returns></returns>
		public static bool IsValidEmailAddress(string strEmailAddress)
		{
			bool isValidEmailAddress = Regex.IsMatch(strEmailAddress.Trim(), String.Format("^{0}$", Config.CurrentConfig.Sections.ValidationPatterns.Email));
			LogHelper.Log.Debug("'{0}' {1} valid address", strEmailAddress, isValidEmailAddress ? "is" : "is not a");
			return isValidEmailAddress;
		}

		/// <summary>
		/// Converts an empty/blank <see cref="String"/> to <b>null</b>
		/// </summary>
		/// <param name="str">A string value that might be empty or contain only whitespace</param>
		/// <returns></returns>
		/// <remarks>
		/// Because this method may potentially be used alot within an application, <paramref name="str"/> is passed by reference to
		/// reduce the memory overhead of creating a copy of the parameter's value.
		/// </remarks>
		static public string Nullify(ref string str)
		{
			return String.IsNullOrWhiteSpace(str) ? null : str;
		}

		/// <summary>
		/// Converts an empty/blank <see cref="String"/> to <b>null</b>
		/// </summary>
		/// <param name="str">A string value that might be empty or contain only whitespace</param>
		/// <returns></returns>
		/// <remarks>
		/// This method is a wrapper which provides a seamless way to values that cannot be passed <i>by reference</i> (e.g.
		/// ASP.NET Textbox content)
		/// </remarks>
		/// <seealso cref="Nullify(ref string)"/>
		static public string Nullify(string str)
		{
			return Nullify(ref str);
		}

		/// <summary>
		/// Identifies whether the specified <see cref="String"/> can be converted to a number
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		static public bool IsNumber(string str)
		{
			int bucket;
			if (int.TryParse(str, out bucket))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Capitalizes the first letter of each word in a <see cref="String"/>
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		/// <remarks>
		/// This method utilizes code taken from <a href="http://channel9.msdn.com/forums/TechOff/252814-Howto-Capitalize-first-char-of-words-in-a-string-NETC/">this
		/// forum post</a>.
		/// </remarks>
		static public string TitleCase(string str)
		{
			if(String.IsNullOrWhiteSpace(str)) {
				throw new ArgumentNullException("value");
			}

			StringBuilder result = new StringBuilder(str.ToLower());
			result[0] = char.ToUpper(result[0]);
			 
			for( int i = 1; i < result.Length; ++i )
			{
				if( char.IsWhiteSpace(result[i - 1]) ) {
					result[i] = char.ToUpper(result[i]);
				}
			}

			return result.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dll"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		static public string GetApplicationVersion(Assembly dll, string format = null)
		{
			LogHelper.Log.Trace("Toolkit::GetApplicationVersion(dll=[{0}], format='{1}')", dll != null ? dll.GetName().FullName : "null", format);

			Version ver = dll.GetName().Version;
			string versionOutput = format;

			if (String.IsNullOrWhiteSpace(format))
			{
				versionOutput = ver.ToString();
			}
			else
			{
				CheckForAndReplace(ref versionOutput, "{MAJOR}", ver.Major);
				CheckForAndReplace(ref versionOutput, "{MINOR}", ver.Minor);
				CheckForAndReplace(ref versionOutput, "{REVISION}", ver.Revision);
				CheckForAndReplace(ref versionOutput, "{BUILD}", ver.Build);
			}

			LogHelper.Log.Debug("Formatted application version as '{0}'", versionOutput);
			return versionOutput;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="token"></param>
		/// <param name="component"></param>
		private static void CheckForAndReplace(ref string buffer, string token, int component)
		{
			LogHelper.Log.Trace("=> Version::CheckForAndReplace(buffer='{0}', token='{1}', component={2})", buffer, token, component);

			if (buffer.Contains(token))
			{
				LogHelper.Log.Trace("buffer contains '{0}' -> replacing with [{1}]...", token, component);
				buffer = buffer.Replace(token, component.ToString());
			}
		}
	}
}