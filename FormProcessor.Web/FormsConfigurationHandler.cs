using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using FormProcessor.Config;

namespace FormProcessor
{
	/// <summary>
	/// 
	/// </summary>
	public class FormsConfigurationHandler : IConfigurationSectionHandler
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="configContext"></param>
		/// <param name="section"></param>
		/// <returns></returns>
		public object Create(object parent, object configContext, XmlNode section)
		{
			FormsSettings result = null;
			if (section == null)
					return result;
			XmlSerializer ser = new XmlSerializer(typeof(FormsSettings));

			using (XmlNodeReader reader = new XmlNodeReader(section))
			{
				result = (FormsSettings)ser.Deserialize(reader);

				return result;
			}
		}
	}
}