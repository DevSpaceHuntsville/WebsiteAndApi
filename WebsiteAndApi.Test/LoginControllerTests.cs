using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using DevSpace.Api.Controllers;
using DevSpace.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevSpace.WebsiteAndApi.Test {
	[TestClass]
	public class LoginControllerTests {
		internal class TestUser : IUser {
			public TestUser( int id ) {
				this.Id = id;
			}

			public string Bio {
				get {
					throw new NotImplementedException();
				}
			}

			public string DisplayName {
				get {
					throw new NotImplementedException();
				}
			}

			public string EmailAddress {
				get {
					throw new NotImplementedException();
				}
			}

			public int Id { get; private set; }

			public string PasswordHash {
				get {
					throw new NotImplementedException();
				}
			}

			public byte Permissions {
				get {
					throw new NotImplementedException();
				}
			}

			public DateTime SessionExpires {
				get {
					throw new NotImplementedException();
				}
			}

			public Guid SessionToken {
				get {
					throw new NotImplementedException();
				}
			}

			public string Twitter {
				get {
					throw new NotImplementedException();
				}
			}

			public string Website {
				get {
					throw new NotImplementedException();
				}
			}

			public string Blog => throw new NotImplementedException();

			public string ProfilePicture => throw new NotImplementedException();

			public Guid? SessionizeId => throw new NotImplementedException();

			public IUser UpdateBio( string newBio ) {
				throw new NotImplementedException();
			}

			public IUser UpdateBlog( string newBlog ) => throw new NotImplementedException();

			public IUser UpdateDisplayName( string newDisplayName ) {
				throw new NotImplementedException();
			}

			public IUser UpdateEmailAddress( string newEmailAddress ) {
				throw new NotImplementedException();
			}

			public IUser UpdateId( int newId ) {
				throw new NotImplementedException();
			}

			public IUser UpdatePasswordHash( string newPasswordHash ) {
				throw new NotImplementedException();
			}

			public IUser UpdatePermissions( byte newPermissions ) {
				throw new NotImplementedException();
			}

			public IUser UpdateProfilePicture( string newProfilePicture ) => throw new NotImplementedException();

			public IUser UpdateSessionExpires( DateTime newSessionExpires ) {
				throw new NotImplementedException();
			}

			public IUser UpdateSessionizeId( Guid? newSessionizeId ) => throw new NotImplementedException();

			public IUser UpdateSessionToken( Guid newSessionToken ) {
				throw new NotImplementedException();
			}

			public IUser UpdateTwitter( string newTwitter ) {
				throw new NotImplementedException();
			}

			public IUser UpdateWebsite( string newWebsite ) {
				throw new NotImplementedException();
			}
		}

		internal class TestDataStore : IDataStore<IUser> {
			public Task<IUser> Add( IUser ItemToAdd ) {
				throw new NotImplementedException();
			}

			public Task<bool> Delete( int Id ) {
				throw new NotImplementedException();
			}

			public Task<IUser> Get( int Id ) {
				throw new NotImplementedException();
			}

			public Task<IList<IUser>> Get( string Field, object Value ) {
				IList<IUser> returnValue = new List<IUser>();
				switch( Value.ToString() ) {
					case "test@email.com":
						returnValue.Add( new TestUser( 1 ) );
						break;
				}
				return Task.FromResult( returnValue );
			}

			public Task<IList<IUser>> GetAll() {
				throw new NotImplementedException();
			}

			public Task<IUser> Update( IUser ItemToUpdate ) {
				throw new NotImplementedException();
			}
		}

		[TestMethod]
		public void Get_NoContext() {
			LoginController objectUnderTest = new LoginController( new TestDataStore() );
			Assert.AreEqual( -1, objectUnderTest.Get().Result );
		}

		[TestMethod]
		public void Get_NotFound() {
			try {
				GenericIdentity Identity = new GenericIdentity( "badtest@email.com" );
				Thread.CurrentPrincipal = new GenericPrincipal( Identity, null );
				LoginController objectUnderTest = new LoginController( new TestDataStore() );
				Assert.AreEqual( -1, objectUnderTest.Get().Result );
			} finally {
				Thread.CurrentPrincipal = null;
			}
		}

		[TestMethod]
		public void Get() {
			try {
				GenericIdentity Identity = new GenericIdentity( "test@email.com" );
				Thread.CurrentPrincipal = new GenericPrincipal( Identity, null );
				LoginController objectUnderTest = new LoginController( new TestDataStore() );
				Assert.AreEqual( 1, objectUnderTest.Get().Result );
			} finally {
				Thread.CurrentPrincipal = null;
			}
		}
	}
}
