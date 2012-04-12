using System;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web;
using BellevueCollege.Config;
using Common.Logging;

namespace FormProcessor
{
	/// <summary>
	/// 
	/// </summary>
	public class NotificationHelper
	{
		private static readonly char[] ADDRESS_DELIMITERS = new[] {';', ','};

		readonly static ILog _log = LogManager.GetCurrentClassLogger();
		private static EmailSettings _emailConfig;

		/// <summary>
		/// 
		/// </summary>
		private static EmailSettings EmailConfig
		{
			get
			{
				if (_emailConfig == null)
				{
					_emailConfig = ConfigurationManager.GetSection(EmailSettings.SectionName) as EmailSettings;
				}
				return _emailConfig;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="to"></param>
		/// <param name="subject"></param>
		/// <param name="body"></param>
		/// <param name="cc"></param>
		/// <param name="highPriority"></param>
		/// <param name="async"></param>
		public static bool SendEmail(string to, string subject, string body, string cc = null, bool highPriority = false, bool async = false)
		{
			return SendEmail(to, subject, body, EmailConfig.DefaultFromAddress, EmailConfig.UseHtml, cc, highPriority, async);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strTo"></param>
		/// <param name="strSubject"></param>
		/// <param name="strBody"></param>
		/// <param name="strFrom"></param>
		/// <param name="blnHTML"></param>
		/// <param name="strCc"></param>
		/// <param name="blnHighPriority"></param>
		/// <param name="blnAsync"></param>
		public static bool SendEmail(string strTo, string strSubject, string strBody, string strFrom, bool blnHTML, string strCc = null, bool blnHighPriority = false, bool blnAsync = false)
		{
			_log.Trace(m => m("NotificationHelper::SendEmail(strTo='{0}', strSubject='{1}', strFrom='{2}', blnHtml={3}, blnHighPriority={4}", strTo, strSubject, strFrom, blnHTML, blnHighPriority));

			if (String.IsNullOrWhiteSpace(strFrom))
			{
				throw new ArgumentNullException("strFrom", "From e-mail address cannot be empty.");
			}

			// TODO: provide a way to specify both address and display name
			MailAddress mailfrom = new MailAddress(strFrom, strFrom);
			MailMessage mailmsg = new MailMessage();

			// Assign the To addresses
			_log.Debug(m => m("Determining To: addresses... ({0})", strTo));
			string[] toAddresses = ExtractAddressesFromString(strTo);
			_log.Debug(m => m("Determining Cc: addresses... ({0})", strCc));
			string[] ccAddresses = ExtractAddressesFromString(strCc);
			
			if (EmailConfig.Mode == EmailSettings.EmailMode.Test)
			{
				_log.Debug(m => m("Test mode enabled - appending To: and CC: addresses to the body of the e-mail"));
				// append e-mail address to body when testing
				strBody = String.Concat(strBody, (blnHTML ? "\n<hr />\n" : "\n--\n"));
				AddAddresses(ref strBody, toAddresses);
				AddAddresses(ref strBody, ccAddresses);
			}
			else
			{
				_log.Debug(m => m("Adding To: addresses..."));
				AddAddresses(mailmsg.To, toAddresses);

				_log.Debug(m => m("Adding CC: addresses..."));
				AddAddresses(mailmsg.CC, ccAddresses);
			}

			_log.Debug(m => m("Assigning additional To: addresses specified in the .config... ({0})", EmailConfig.AdditionalToAddresses));
			// Add any additional To addresses specified
			AddAddresses(mailmsg.To, ExtractAddressesFromString(EmailConfig.AdditionalToAddresses));

			if (mailmsg.To.Count > 0)
			{
				mailmsg.Sender = mailmsg.From = mailfrom;

				mailmsg.IsBodyHtml = blnHTML;
				mailmsg.Priority = (blnHighPriority ? MailPriority.High : MailPriority.Normal);
				mailmsg.Subject = String.IsNullOrWhiteSpace(strSubject) ? EmailConfig.DefaultSubject : strSubject;
				mailmsg.Body = strBody;

				if (EmailConfig.Enabled)
				{
					// default to the local host if no server is specified
					SmtpClient smtp = new SmtpClient(String.IsNullOrWhiteSpace(EmailConfig.SmtpHost) ? Environment.MachineName : EmailConfig.SmtpHost);
					smtp.Port = EmailConfig.SmtpPort;

					// set authentication, if required
					if (EmailConfig.RequireSmtpAuth)
					{
						_log.Debug(m => m("SMTP authentication required - setting credentials"));
						ApplySmtpAuthenticationSettings(smtp);
					}
					if (EmailConfig.RequireSSL)
					{
						smtp.EnableSsl = true;
						_log.Debug(m => m("SMTP over SSL enabled."));
					}

					// TODO: limit the length of the values to prevent buffer overflow-like attacks
					string msgID = String.Format("To:{0}|Subject:{1}", mailmsg.To, mailmsg.Subject);
					try
					{
						if (blnAsync || EmailConfig.ForceAsync)
						{
							_log.Debug(m => m("Sending e-mail asynchronously [{0}] via [{1}:{2}]...", msgID, smtp.Host, smtp.Port));

							smtp.SendCompleted += new SendCompletedEventHandler(SmtpSend_Completed);
							smtp.SendAsync(mailmsg, msgID);

							// *WARNING* DO NOT Dispose if using SendAsync()
						}
						else
						{
							_log.Debug(m => m("Sending e-mail [{0}] via [{1}:{2}]...", msgID, smtp.Host, smtp.Port));

							smtp.Send(mailmsg);

							smtp.Dispose();
							mailmsg.Dispose();
						}
					}
					catch (Exception ex)
					{
						_log.Error(m => m("Error sending e-mail:\n{0}", ex));
						return false;
					}
				}
				else
				{
					_log.Info(m => m("E-mail notificationis disabled - simulating:\nTo: {0}\nCC: {1}\nFrom: {2} (Sender: {3})\nSubject: {4}\nPriority: {5}\nBody:\n{6}",
						                  mailmsg.To, mailmsg.CC, mailmsg.From, mailmsg.Sender, mailmsg.Subject, mailmsg.Priority, mailmsg.Body));
				}
			}
			else
			{
				_log.Warn(m => m("The message with subject '{0}' was not sent because there were no recipients", strSubject));
				return false;
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void SmtpSend_Completed(object sender, AsyncCompletedEventArgs e)
		{
			string token = (string)e.UserState;

			if (e.Cancelled)
			{
				_log.Info(m => m("E-mail was cancelled: [{0}]", token));
			}

			if (e.Error != null)
			{
				_log.Error(m => m("E-mail error: [{0}]\n{1}", token, e.Error));
			}
			else
			{
				_log.Debug(m => m("E-mail successfully sent: [{0}]", token));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="smtp"></param>
		private static void ApplySmtpAuthenticationSettings(SmtpClient smtp)
		{
			NetworkCredential cred = new NetworkCredential();

			_log.Trace(m => m("Applying AuthMode: '{0}'...", EmailConfig.AuthMode.ToString()));
			switch (EmailConfig.AuthMode)
			{
				case EmailSettings.SmtpAuthMode.Context:
					cred = CredentialCache.DefaultNetworkCredentials;
					break;

				case EmailSettings.SmtpAuthMode.WebUser:
					if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.LogonUserIdentity != null)
					{
						// impersonate the currently logged-in user just long enough to get a copy of their network credentials - so we
						// can pass those to the smtp server.
						WindowsImpersonationContext wic = WindowsIdentity.Impersonate(HttpContext.Current.Request.LogonUserIdentity.Token);
						try
						{
							cred = CredentialCache.DefaultNetworkCredentials;
						}
						finally
						{
							wic.Undo(); // restore previously used identity
						}
					}
					else
					{
						_log.Warn(m => m("Unable to retrieve Web User Identity - defaulting to current account context..."));
						cred = CredentialCache.DefaultNetworkCredentials;
					}
					break;

				case EmailSettings.SmtpAuthMode.Manual:
					cred.UserName = EmailConfig.Username;
					cred.Password = EmailConfig.Password;
					cred.Domain = EmailConfig.LoginDomain;
					break;

				default:
					_log.Warn(m => m("An unrecognized AuthMode was specified for <{0}> configuration", EmailSettings.SectionName));
					break;
			}

			if (cred != null)
			{
				_log.Debug(m => m("Setting SMTP credentials to the currently executing context ({0}\\{1})...", cred.Domain, cred.UserName));
				smtp.Credentials = cred;
			}
			else
			{
				_log.Debug(m => m("No credentials were set"));
			}

			// specify method to validate the server certificate (?)
			ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="sslpolicyerrors"></param>
		/// <returns></returns>
		/// <remarks>
		/// This code is based heavily on this MSDN Example: http://msdn.microsoft.com/en-us/library/dd633677%28v=EXCHG.80%29.aspx
		/// </remarks>
		private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			// If the certificate is a valid, signed certificate, return true.
			if (sslpolicyerrors == SslPolicyErrors.None)
			{
				_log.Trace("SSL certificate is valid.");
				return true;
			}

			// If specified, accept any and all certs - even if they're not valid
			if (EmailConfig.IgnoreInvalidSslCerts)
			{
				_log.Info("Skipping SSL validation check (Web.config setting).");
				return true;
			}

			// If there are errors in the certificate chain, look at each error to determine the cause.
			if ((sslpolicyerrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
			{
				if (chain != null && chain.ChainStatus != null)
				{
					_log.Trace("Walking the cert chain to determine what the errors are...");
					foreach (X509ChainStatus status in chain.ChainStatus)
					{
						if ((certificate.Subject == certificate.Issuer) && (status.Status == X509ChainStatusFlags.UntrustedRoot))
						{
							_log.Debug(m => m("Cert is self-signed by Untrusted authority '{0}', but will be considered valid", certificate.Issuer));
							// Self-signed certificates with an untrusted root are valid - if their public key matches those we've specified
							if (HasTrustedPublicKey(certificate))
							{
								continue;
							}
							return false;
						}
						
						if (status.Status != X509ChainStatusFlags.NoError)
						{
							_log.Warn(m => m("Cert issued by '{0}' is not valid: [{1}]", certificate.Issuer, status.Status));
							// If there are any other errors in the certificate chain, the certificate is invalid,
							// so the method returns false.
							return false;
						}
					}
				}

				// When processing reaches this line, the only errors in the certificate chain are
				// untrusted root errors for self-signed certificates. These certificates are valid
				// for default Exchange server installations, so return true.
				return true;
			}

			_log.Warn("Cert had SSL Policy Errors, but they were not specifically identified. (Probably need to add more logic.)");
			// In all other cases, return false.
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="certificate"></param>
		/// <returns></returns>
		private static bool HasTrustedPublicKey(X509Certificate certificate)
		{
			_log.Trace("Checking cert's public key against list of trusted keys...");
			string certPk = certificate.GetPublicKeyString();

			if (!String.IsNullOrWhiteSpace(certPk))
			{
				foreach (string pk in EmailConfig.TrustedPublicKeys.Split(new[] {',', ' '}))
				{
					if (!String.IsNullOrWhiteSpace(pk) && certPk.Equals(pk))
					{
						_log.Debug(m => m("Public key match found: [{0}]", pk));
						return true;
					}
				}
			}
			else
			{
				_log.Debug("Cert's public key is an empty string.");
			}

			_log.Trace("NO public key match found.");
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static string[] ExtractAddressesFromString(string str)
		{
			return (str != null) ? str.Trim().Replace(" ", "").Split(ADDRESS_DELIMITERS) : new string[] {};	// return empty array if str is null
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="addressField"></param>
		/// <param name="addresses"></param>
		private static void AddAddresses(MailAddressCollection addressField, string[] addresses)
		{
			_log.Trace(m => m("Processing {0} addresses...", addresses.Length));
			foreach (string address in addresses)
			{
				if (Utility.IsValidEmailAddress(address))
				{
					addressField.Add(address);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="addresses"></param>
		private static void AddAddresses(ref string msg, string[] addresses)
		{
			_log.Trace(m => m("Processing {0} addresses...", addresses.Length));
			foreach (string address in addresses)
			{
				if (Utility.IsValidEmailAddress(address))
				{
					msg = String.Concat(msg, "Sent to: ", address, "\n");
				}
			}
		}
	}
}
