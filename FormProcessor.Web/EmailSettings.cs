using System;
using System.Configuration;
using System.Xml.Serialization;

namespace BellevueCollege.Config
{
	/// <summary>
	/// Provides programmatic access to e-mail settings from the .config file
	/// </summary>
	/// <remarks>
	/// To use this class, the app.config or Web.config file must include an EmailSettings
	/// section:
	/// <example>
	///		Add the configuration section handler:
	///		<code>
	///		&lt;configuration&gt;
	///			&lt;configSections&gt;
	///				&lt;section name="EmailSettings" type="BellevueCollege.Config.EmailSettingsConfigHandler, BellevueCollege.Common" /&gt;
	///			&lt;/configSections&gt;
	///		&lt;/configuration&gt;
	///		</code>
	///		And then the EmailSettings section, with the appropriate values:
	///		<code>
	///		&lt;EmailSettings Enabled="true"
	///									 Mode="Test"
	///									 SmtpHost="localhost"
	///									 DefaultFromAddress="noreply@domain.com"
	///									 UseHtml="true"/>
	///		</code>
	///		* See the available properties for the full list of available settings.
	/// </example>
	/// </remarks>
	[XmlType("EmailSettings")]
	public class EmailSettings
	{
		// Default values
// ReSharper disable RedundantDefaultFieldInitializer
		private bool _enabled = false;
		private EmailMode _mode = EmailMode.Normal;
		private string _smtpHost = "localhost";
		private int _smtpPort = 25;
		private bool _forceAsync = false;
		private bool _useHtml = false;
		private bool _requireSmtpAuth = false;
		private bool _requireSsl = false;
		private SmtpAuthMode _authMode = SmtpAuthMode.Context;
		private bool _ignoreInvalidSslCerts = false;
		private string _trustedPublicKeys = String.Empty;
// ReSharper restore RedundantDefaultFieldInitializer

		/// <summary>
		/// The name of the <see cref="ConfigurationSection"/> for this object
		/// </summary>
		static public string SectionName
		{
			get
			{
				Type thisType = typeof(EmailSettings);

				System.Reflection.MemberInfo info = thisType;
				object[] attributes = info.GetCustomAttributes(typeof(XmlTypeAttribute), false);

				if (attributes != null && attributes.Length > 0 && attributes[0] != null)
				{
					return (attributes[0] as XmlTypeAttribute).TypeName;
				}
				
				throw new InvalidOperationException(string.Format("Unable to find an XmlTypeAttribute for [{0}]", thisType));
			}
		}

		#region Enumerations
		/// <summary>
		/// Specifies how sending of e-mail is handled
		/// </summary>
		public enum EmailMode
		{
			///<summary>
			/// Process e-mail normally
			///</summary>
			[XmlEnum(Name = "Normal")]
			Normal,

			///<summary>
			/// Only send e-mail to specified individuals
			///</summary>
			/// <remarks>
			/// This setting is used during Development and Testing so that real users don't receive
			/// e-mail during these processes.
			/// </remarks>
			/// <seealso cref="EmailSettings.Mode"/>
			/// <seealso cref="EmailSettings.AdditionalToAddresses"/>
			[XmlEnum(Name = "Test")]
			Test
		}

		/// <summary>
		/// Specifies the authentication mode to use
		/// </summary>
		/// <seealso cref="EmailSettings.RequireSmtpAuth"/>
		/// <seealso cref="EmailSettings.AuthMode"/>
		public enum SmtpAuthMode
		{
			/// <summary>
			/// Use the credentials for the currently executing security context
			/// </summary>
			/// <remarks>
			/// Typically, this is the service account under which the application is running
			/// </remarks>
			[XmlEnum(Name = "Context")]
			Context,

			/// <summary>
			/// Use the credentials for the user currently logged into the application
			/// </summary>
			[XmlEnum(Name = "WebUser")]
			WebUser,

			[XmlEnum(Name = "Manual")]
			Manual
		}

		#endregion

		#region Properties (.config attributes)
		/// <summary>
		/// Enables/disables sending of e-mail
		/// </summary>
		/// <remarks>
		/// Default value: <i>false</i>
		/// </remarks>
		[XmlAttribute(AttributeName = "Enabled", DataType = "boolean")]
		public bool Enabled
		{
			get {return _enabled;}
			set {_enabled = value;}
		}

		/// <summary>
		/// Specifies how to manage the sending of e-mail
		/// </summary>
		/// <remarks>
		/// Default value: <see cref="EmailMode.Normal"/>
		/// </remarks>
		/// <seealso cref="EmailMode"/>
		[XmlAttribute("Mode")]
		public EmailMode Mode
		{
			get {return _mode;}
			set {_mode = value;}
		}

		/// <summary>
		/// The server to use when sending e-mail
		/// </summary>
		/// <remarks>
		/// Default value: <i>localhost</i>
		/// </remarks>
		/// <seealso cref="SmtpPort"/>
		[XmlAttribute("SmtpHost")]
		public string SmtpHost
		{
			get {return _smtpHost;}
			set {_smtpHost = value;}
		}

		/// <summary>
		/// Enforces asynchronous sending of all e-mail
		/// </summary>
		/// <remarks>
		/// Default value: <i>false</i>
		/// </remarks>
		[XmlAttribute(AttributeName = "ForceAsync", DataType = "boolean")]
		public bool ForceAsync
		{
			get {return _forceAsync;}
			set {_forceAsync = value;}
		}

		/// <summary>
		/// The <b>From</b> address to use if one is not explicitly specified
		/// </summary>
		[XmlAttribute("DefaultFromAddress")]
		public string DefaultFromAddress {get;set;}

		/// <summary>
		/// The e-mail <b>Subject</b> to use if one is not explicitly specified
		/// </summary>
		[XmlAttribute("DefaultSubject")]
		public string DefaultSubject { get;set;}

		/// <summary>
		/// Send e-mail to these addresses in addition to the addresses specified
		/// </summary>
		/// <remarks>
		/// When in <see cref="EmailMode.Test"/> <see cref="Mode"/>, e-mail will <i><b>only</b></i> be sent to these addresses.
		/// </remarks>
		/// <seealso cref="Mode"/>
		/// <seealso cref="EmailMode"/>
		[XmlAttribute("AdditionalToAddresses")]
		public string AdditionalToAddresses {get;set;}

		/// <summary>
		/// Allow HTML formatting in the body of the e-mail
		/// </summary>
		/// <remarks>
		/// Default value: <i>false</i>
		/// </remarks>
		[XmlAttribute(AttributeName = "UseHtml", DataType = "boolean")]
		public bool UseHtml
		{
			get {return _useHtml;}
			set {_useHtml = value;}
		}

		/// <summary>
		/// The domain to use for staff e-mail addresses
		/// </summary>
		/// <remarks>
		/// This string should include the @ symbol, and will be appended to the user's login username
		/// to generate their e-mail address.
		/// </remarks>
		[XmlAttribute("StaffDomain")]
		public string StaffDomain{get;set;}

		/// <summary>
		/// Specify whether the SMTP server requires login credentials
		/// </summary>
		/// <remarks>
		/// Default value: <i>false</i>
		/// </remarks>
		/// <seealso cref="AuthMode"/>
		[XmlAttribute(AttributeName = "RequireSmtpAuth", DataType = "boolean")]
		public bool RequireSmtpAuth
		{
			get {return _requireSmtpAuth;}
			set {_requireSmtpAuth = value;}
		}

		/// <summary>
		/// Specifies which credentials to use when sending e-mail
		/// </summary>
		/// <remarks>
		/// Default value: <see cref="SmtpAuthMode.Context"/>
		/// </remarks>
		/// <seealso cref="RequireSmtpAuth"/>
		/// <seealso cref="SmtpAuthMode"/>
		[XmlAttribute("AuthMode")]
		public SmtpAuthMode AuthMode
		{
			get {return _authMode;}
			set {_authMode = value;}
		}

		[XmlAttribute("Username")]
		public string Username{get;set;}

		[XmlAttribute("Password")]
		public string Password{get;set;}

		[XmlAttribute("LoginDomain")]
		public string LoginDomain{get;set;}

		/// <summary>
		/// Connect to the SMTP server over SSL
		/// </summary>
		/// <remarks>
		/// Default value: <i>false</i>
		/// </remarks>
		[XmlAttribute(AttributeName = "RequireSSL", DataType = "boolean")]
		public bool RequireSSL
		{
			get {return _requireSsl;}
			set {_requireSsl = value;}
		}

		/// <summary>
		/// The port to connect to on the <see cref="SmtpHost"/>
		/// </summary>
		/// <remarks>
		/// Default value: <i>25</i>
		/// </remarks>
		/// <seealso cref="SmtpHost"/>
		[XmlAttribute(AttributeName = "SmtpPort", DataType = "int")]
		public int SmtpPort
		{
			get {return _smtpPort;}
			set {_smtpPort = value;}
		}

		/// <summary>
		/// Ignore SSL certificat validation errors
		/// </summary>
		/// <remarks>
		/// <b>WARNING:</b> This can present a significant security hole. Setting this
		/// property to <i>true</i> will cause the code to ignore <b>ALL</b> SSL certificate
		/// warnings - including potentially malicious and/or spoofed certificates.
		/// </remarks>
		[XmlAttribute(AttributeName = "IgnoreInvalidSslCerts", DataType = "boolean")]
		public bool IgnoreInvalidSslCerts
		{
			get {return _ignoreInvalidSslCerts;}
			set {_ignoreInvalidSslCerts = value;}
		}

		/// <summary>
		/// [Not yet implemented]
		/// </summary>
		/// <remarks>
		/// Default value: <see cref="String.Empty"/>
		/// </remarks>
		[XmlAttribute(AttributeName = "TrustedPublicKeys")]
		public string TrustedPublicKeys
		{
			get {return _trustedPublicKeys;}
			set {_trustedPublicKeys = value;}
		}
		#endregion
	}
}
