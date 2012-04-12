using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using Common.Logging;
using FormProcessor.Config;
using Microsoft.Security.Application;
using Ctc.Ods.Extensions;

namespace FormProcessor
{
	public partial class Gateway : System.Web.UI.Page
	{
		private readonly string FORM_ID_FIELD_NAME = string.Concat(Utility.META_FIELD_PREFIX, "FormID");
		private readonly string DB_ID_PARAMETER = "id";

		readonly ILog _log = LogManager.GetCurrentClassLogger();
		private FormsSettings _formsConfig;

		private string _currentReferrer;

		/// <summary>
		/// Reference to the &lt;forms&gt; settings in the .config file
		/// </summary>
		protected FormsSettings FormsConfig
		{
			get {return _formsConfig ?? (_formsConfig = Global.FormsConfig);}
		}

		/// <summary>
		/// Placeholder for any content that needs to be displayed to the user
		/// </summary>
		protected string UserMessage{get;set;}

		/// <summary>
		/// The URL of the HTML form which called this page
		/// </summary>
		protected string CurrentReferrer
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_currentReferrer))
				{
					_currentReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.GetAbsoluteUriWithoutQuery() : "(empty)";
				}
				return _currentReferrer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
// ReSharper disable InconsistentNaming
		protected void Page_Load(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
		{

			if (Request.Form.Keys != null && Request.Form.Keys.Count > 0)
			{
				_log.Trace(m => m("Request contains POSTed data"));

				if (Request.Form.AllKeys.Contains(FORM_ID_FIELD_NAME))
				{
					_log.Trace(m => m("POSTed form includes a Form ID"));

					Guid formId;
					FormSettings formSettings = GetFormSettings(out formId);

					if (formSettings != null)
					{
						_log.Debug(m => m("Settings found for Form ID [{0}]", formId));

						// check to make sure we're coming from a valid referrer
						// (using .Where() allows us to Upper each value, where the simpler .Contains() doesn't)
						// TODO: This evaluation doesn't appear to be case insensitive as intended
						if (formSettings.Referrers.Urls.Where(u => u.ToUpper() == CurrentReferrer.ToUpper()).Count() > 0)
						{
							_log.Debug(m => m("Referrer [{0}] is valid", CurrentReferrer));

							try
							{
								_log.Trace(m => m("Generating form submission..."));
								FormSubmission form = FormSubmission.Create(formId, Request, formSettings.RequiredFields);
								
								_log.Trace(m => m("Converting to XML..."));
								XmlDocument xmlDoc = form.ToXmlDoc();

								_log.Trace(m => m("Saving to database..."));
								Guid recordID = SaveToDatabase(form, xmlDoc);
								_log.Trace(m => m("Saved to database. Record ID: [{0}]", recordID));

								// check for any required fields that are missing values
								IList<string> missing;
								if (form.IsMissingRequiredFields(out missing))
								{
									_log.Error(m => m("Required fields are missing:\n{0}", string.Join(", ", missing.ToArray())));
									DisplayResult(formSettings, formSettings.Error, recordID, missing);
								}
								else
								{
									_log.Trace(m => m("No required fields missing"));
									if (formSettings.Email.Enabled)
									{
										_log.Trace(m => m("Sending e-mail..."));
										if (!SendEmails(formSettings, xmlDoc))
										{
											_log.Error(m => m("Failed to send e-mail."));
											DisplayResult(formSettings, formSettings.Error, recordID);
											return;	// abort
										}
										_log.Debug(m => m("E-mail successfully sent"));
									}
									DisplayResult(formSettings, formSettings.Success, recordID);
								}
							}
							catch (Exception ex)
							{
								_log.Error(m => m("Unexpected error occurred: {0}", ex));
								ReturnHttpError(500, endResponse: false);
							}
						}
						else
						{
							_log.Warn(m => m("Denied attempt to use form ID '{0}' from URL '{1}'", formId, CurrentReferrer));
							ReturnHttpError(403, "Unrecognized form");
						}
					}
					else
					{
						_log.Warn(m => m("Unable to find settings for form ID '{0}' from URL '{1}'", formId, CurrentReferrer));
						ReturnHttpError(403, "Unrecognized form");
					}
				}
				else
				{
					_log.Warn(m => m("Denied attempt to use form w/ no ID from URL '{0}'", CurrentReferrer));
					ReturnHttpError(403, "Unrecognized form");
				}
			}
			else
			{
				_log.Warn(m => m("Form submitted from URL {0} does not contain any data.", CurrentReferrer));
				ReturnHttpError(400, "Form does not contain any data.");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="result"></param>
		/// <param name="recordID"></param>
		/// <param name="fields"></param>
		private void DisplayResult(FormSettings settings, ResultActionBase result, Guid recordID, IEnumerable<string> fields = null)
		{
			_log.Debug(m => m("Performing POST result action ({0})...", result.Action.ToString("G")));
			switch (result.Action)
			{
				case ActionOption.Message:
					UserMessage = fields != null ? Utility.ApplyTemplateSubstitution(result.MessageText, fields) : result.MessageText;
					_log.Trace(m => m("User message: {0}", UserMessage));

					if (!string.IsNullOrWhiteSpace(result.Link))
					{
						if (result.Link.ToUpper() == "${REFERRER}")
						{
							SetReturnLink(CurrentReferrer, settings.Database.ReturnIdToCaller ? recordID : Guid.Empty);
						}
						else
						{
							SetReturnLink(Encoder.HtmlAttributeEncode(result.Link), settings.Database.ReturnIdToCaller ? recordID : Guid.Empty);
						}
					}
					break;

				case ActionOption.Redirect:
					if (result.Link == "${REFERRER}")
					{
						ForceRedirect((recordID != null && settings.Database.ReturnIdToCaller) ? string.Format("{0}?{1}={2}", CurrentReferrer, DB_ID_PARAMETER, recordID) : CurrentReferrer);
					}
					else
					{
						// valid URLs should conform to attribute value rules
						string link = Encoder.HtmlAttributeEncode((recordID != null && settings.Database.ReturnIdToCaller) ? string.Format("{0}?{1}={2}", result.Link, DB_ID_PARAMETER, recordID) : result.Link);

						if (Utility.IsUrl(link))
						{
							ForceRedirect(link);
						}
						else
						{
							UserMessage = "Your form was submitted, but we are unable to send you to the next page.";
							_log.Error(m => m("Cannot redirect user. Invalid URL: <{0}>", link));
							ReturnHttpError(500, endResponse: false);
						}
					}
					break;

				default:
					throw new ApplicationException("Unrecognized Result Action");
			}
		}

		/// <summary>
		/// Replacement for Response.Redirect() when the latter is not working properly
		/// </summary>
		/// <param name="url"></param>
		/// <remarks>
		/// Sometimes instead of actually redirecting the user, the browser displays the message "Object moved" with
		/// link to the redirect destination. Since this is not desired behavior, I've implemented this function
		/// according to this KB article: http://support.microsoft.com/?id=193489 and this forum post:
		/// http://forums.asp.net/t/1271142.aspx/1
		/// </remarks>
		private void ForceRedirect(string url)
		{
			Response.BufferOutput = Response.Buffer = true;
			Response.Clear();
			Response.Status = "302 Object moved";
			Response.AddHeader("Location", url);
			Response.Write("<html><head>");
			Response.Write(string.Format("<meta http-equiv='refresh' content='0;url={0}'/>", url));
			Response.Write(string.Format("<script type='text/javascript'>window.location='{0}';</script>", url));
			Response.Write("</head></html>");
			//Response.Flush();
			//Response.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="linkText"></param>
		/// <param name="recordID"></param>
		/// <returns></returns>
		private void SetReturnLink(string linkText, Guid recordID)
		{
			if (string.IsNullOrWhiteSpace(linkText))
			{
				ReturnUrl.Visible = ReturnLink.Visible = false;	// disable both
				_log.Trace(m => m("Link text is empty - hiding."));
			}
			else
			{
				if (Utility.IsUrl(linkText))
				{
					ReturnUrl.HRef = (recordID != null && recordID != Guid.Empty) ? string.Format("{0}?{1}={2}", linkText, DB_ID_PARAMETER, recordID) : linkText;
					ReturnUrl.Visible = true;
					_log.Trace(m => m("Link text is a raw URL. Set href to [{0}]", ReturnUrl.HRef));
				}
				else
				{
					// TODO: inject recordID GET prameter into link href
					ReturnLink.Text = Sanitizer.GetSafeHtml(linkText);
					ReturnUrl.Visible = false;
					_log.Trace(m => m("Link text is an anchor tag: [{0}]", linkText));
				}
				// toggle the other link
				ReturnLink.Visible = !(ReturnUrl.Visible);
			}
		}

		/// <summary>
		/// Saves form data (including XML representation) to the database
		/// </summary>
		/// <param name="form"></param>
		/// <param name="data"></param>
		private Guid SaveToDatabase(FormSubmission form, XmlDocument data)
		{
			using (FormProcessorDbContext db = new FormProcessorDbContext())
			{
				FormSubmissionEntity formData = new FormSubmissionEntity
				                                	{
																						ID = Guid.NewGuid(),
				                                		FormID = form.Meta.ID,
				                                		Referrer = form.Meta.Referrer,
				                                		Datetime = form.Meta.Datetime,
				                                		ClientIP = form.Meta.ClientIP,
																						Data = data.InnerXml
				                                	};
				db.Forms.Add(formData);
				db.SaveChanges();
				_log.Debug(m => m("Form submission successfully saved to the database. (ID = {0})", formData.ID));

				return formData.ID;
			}
		}

		/// <summary>
		/// Sends form POST as e-mail to specified recipients
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="data"></param>
		private bool SendEmails(FormSettings settings, XmlDocument data)
		{
			string formEmail = TransformToEmail(settings, data);
			_log.Trace(m => m("To: {0}\nBody:\n{1}", settings.Email.To, formEmail));

			try
			{
				return NotificationHelper.SendEmail(settings.Email.To, "Test form processor", formEmail.Replace("&amp;", "&"));	// HACK: to deal w/ encoded URLs
			}
			catch (Exception ex)
			{
				_log.Error(m => m("Failed to e-mail the following form to '{0}'\n{1}-----\nException details:\n-----{2}", settings.Email.To, data.InnerXml, ex));
				UserMessage = "There was an error attempting to send your form.";
			}

			return false;
		}

		/// <summary>
		/// Apply XSLT template for E-mail
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="form"></param>
		/// <returns></returns>
		private string TransformToEmail(FormSettings settings, XmlDocument form)
		{
			if (string.IsNullOrWhiteSpace(settings.Email.TemplateName)) {
				throw new NullReferenceException(string.Format("No Email xslTemplateName is specified for form ID {0}", settings.ID));
			}

			string xsltFilename = Server.MapPath(string.Format("templates\\{0}.xslt", settings.Email.TemplateName));
			XslCompiledTransform xslt = Utility.LoadXslTempate(xsltFilename);
			
			try
			{
				using (StringWriter output = new StringWriter())
				{
					xslt.Transform(form.CreateNavigator(), null, output);
					return output.ToString();
				}
			}
			catch (XmlSchemaValidationException ex)
			{
				_log.Error(m => m("{0}: XSLT is not valid.\n{1}", xsltFilename, ex));
			}
			catch (XsltException ex)
			{
				_log.Error(m => m("{0}: XSLT is not valid.\n{1}", xsltFilename, ex));
			}
			catch (XmlException ex)
			{
				_log.Error(m => m("{0}: XML is not valid.\n{1}", xsltFilename, ex));
			}
			catch (XmlSchemaException ex)
			{
				_log.Error(m => m("{0}: Schema is not valid.\n{1}", xsltFilename, ex));
			}
			catch (ArgumentNullException ex)
			{
				_log.Error(m => m("{0}: XSLT is blank.\n{1}", xsltFilename, ex));
			}

			// TODO: will an empty string throw an exception further up the chain?
			_log.Warn(m => m("XML transform returning an empty string."));
			return string.Empty;
		}

		/// <summary>
		/// Retrieves the settings for the form currently being POSTed
		/// </summary>
		/// <param name="formId"></param>
		/// <returns></returns>
		private FormSettings GetFormSettings(out Guid formId)
		{
			if (Guid.TryParse(Request.Form[FORM_ID_FIELD_NAME], out formId))
			{
				Guid id = formId;	// can't use ref/out paramters in the lambda expression below
				IEnumerable<FormSettings> forms = FormsConfig.Forms.Where(postingForm => postingForm.ID.Equals(id));
				
				return forms.Count() > 0 ? forms.Single() : null;
			}
			return null;
		}

		/// <summary>
		/// Sends specified HTTP status code back to the client and aborts any remaining Response content
		/// </summary>
		/// <param name="statusCode"></param>
		/// <param name="statusDescription"></param>
		/// <param name="endResponse"></param>
		private void ReturnHttpError(int statusCode, string statusDescription = null, bool endResponse = true)
		{
//			throw new HttpException(statusCode, statusDescription ?? string.Concat("HTTP ", statusCode.ToString()));

			string statusCodeDesc = string.Concat("HTTP ", statusCode.ToString());

			Response.StatusCode = statusCode;
		  Response.StatusDescription = statusDescription ?? statusCodeDesc;
			Response.Write("<html><body>");
			Response.Write(statusDescription != null ? string.Concat(statusCodeDesc, ": ", statusDescription) : statusCodeDesc);
			Response.Write("</body></html>");
			Response.Flush();

			if (endResponse) {
			  Response.End();
			}
		}
	}
}