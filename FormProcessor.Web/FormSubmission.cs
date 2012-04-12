using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Common.Logging;
using Microsoft.Security.Application;
using Encoder = Microsoft.Security.Application.Encoder;

namespace FormProcessor
{
	[XmlType("formData")]
	public class FormSubmission
	{
		static readonly ILog _log = LogManager.GetCurrentClassLogger();

		public FormSubmission()
		{
			Meta = new FormMeta();
			Data = new FormData();
		}

		[XmlElement(ElementName = "meta")]
		public FormMeta Meta{get;set;}

		[XmlElement(ElementName = "data")]
		public FormData Data{get;set;}

		/// <summary>
		/// Creates a new <see cref="FormSubmission"/> object from the POSTed form
		/// </summary>
		/// <param name="formID"></param>
		/// <param name="request"></param>
		/// <param name="requiredFields"></param>
		/// <returns></returns>
		public static FormSubmission Create(Guid formID, HttpRequest request, string requiredFields)
		{
			IList<string> required = string.IsNullOrWhiteSpace(requiredFields) ? new List<string>() : new List<string>(requiredFields.Split(','));
			_log.Trace(m => m("{0} required fields: {1}", required.Count, requiredFields));

			FormSubmission form = new FormSubmission
			                      	{
			                      			Meta =
			                      					{
			                      							ID = formID,
			                      							Referrer = request.UrlReferrer != null ? request.UrlReferrer.AbsoluteUri : "(empty)",
																					// the following conversion forces the DateTime to be handled as a local time (rather
																					// than UTC) in XML serialization. - 12/20/2011, shawn.south@bellevuecollege.edu
			                      							Datetime = XmlConvert.ToDateTime(DateTime.Now.ToString("s"), XmlDateTimeSerializationMode.RoundtripKind),
			                      							ClientIP = request.UserHostAddress
			                      					}
			                      	};
			_log.Trace(m => m("Meta data added:\n\nID = {0}\nReferrer = {1}\nDatetime = {2}\nClientIP = {3}", form.Meta.ID, form.Meta.Referrer, form.Meta.Datetime, form.Meta.ClientIP));

			_log.Debug(m => m("Processing {0} POST variables...", request.Form.AllKeys.Length));
			foreach (string name in request.Form.AllKeys)
			{
				// filter out form meta data and ASP.NET internal values
				if (!name.StartsWith("__"))
				{
					string fieldValue = request.Form[name];

					_log.Trace(m => m("Creating form submission field: '{0}'", fieldValue));
					if (name.StartsWith(Utility.META_FIELD_PREFIX))
					{
						form.Meta.Fields.Add(FormField.Create(name.Substring(Utility.META_FIELD_PREFIX.Length), fieldValue ?? "(blank)" ));
					}
					else
					{
						form.Data.Fields.Add(FormField.Create(name, fieldValue, name.StartsWith(Utility.REQUIRED_FIELD_PREFIX) || required.Contains(name)));
					}
				}
				else
				{
					_log.Trace(m => m("Ignoring ASP.NET internal variable: {0}", name));
				}
			}

			return form;
		}

		/// <summary>
		/// Serializes the current <see cref="FormSubmission"/> into an <see cref="XmlDocument"/>
		/// </summary>
		/// <returns></returns>
		public XmlDocument ToXmlDoc()
		{
			XmlDocument doc = new XmlDocument();

			using (Stream memoryStore = new MemoryStream())
			{
				XmlSerializer xs = new XmlSerializer(typeof(FormSubmission));
				XmlWriter xmlWriter = new XmlTextWriter(memoryStore, Encoding.Unicode);
				xs.Serialize(xmlWriter, this);
				
				// reset the read postition so .Load() will start at the beginning (instead of last position written to)
				memoryStore.Position = 0;

				doc.Load(memoryStore);
			}

			return doc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="missingFields"></param>
		/// <returns></returns>
		public bool IsMissingRequiredFields(out IList<string> missingFields)
		{
			missingFields = Data.Fields.Where(f => f.Required && string.IsNullOrWhiteSpace(f.Value)).Select(f => f.Name).ToList();
			return missingFields.Count > 0;
		}
	}

	[XmlType("meta")]
	public class FormMeta
	{
		public FormMeta()
		{
			Fields = new List<FormField>();
		}

		[XmlAttribute(AttributeName = "id")]
		public Guid ID{get;set;}

		[XmlAttribute(AttributeName = "referrer")]
		public string Referrer{get;set;}

		[XmlAttribute(AttributeName = "datetime")]
		public DateTime Datetime{get;set;}

		[XmlAttribute(AttributeName = "clientIP")]
		public string ClientIP{get;set;}

		[XmlArray("fields")]
		public List<FormField> Fields{get;set;}
	}

	[XmlType("data")]
	public class FormData
	{
		public FormData()
		{
			Fields = new List<FormField>();
		}

		[XmlArray("fields")]
		public List<FormField> Fields{get;set;}
	}

	[XmlType("field")]
	public class FormField
	{
		static readonly ILog _log = LogManager.GetCurrentClassLogger();

		[XmlAttribute(AttributeName = "name")]
		public string Name{get;set;}

		[XmlText]
		public string Value{get;set;}

		public bool Required{get;set;}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="required"></param>
		/// <returns></returns>
		static public FormField Create(string name, string value, bool required = false)
		{
			_log.Trace(m => m("Creating {0} form field: '{1}' = '{2}'...", required ? "required" : string.Empty, name, value));
			
			return new FormField
			       	{
			       			// strip off the "req_" prefix, if present
			       			Name = Encoder.XmlAttributeEncode(name.StartsWith(Utility.REQUIRED_FIELD_PREFIX) ? name.Substring(Utility.REQUIRED_FIELD_PREFIX.Length) : name),
			       			Value = Encoder.XmlEncode(value),
			       			Required = required
			       	};
		}
	}
}
