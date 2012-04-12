using System.Xml;
using System.Xml.Schema;
using Common.Logging;

namespace FormProcessor
{
	class XmlDocumentValidator : IXmlValidator
	{
		readonly private ILog _log = LogManager.GetCurrentClassLogger();

		readonly private string _schemaNamespace;
		readonly private string _schemaFilename;
		private XmlValidatorStatus _validationStatus;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="schemaNamespace"></param>
		/// <param name="schemaFilename"></param>
		public XmlDocumentValidator(string schemaNamespace, string schemaFilename)
		{
			_schemaNamespace = schemaNamespace;
			_schemaFilename = schemaFilename;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public XmlValidatorStatus Validate(XmlNode node)
		{
			_validationStatus = XmlValidatorStatus.Success;

			if (node.NodeType != XmlNodeType.Document)
			{
				_validationStatus = XmlValidatorStatus.InvalidType;
			}
			else
			{
				XmlDocument xmlDoc = node as XmlDocument;
				if (xmlDoc != null)
				{
					xmlDoc.Schemas.Add(_schemaNamespace, _schemaFilename);
					xmlDoc.Validate(XmlValidationHandler);
				}
				else
				{
					_log.Warn(m => m("XML Document is null"));
				}
			}

			return _validationStatus;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void XmlValidationHandler(object sender, ValidationEventArgs e)
		{
			// TODO: confirm that we only hit this event handler when validation fails
			_validationStatus = XmlValidatorStatus.FailedValidation;
		}

	}
}