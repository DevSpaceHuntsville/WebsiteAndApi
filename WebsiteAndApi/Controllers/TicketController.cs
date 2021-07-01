using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using DevSpace.Common;
using Newtonsoft.Json;

namespace DevSpace.Api.Controllers {
	public class JsonStudentCodeBinder : IModelBinder {
		public bool BindModel( HttpActionContext actionContext, ModelBindingContext bindingContext ) {
			HttpContent content = actionContext.Request.Content;
			string json = content.ReadAsStringAsync().Result;
			IStudentCode obj = JsonConvert.DeserializeObject<StudentCode>( json );
			bindingContext.Model = obj;
			return true;
		}
	}

	public class TicketController : ApiController {
		private static readonly HttpClient EventBriteApi;

		private static readonly string EventBriteOrgId; 
		private static readonly string EventBriteApiKey;
		private static readonly string EventBriteEventId;
		private static readonly string EventBriteTicketId; 
		static TicketController() {
#if DEBUG
			EventBriteOrgId = Environment.GetEnvironmentVariable( "EVENTBRITEORGID", EnvironmentVariableTarget.Machine );
			EventBriteApiKey = Environment.GetEnvironmentVariable( "EVENTBRITEAPIKEY", EnvironmentVariableTarget.Machine );
			EventBriteEventId = Environment.GetEnvironmentVariable( "EVENTBRITEEVENTID", EnvironmentVariableTarget.Machine );
			EventBriteTicketId = Environment.GetEnvironmentVariable( "EVENTBRITETICKETID", EnvironmentVariableTarget.Machine );
#else
			EventBriteOrgId = ConfigurationManager.AppSettings["EventBriteOrgId"];
			EventBriteApiKey = ConfigurationManager.AppSettings["EventBriteApiKey"];
			EventBriteEventId = ConfigurationManager.AppSettings["EventBriteEventId"];
			EventBriteTicketId = ConfigurationManager.AppSettings["EventBriteTicketId"];
#endif

			EventBriteApi = new HttpClient();
			EventBriteApi.BaseAddress = new Uri( $"https://www.eventbriteapi.com/v3/organizations/{EventBriteOrgId}/discounts/" );

			EventBriteApi.DefaultRequestHeaders.Accept.Clear();
			EventBriteApi.DefaultRequestHeaders.Accept.Add(
				new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue( "application/json" )
			);

			EventBriteApi.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue(
					"Bearer",
					EventBriteApiKey
				);
		}

		private IDataStore<IStudentCode> _DataStore;
		public TicketController( IDataStore<IStudentCode> DataStore ) {
			this._DataStore = DataStore;
		}

		private class MutableStudentCode : IStudentCode {
			public string Code { get; set; }
			public string Email { get; set; }
			public int Id { get; set; }
		};

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Post( [ModelBinder(typeof(JsonStudentCodeBinder))]IStudentCode value ) {
			MutableStudentCode NewStudentCode = new MutableStudentCode {
				Email = value.Email
			};

			if( null == NewStudentCode ) return new HttpResponseMessage( HttpStatusCode.BadRequest );

			// Check for .edu email
			if( string.IsNullOrWhiteSpace( NewStudentCode.Email ) )
				return new HttpResponseMessage( HttpStatusCode.BadRequest );

			if( !NewStudentCode.Email.Trim().EndsWith( ".edu", StringComparison.InvariantCultureIgnoreCase ) )
				return new HttpResponseMessage( HttpStatusCode.BadRequest );

			// Check DataStore for existing code
			IList<IStudentCode> ExistingCodes = await _DataStore.Get( "Email", NewStudentCode.Email );

			//	If exists, resent existing code
			if( ExistingCodes.Count > 0 ) {
				SendEmail( ExistingCodes[0] );
				return new HttpResponseMessage( HttpStatusCode.NoContent );
			}

			// Generate Code
			NewStudentCode.Code = BitConverter.ToString( Guid.NewGuid().ToByteArray() ).Replace( "-", "" ).Substring( 0, 16 );
			while( 1 == ( await _DataStore.Get( "Code", NewStudentCode.Code ) ).Count )
				NewStudentCode.Code = BitConverter.ToString( Guid.NewGuid().ToByteArray() ).Replace( "-", "" ).Substring( 0, 16 );

			Newtonsoft.Json.Linq.JObject input = new Newtonsoft.Json.Linq.JObject {
				["discount"] = new Newtonsoft.Json.Linq.JObject {
						["type"] = "access",
						["code"] = NewStudentCode.Code,
						["event_id"] = EventBriteEventId,
						["ticket_class_ids"] = new Newtonsoft.Json.Linq.JArray( new[] { EventBriteTicketId } ),
						["quantity_available"] = 1
					}
			};

			try {
				HttpResponseMessage EventBriteResponse = await EventBriteApi.PostAsync(
					string.Empty,
					new StringContent(
						input.ToString(),
						Encoding.UTF8,
						"application/json"
					)
				);
				EventBriteResponse.Dispose();
			} catch( WebException ) {
				return new HttpResponseMessage( HttpStatusCode.BadGateway );
			} catch( Exception ) {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			// Store Code in DataStore
			NewStudentCode.Id = ( await _DataStore.Add( NewStudentCode ) ).Id;

			// Email Code
			SendEmail( NewStudentCode );

			return new HttpResponseMessage( HttpStatusCode.Created );
		}

		private void SendEmail( IStudentCode studentCode ) {
			Email Mail = new Email( studentCode.Email );

			Mail.Subject = "Student Ticket Code";
			Mail.Body = string.Format(
@"This email is a response to a request for a student discount code. We have validated your email address and are pleased to offer you this code.

This code is tied to your email address is a valid for one use. If you misplace this email, you may supply your email to the DevSpace website on the tickets page to receive another copy of this email. A new code will not be generated.

You may go directly to out ticketing page using the link below.
 
https://devspace.eventbrite.com?discount={0}

If you wish, you may also go directly to EventBrite, find our event, and manually enter the code:

{0}

We thank you for your interest in the DevSpace Technical Conference and look forward to seeing you there.", studentCode.Code );

			Mail.Send();
		}
	}
}
