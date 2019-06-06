using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using DevSpace.Api.Controllers;
using DevSpace.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevSpace.WebsiteAndApi.Test {
	[TestClass]
	public class UserControllerTests {
		internal class TestUser : IUser {
			public string Bio { get; set; }

			public string DisplayName { get; set; }

			public string EmailAddress { get; set; }

			public int Id { get; set; }

			public string PasswordHash { get; set; }

			public byte Permissions { get; set; }

			public DateTime SessionExpires { get; set; }

			public Guid SessionToken { get; set; }

			public string Twitter { get; set; }

			public string Website { get; set; }

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
				this.PasswordHash = newPasswordHash;
				return this;
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
			public readonly Dictionary<int, IUser> data = new Dictionary<int, IUser>();

			public Task<IUser> Add( IUser ItemToAdd ) {
				TestUser blah = ItemToAdd as TestUser;
				blah.Id = data.Count + 1;
				data.Add( blah.Id, blah );
				return Task.FromResult( blah as IUser );
			}

			public Task<bool> Delete( int Id ) {
				throw new NotImplementedException();
			}

			public Task<IUser> Get( int Id ) {
				throw new NotImplementedException();
			}

			public Task<IList<IUser>> Get( string Field, object Value ) {
				return Task.FromResult( data.Values.ToList() as IList<IUser> );
			}

			public Task<IList<IUser>> GetAll() {
				throw new NotImplementedException();
			}

			public Task<IUser> Update( IUser ItemToUpdate ) {
				data[ItemToUpdate.Id] = ItemToUpdate;
				return Task.FromResult( ItemToUpdate );
			}
		}

		[TestMethod]
		public void Post_Null() {
			UserController objectUnderTest = new UserController( new TestDataStore() );
			HttpResponseMessage actual = objectUnderTest.Post( null ).Result;
			Assert.AreEqual( HttpStatusCode.BadRequest, actual.StatusCode );
		}

		[TestMethod]
		public void Post_New() {
			TestUser testUser = new TestUser {
				EmailAddress = "email@test.com",
				DisplayName = "Test Display Name",
				PasswordHash = "TestPassword"
			};

			TestDataStore testDataStore = new TestDataStore();
			UserController objectUnderTest = new UserController( testDataStore );
			HttpResponseMessage actual = objectUnderTest.Post( testUser ).Result;

			Assert.AreEqual( HttpStatusCode.Created, actual.StatusCode );

			IUser savedUser = testDataStore.data.Values.FirstOrDefault();
			Assert.IsNotNull( savedUser );

			Assert.AreEqual( testUser.EmailAddress, savedUser.EmailAddress );
			Assert.AreEqual( testUser.DisplayName, savedUser.DisplayName );
			Assert.AreNotEqual( "TestPassword", savedUser.PasswordHash );
			Assert.AreNotEqual( 0, savedUser.Id );
		}

		[TestMethod]
		public void Post_Update() {
			try {
				TestUser testUser = new TestUser {
					Id = 1,
					EmailAddress = "email@test.com",
					DisplayName = "Test Display Name",
					PasswordHash = "TestPassword"
				};

				TestDataStore testDataStore = new TestDataStore();
				testDataStore.data.Add( 1, testUser );

				Thread.CurrentPrincipal = new GenericPrincipal( new GenericIdentity( "email@test.com" ), null );

				UserController objectUnderTest = new UserController( testDataStore );
				HttpResponseMessage actual = objectUnderTest.Post( testUser ).Result;

				Assert.AreEqual( HttpStatusCode.OK, actual.StatusCode );

				IUser savedUser = testDataStore.data.Values.FirstOrDefault();
				Assert.IsNotNull( savedUser );

				Assert.AreEqual( testUser.EmailAddress, savedUser.EmailAddress );
				Assert.AreEqual( testUser.DisplayName, savedUser.DisplayName );
				Assert.AreNotEqual( "TestPassword", savedUser.PasswordHash );
				Assert.AreNotEqual( 0, savedUser.Id );
			} finally {
				Thread.CurrentPrincipal = null;
			}
		}

		[TestMethod]
		public void Post_Update_NotLoggedIn() {
			TestUser testUser = new TestUser {
				Id = 1,
				EmailAddress = "email@test.com",
				DisplayName = "Test Display Name",
				PasswordHash = "TestPassword"
			};

			TestDataStore testDataStore = new TestDataStore();
			testDataStore.data.Add( 1, testUser );

			UserController objectUnderTest = new UserController( testDataStore );
			HttpResponseMessage actual = objectUnderTest.Post( testUser ).Result;

			Assert.AreEqual( HttpStatusCode.Unauthorized, actual.StatusCode );
		}

		[TestMethod]
		public void Post_Update_WrongEmail() {
			try {
				TestUser testUser = new TestUser {
					Id = 1,
					EmailAddress = "email@test.com",
					DisplayName = "Test Display Name",
					PasswordHash = "TestPassword"
				};

				TestDataStore testDataStore = new TestDataStore();
				testDataStore.data.Add( 1, testUser );

				Thread.CurrentPrincipal = new GenericPrincipal( new GenericIdentity( "wrongemail@test.com" ), null );

				UserController objectUnderTest = new UserController( testDataStore );
				HttpResponseMessage actual = objectUnderTest.Post( testUser ).Result;

				Assert.AreEqual( HttpStatusCode.Unauthorized, actual.StatusCode );
			} finally {
				Thread.CurrentPrincipal = null;
			}
		}
	}
}
