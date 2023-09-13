using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using DevSpace.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrypt;

namespace DevSpace.Api.Controllers {
	public class JsonUserBinder : IModelBinder {
		public bool BindModel( HttpActionContext actionContext, ModelBindingContext bindingContext ) {
			HttpContent content = actionContext.Request.Content;
			string json = content.ReadAsStringAsync().Result;
			IUser obj = JsonConvert.DeserializeObject<User>( json );

			// The PasswordHash isn't in the DataContract
			JObject raw = JsonConvert.DeserializeObject<JObject>( json );
			obj = obj.UpdatePasswordHash( raw["PasswordHash"]?.ToString() );

			bindingContext.Model = obj;
			return true;
		}
	}

	[AllowAnonymous]
	public class UserController : ApiController {
		private IDataStore<IUser> _DataStore;
		public UserController( IDataStore<IUser> DataStore ) {
			this._DataStore = DataStore;
		}

		private async Task<string> CreateJsonUser( IUser User, IList<ISession> SessionList ) {
			JObject UserData = new JObject();

			UserData["Id"] = User.Id;
			UserData["DisplayName"] = User.DisplayName;
			UserData["Bio"] = User.Bio;
			UserData["Twitter"] = User.Twitter;
			UserData["Website"] = User.Website;
			UserData["Blog"] = User.Blog;
			UserData["ProfilePicture"] = User.ProfilePicture;

			JArray Sessions = new JArray();
			foreach( ISession Session in SessionList.Where( ses => ses.UserIds.Contains( User.Id ) ) ) {
				JObject jses = new JObject();
				jses["Id"] = Session.Id;
				jses["Title"] = Session.Title;
				Sessions.Add( jses );
			}
			UserData["Sessions"] = Sessions;

			return UserData.ToString( Formatting.None );
		}

		private async Task<string> CreateJsonUserArray( IList<IUser> Users ) {
			Database.SessionDataStore SessionsDS = new Database.SessionDataStore();
			// HACK : forcing to current sessions
			IList<ISession> SessionList = ( await SessionsDS.GetAll() ).Where( ses => ses.EventId == 2023 ).Where( ses => ses.Accepted ?? false ).ToList();

			JArray JsonArray = new JArray();
			foreach( IUser User in Users ) {
				JsonArray.Add( JObject.Parse( await CreateJsonUser( User, SessionList ) ) );
			}
			return JsonArray.ToString( Formatting.None );
		}

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Get() {
			try {
				IList<IUser> Users = ( await _DataStore.GetAll() ).Where( u => u.Permissions == 1 ).ToList();

				HttpResponseMessage Response = new HttpResponseMessage( HttpStatusCode.OK );
				Response.Content = new StringContent( await CreateJsonUserArray( Users.OrderBy( ses => ses.DisplayName ).ToList() ) );
				return Response;
			} catch {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Get( int Id ) {
			if( Thread.CurrentPrincipal.Identity.IsAuthenticated ) {
				IUser ExistingUser = await _DataStore.Get( Id );

				if( null == ExistingUser )
					return new HttpResponseMessage( HttpStatusCode.NotFound );

				if( Thread.CurrentPrincipal?.Identity?.Name.Equals( ExistingUser.EmailAddress, System.StringComparison.InvariantCultureIgnoreCase ) ?? false ) {
					HttpResponseMessage Response = new HttpResponseMessage( HttpStatusCode.OK );
					Response.Content = new StringContent( await Task.Factory.StartNew( () => JsonConvert.SerializeObject( ExistingUser, Formatting.None ) ) );
					return Response;
				} else {
					return new HttpResponseMessage( HttpStatusCode.Unauthorized );
				}
			} else {
				try {
					Database.SessionDataStore SessionsDS = new Database.SessionDataStore();
					IList<ISession> SessionList = ( await SessionsDS.GetAll() ).Where( ses => ses.UserIds.Contains( Id ) ).Where( ses => ses.Accepted ?? false ).Where( ses => ses.EventId == 2023 ).ToList();

					HttpResponseMessage Response = new HttpResponseMessage( HttpStatusCode.OK );
					Response.Content = new StringContent( await CreateJsonUser( await _DataStore.Get( Id ), SessionList ) );
					return Response;
				} catch {
					return new HttpResponseMessage( HttpStatusCode.InternalServerError );
				}
			}
		}

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Post( [ModelBinder( typeof( JsonUserBinder ) )]IUser NewUser ) {
			if( null == NewUser )
				return new HttpResponseMessage( HttpStatusCode.BadRequest );

			ScryptEncoder Hasher = new ScryptEncoder();
			IUser ExistingRecord = await _DataStore.Get( NewUser.Id );

			// Register new profile
			if( null == ExistingRecord ) {
				await _DataStore.Add( NewUser.UpdatePasswordHash( Hasher.Encode( string.Format( "{0}:{1}", NewUser.EmailAddress, NewUser.PasswordHash ) ) ) );

				Email NewMail = new Email( NewUser.EmailAddress, NewUser.DisplayName );
				NewMail.Subject = "New Account Created";
				NewMail.Body =
@"This email is to confirm the creation of your account on the DevSpace website.

If you did not create this account, please contact info@devspaceconf.com.";
				NewMail.Send();

				return new HttpResponseMessage( HttpStatusCode.Created );
			} else {
				// BUG: The permissions, which is currently being used as a hack to speed up database queries
				// was being reset.  The reclains the existing value.
				NewUser = NewUser.UpdatePermissions( ExistingRecord.Permissions );
			}

			// You can only post a new profile if you're not authenticated
			if( !Thread.CurrentPrincipal?.Identity?.IsAuthenticated ?? true ) {
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}

			// You can only update yourself
			if( !( Thread.CurrentPrincipal.Identity as DevSpaceIdentity ).Identity.Id.Equals( NewUser.Id ) ) {
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}

			if( !string.IsNullOrWhiteSpace( NewUser.PasswordHash ) ) {
				NewUser = NewUser.UpdatePasswordHash( Hasher.Encode( string.Format( "{0}:{1}", NewUser.EmailAddress, NewUser.PasswordHash ) ) );
			}

			await _DataStore.Update( NewUser );

			Email UpdateMail = new Email( NewUser.EmailAddress, NewUser.DisplayName );
			UpdateMail.Subject = "Account Updated";
			UpdateMail.Body =
@"This email is to confirm your account on the DevSpace website was updated.

If you did not update this account, please contact info@devspaceconf.com.";
			UpdateMail.Send();

			return new HttpResponseMessage( HttpStatusCode.OK );
		}
	}
}
