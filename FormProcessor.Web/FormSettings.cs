
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Serialization;

namespace FormProcessor.Config
{
	/// <summary>
	/// 
	/// </summary>
	[XmlType("forms")]
	public class FormsSettings
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "form")]
		public List<FormSettings> Forms{get;set;}

		/// <summary>
		/// The name of the <see cref="ConfigurationSection"/> for this object
		/// </summary>
		static public string SectionName
		{
			get
			{
				Type thisType = typeof(FormsSettings);

				System.Reflection.MemberInfo info = thisType;
				object[] attributes = info.GetCustomAttributes(typeof(XmlTypeAttribute), false);

				if (attributes != null && attributes.Length > 0 && attributes[0] != null)
				{
// ReSharper disable PossibleNullReferenceException
					return (attributes[0] as XmlTypeAttribute).TypeName;
// ReSharper restore PossibleNullReferenceException
				}
				
				throw new InvalidOperationException(string.Format("Unable to find an XmlTypeAttribute for [{0}]", thisType));
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[XmlType("form")]
	public class FormSettings
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "id")]
		public Guid ID{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "requiredFields")]
		public string RequiredFields{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "referrers")]
		public FormReferrers Referrers { get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "email")]
		public EmailAction Email{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "database")]
		public DatabaseAction Database{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "success")]
		public SuccessAction Success{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "error")]
		public ErrorAction Error{get;set;}
	}

	/// <summary>
	/// 
	/// </summary>
	[XmlType("referrers")]
	public class FormReferrers
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "enforce", DataType = "boolean")]
		public bool Enforce{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "url")]
		public List<string> Urls{get;set;}
	}

	/// <summary>
	/// 
	/// </summary>
	[XmlType("email")]
	public class EmailAction
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "enabled", DataType = "boolean")]
		public bool Enabled{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute("to")]
		public string To{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute("subject")]
		public string Subject{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute("xslTemplateName")]
		public string TemplateName{get;set;}
	}

	/// <summary>
	/// 
	/// </summary>
	[XmlType("database")]
	public class DatabaseAction
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "enabled", DataType = "boolean")]
		public bool Enabled{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "returnIdToCaller", DataType = "boolean")]
		public bool ReturnIdToCaller{get;set;}
	}

	/// <summary>
	/// 
	/// </summary>
	[XmlType("success")]
	public class SuccessAction : ResultActionBase
	{
	}

	/// <summary>
	/// 
	/// </summary>
	[XmlType("error")]
	public class ErrorAction : ResultActionBase
	{
	}

	/// <summary>
	/// 
	/// </summary>
	public enum ActionOption
	{
		[XmlEnum("message")]
		Message,

		[XmlEnum("redirect")]
		Redirect,
	}

	public class ResultActionBase
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "action")]
		public ActionOption Action{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "link")]
		public string Link{get;set;}

		/// <summary>
		/// 
		/// </summary>
		[XmlText]
		public string MessageText{get;set;}
	}
}