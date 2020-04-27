using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace DevSpace {
	public class Email {
		private MailMessage Mail = null;
		private SmtpClient Client = null;

		public Email( string toAddress, string toDisplay = null ) {
			this.Mail = new MailMessage(
				new MailAddress(
					ConfigurationManager.AppSettings["SmtpEmailAddress"].IfNullOrWhiteSpace( "info@devspaceconf.com" ),
					ConfigurationManager.AppSettings["SmtpDisplayName"].IfNullOrWhiteSpace( "DevSpace Technical Conference" )
				),
				new MailAddress( toAddress, toDisplay )
			);

			if( string.IsNullOrWhiteSpace( ConfigurationManager.AppSettings["SmtpServer"] ) )
				return;

			this.Client = new SmtpClient( ConfigurationManager.AppSettings["SmtpServer"], Convert.ToInt32( ConfigurationManager.AppSettings["SmtpPort"] ) );
			this.Client.EnableSsl = true;
			this.Client.Credentials = new NetworkCredential( ConfigurationManager.AppSettings["SmtpEmailAddress"], ConfigurationManager.AppSettings["SmtpPassword"] );
		}

		public string Subject {
			get {
				return this.Mail.Subject;
			}

			set {
				this.Mail.Subject = value;
			}
		}

		public string Body {
			get {
				return this.Mail.Body;
			}

			set {
				this.Mail.Body = value;
			}
		}

		public bool BccInfo {
			set {
				if( value ) {
					if( !Mail.Bcc.Any() ) {
						Mail.Bcc.Add( new MailAddress( "info@devspaceconf.com", "DevSpace Information" ) );
					}
				} else {
					Mail.Bcc.Clear();
				}
			}
		}

		public void Send() {

//			Mail.Subject = "Student Ticket Code";
//			Mail.Body = string.Format(
//@"This email is a response to a request for a student discount code. We have validated your email address and are pleased to offer you this code.

//This code is tied to your email address is a valid for one use. If you misplace this email, you may supply your email to the DevSpace website on the tickets page to receive another copy of this email. A new code will not be generated.

//You may go directly to out ticketing page using the link below.
 
//https://www.eventbrite.com/e/devspace-2016-registration-24347789895?access={0}

//If you wish, you may also go directly to EventBrite, find out event, and manually enter the code:

//{0}

//We thank you for your interest in the DevSpace Technical Conference and look forward to seeing you there.", studentCode.Code );

			Client?.Send( Mail );
		}
	}
}