using System.Xml;

namespace FormProcessor
{
	/// <summary>
	/// 
	/// </summary>
	public interface IXmlValidator
	{
		XmlValidatorStatus Validate(XmlNode node);
	}

	/// <summary>
	/// 
	/// </summary>
	public enum XmlValidatorStatus
	{
		Success = 0,
		InvalidType = 2,
		FailedValidation = 4
	}
}