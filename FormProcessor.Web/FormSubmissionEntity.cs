using System;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace FormProcessor
{
	[Table("SubmittedForm")]
	public class FormSubmissionEntity
	{
		[Key]
		public Guid ID{get;set;}

		public Guid FormID{get;set;}

		public string Referrer{get;set;}

		public DateTime Datetime{get;set;}

		public string ClientIP{get;set;}

		/// <summary>
		/// The entire form stored as an <see cref="XmlDocument"/>
		/// </summary>
		public string Data{get;set;}
	}
}