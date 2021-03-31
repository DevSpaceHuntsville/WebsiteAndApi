using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DevSpace.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevSpace.Api.Controllers {
	public class ContentController : ApiController {
		private IDataStore<IArticle> _DataStore;
		public ContentController( IDataStore<IArticle> DataStore ) {
			this._DataStore = DataStore;
		}

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Get() {
			try {
				IList<IArticle> Articles = ( await _DataStore.GetAll() )
					.Where( a => a.ExpireDate > DateTime.UtcNow )
					.Where( a => a.PublishDate < DateTime.UtcNow )
					.ToList();

				HttpResponseMessage Response = new HttpResponseMessage( HttpStatusCode.OK );
				Response.Content = new StringContent( JsonConvert.SerializeObject( Articles ) );
				return Response;
			} catch {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Get( int id ) {
			try {
				IArticle Article = await _DataStore.Get( id );

				HttpResponseMessage Response = new HttpResponseMessage( HttpStatusCode.OK );
				Response.Content = new StringContent( JsonConvert.SerializeObject( Article ) );
				return Response;
			} catch {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}

		[Authorize]
		public async Task<HttpResponseMessage> Post( [FromBody] JObject formData ) {
			if( null == formData )
				return new HttpResponseMessage( HttpStatusCode.BadRequest );

			try {
				HttpResponseMessage Response = new HttpResponseMessage( HttpStatusCode.OK );
				Response.Content =
					new StringContent(
						JsonConvert.SerializeObject(
							await _DataStore.Add(
								JsonConvert.DeserializeObject<IArticle>(
									formData.ToString()
								)
							)
						)
					);

				return Response;
			} catch {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}

		[Authorize]
		public async Task<HttpResponseMessage> Delete( int id ) {
			try {
				if( await _DataStore.Delete( id ) )
					return new HttpResponseMessage( HttpStatusCode.NoContent );
				else
					return new HttpResponseMessage( HttpStatusCode.NotFound );
			} catch {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}
	}
}
