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
	public class SponsorController : ApiController {
		private IDataStore<ISponsor> _DataStore;
		public SponsorController( IDataStore<ISponsor> DataStore ) {
			this._DataStore = DataStore;
		}

		public JObject CreateSponsorJson( ISponsor Sponsor ) {
			JObject value = new JObject();
			value["DisplayName"] = Sponsor.DisplayName;
			value["LogoLarge"] = Sponsor.LogoLarge;
			value["LogoSmall"] = Sponsor.LogoSmall;
			value["Website"] = Sponsor.Website;
			return value;
		}

		public JObject CreateSponsorLevelJson( ISponsorLevel SponsorLevel, IEnumerable<ISponsor> Sponsors ) {
			JObject value = new JObject();
			value["DisplayName"] = SponsorLevel.DisplayName;
			value["DisplayInSidebar"] = SponsorLevel.DisplayInSidebar;

			JArray array = new JArray();
			foreach( ISponsor Sponsor in Sponsors )
				array.Add( CreateSponsorJson( Sponsor ) );
			value["Sponsors"] = array;

			return value;

		}

		public JArray CreateReturnJson( IEnumerable<ISponsor> Sponsors ) {
			IList<ISponsorLevel> SponsorLevels = new List<ISponsorLevel>();
			foreach( ISponsor Sponsor in Sponsors ) {
				if( !SponsorLevels.Contains( Sponsor.Level ) )
					SponsorLevels.Add( Sponsor.Level );
			}

			JArray value = new JArray();
			foreach( ISponsorLevel SponsorLevel in SponsorLevels ) {
				IEnumerable<ISponsor> Subset = Sponsors.Where( spon => spon.Level == SponsorLevel );
				if( Subset.Count() > 0 ) {
					value.Add( CreateSponsorLevelJson( SponsorLevel, Subset ) );
				}
			}

			return value;
		}

		[AllowAnonymous]
		public async Task<HttpResponseMessage> Get() {
			try {
				HttpResponseMessage response = new HttpResponseMessage( HttpStatusCode.OK );
				// string val = JsonConvert.SerializeObject( ( await _DataStore.GetAll() ).OrderBy( spon => spon.Level.DisplayOrder ) );
				response.Content = new StringContent( CreateReturnJson( ( await _DataStore.GetAll() ).Where( spon => spon.EventId == 2020 ).OrderBy( spon => spon.Level.DisplayOrder ) ).ToString() );
				return response;
			} catch( NotImplementedException ) {
				return new HttpResponseMessage( HttpStatusCode.NotImplemented );
			} catch {
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}
	}
}
