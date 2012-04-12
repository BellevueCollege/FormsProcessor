using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace BellevueCollege.Config
{
	/// <summary>
	/// Loads a <see cref="EmailSettings"/> <see cref="ConfigurationSection"/> from .config file into a <see cref="EmailSettings"/> object
	/// </summary>
	/// <seealso cref="EmailSettings"/>
	class EmailSettingsConfigHandler : IConfigurationSectionHandler
	{
		/// <summary>
		/// Loads a <see cref="ConfigurationSection"/> from .config file into a <see cref="EmailSettings"/> object
		/// </summary>
		/// <param name="parent">Parent object</param>
		/// <param name="configContext">Configuration context object</param>
		/// <param name="section">EmailSettings XML node from the configuration file</param>
		/// <returns>An <see cref="EmailSettings"/> configuration object</returns>
		/// <remarks>
		/// This method is invoked from the .NET configuration system when parsing the .config file.
		/// </remarks>
		/// <seealso cref="EmailSettings"/>
		public object Create(object parent, object configContext, XmlNode section)
		{
			EmailSettings result = null;
			if (section == null)
					return result;
			XmlSerializer ser = new XmlSerializer(typeof(EmailSettings));

			using (XmlNodeReader reader = new XmlNodeReader(section))
			{
				result = (EmailSettings)ser.Deserialize(reader);

				return result;
			}
		}
	}
}
