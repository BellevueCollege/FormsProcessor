using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using Common.Logging;

namespace FormProcessor
{
	public class Utility
	{
		public const string META_FIELD_PREFIX = "meta_";
		public const string REQUIRED_FIELD_PREFIX = "req_";
		readonly static ILog _log = LogManager.GetCurrentClassLogger();

		static public bool IsValidEmailAddress(string address)
		{
			bool isValidEmailAddress = Regex.IsMatch(address.Trim(), ConfigurationManager.AppSettings["EmailPattern"]);
			_log.Debug(m => m("'{0}' {1} a valid address", address, isValidEmailAddress ? "is" : "is not"));
			return isValidEmailAddress;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xsltFilename"></param>
		/// <returns></returns>
		static public XslCompiledTransform LoadXslTempate(string xsltFilename)
		{
			using (FileStream fs = new FileStream(xsltFilename, FileMode.Open, FileAccess.Read))
			{
				XslCompiledTransform xslt = new XslCompiledTransform();
				XmlReaderSettings xmlsettings = new XmlReaderSettings
				                                	{
				                                			ConformanceLevel = ConformanceLevel.Document
				                                	};

				XmlReader reader = XmlReader.Create(fs, xmlsettings);
				xslt.Load(reader);
			
				return xslt;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageText"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		static public string ApplyTemplateSubstitution(string messageText, IEnumerable<string> data)
		{
			if (data.Count() > 0)
			{
				string missingFields = string.Concat("<ul><li>", String.Join("</li><li>", data), "</li></ul>");
				return messageText.Replace(@"${MissingFields}", missingFields);
			}
			return messageText;
		}

		static public bool IsUrl(string possibleUrl)
		{
			return Regex.IsMatch(possibleUrl, ConfigurationManager.AppSettings["UrlPattern"]);
		}
	}
}