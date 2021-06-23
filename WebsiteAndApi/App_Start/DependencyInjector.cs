using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using DevSpace.Api.Controllers;

namespace DevSpace {
	/// <summary>Allows the controllers to use the correct database.</summary>
	/// <remarks>
	///		This class glues a lot of things together.
	///		Please do not include usings for any of the external DevSpace libraries.
	/// </remarks>
	internal class DependencyInjector : IDependencyResolver {
		public DependencyInjector() {
			Common.IDatabase Database = new Database.SqlDatabase();
			Database.Initialize();
		}

		public object GetService( Type serviceType ) {
			switch( serviceType.Name ) {
				case nameof( SponsorController ):
					return new SponsorController( new Database.SponsorDataStore() );

				case nameof( TicketController ):
					return new TicketController( new Database.StudentCodeDataStore() );

				case nameof( LoginController ):
					return new LoginController( new Database.UserDataStore() );

				case nameof( UserController ):
					return new UserController( new Database.UserDataStore() );

				case nameof( TagController ):
					return new TagController( new Database.TagDataStore() );

				case nameof( LevelController ):
					return new LevelController( new Database.LevelDataStore() );

				case nameof( CategoryController ):
					return new CategoryController( new Database.CategoryDataStore() );

				case nameof( SessionController ):
					return new SessionController( new Database.SessionDataStore() );

				case nameof( ContentController ):
					return new ContentController( new Database.ContentDataStore() );
			}

			return null;
		}

		public IEnumerable<object> GetServices( Type serviceType ) {
			return new List<object>();
		}

		public IDependencyScope BeginScope() {
			return this;
		}

		public void Dispose() {
		}
	}
}