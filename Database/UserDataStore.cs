using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevSpace.Common;

namespace DevSpace.Database {
	public class UserDataStore : IDataStore<IUser> {
		public async Task<IUser> Add( IUser ItemToAdd ) {
			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "INSERT Users ( DisplayName, EmailAddress, Bio, Permissions, PasswordHash, Twitter, Website ) VALUES ( @DisplayName, @EmailAddress, @Bio, @Permissions, @PasswordHash, @Twitter, @Website ); SELECT SCOPE_IDENTITY();", connection ) ) {
					command.Parameters.Add( "DisplayName", SqlDbType.VarChar ).Value = ItemToAdd.DisplayName;
					command.Parameters.Add( "EmailAddress", SqlDbType.VarChar ).Value = ItemToAdd.EmailAddress;

					if( string.IsNullOrWhiteSpace( ItemToAdd.Bio ) )
						command.Parameters.Add( "Bio", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Bio", SqlDbType.VarChar ).Value = ItemToAdd.Bio;

					command.Parameters.Add( "Permissions", SqlDbType.TinyInt ).Value = ItemToAdd.Permissions;
					command.Parameters.Add( "PasswordHash", SqlDbType.VarChar ).Value = ItemToAdd.PasswordHash;

					if( string.IsNullOrWhiteSpace( ItemToAdd.Twitter ) )
						command.Parameters.Add( "Twitter", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Twitter", SqlDbType.VarChar ).Value = ItemToAdd.Twitter;

					if( string.IsNullOrWhiteSpace( ItemToAdd.Website ) )
						command.Parameters.Add( "Website", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Website", SqlDbType.VarChar ).Value = ItemToAdd.Website;

					ItemToAdd = ItemToAdd.UpdateId( Convert.ToInt32( await command.ExecuteScalarAsync() ) );
				}
			}

			return ItemToAdd;
		}

		public async Task<IUser> Get( int Id ) {
			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "SELECT * FROM Users WHERE Id = @Id", connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = Id;

					using( SqlDataReader dataReader = await command.ExecuteReaderAsync() ) {
						if( await dataReader.ReadAsync() ) {
							return new Models.UserModel( dataReader );
						}
					}
				}
			}

			return null;
		}

		public async Task<IList<IUser>> Get( string Field, object Value ) {
			List<IUser> returnList = new List<IUser>();

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( string.Format( "SELECT * FROM Users WHERE {0} = @value", Field ), connection ) ) {
					command.Parameters.AddWithValue( "value", Value );

					using( SqlDataReader dataReader = await command.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							returnList.Add( new Models.UserModel( dataReader ) );
						}
					}
				}
			}

			return returnList;
		}

		public async Task<IList<IUser>> GetAll() {
			List<IUser> returnList = new List<IUser>();

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				//using( SqlCommand command = new SqlCommand( "SELECT * FROM Users WHERE Id IN ( SELECT DISTINCT UserId FROM Sessions WHERE Accepted = 1 )", connection ) ) {
				using( SqlCommand command = new SqlCommand( "SELECT * FROM Users WHERE Permissions = 1;", connection ) ) {
					using( SqlDataReader dataReader = await command.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							returnList.Add( new Models.UserModel( dataReader ) );
						}
					}
				}
			}

			return returnList;
		}

		public async Task<IUser> Update( IUser ItemToUpdate ) {
			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				// PasswordHash should only be updated if it has a value
				StringBuilder sqlQuery = new StringBuilder( "UPDATE Users SET DisplayName = @DisplayName, EmailAddress = @EmailAddress, Bio = @Bio, Permissions = @Permissions, " );
				if( !string.IsNullOrWhiteSpace( ItemToUpdate.PasswordHash ) )
					sqlQuery.Append( "PasswordHash = @PasswordHash, " );
				sqlQuery.Append( "Twitter = @Twitter, Website = @Website, Blog = @Blog, ProfilePicture = @ProfilePicture WHERE Id = @Id" );

				using( SqlCommand command = new SqlCommand( sqlQuery.ToString(), connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = ItemToUpdate.Id;
					command.Parameters.Add( "DisplayName", SqlDbType.VarChar ).Value = ItemToUpdate.DisplayName;
					command.Parameters.Add( "EmailAddress", SqlDbType.VarChar ).Value = ItemToUpdate.EmailAddress;

					if( string.IsNullOrWhiteSpace( ItemToUpdate.Bio ) )
						command.Parameters.Add( "Bio", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Bio", SqlDbType.VarChar ).Value = ItemToUpdate.Bio;

					command.Parameters.Add( "Permissions", SqlDbType.TinyInt ).Value = ItemToUpdate.Permissions;

					// PasswordHash should only be updated if it has a value
					if( !string.IsNullOrWhiteSpace( ItemToUpdate.PasswordHash ) )
						command.Parameters.Add( "PasswordHash", SqlDbType.VarChar ).Value = ItemToUpdate.PasswordHash;

					if( string.IsNullOrWhiteSpace( ItemToUpdate.Twitter ) )
						command.Parameters.Add( "Twitter", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Twitter", SqlDbType.VarChar ).Value = ItemToUpdate.Twitter;

					if( string.IsNullOrWhiteSpace( ItemToUpdate.Website ) )
						command.Parameters.Add( "Website", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Website", SqlDbType.VarChar ).Value = ItemToUpdate.Website;

					if( string.IsNullOrWhiteSpace( ItemToUpdate.Blog ) )
						command.Parameters.Add( "Blog", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "Blog", SqlDbType.VarChar ).Value = ItemToUpdate.Blog;

					if( string.IsNullOrWhiteSpace( ItemToUpdate.ProfilePicture ) )
						command.Parameters.Add( "ProfilePicture", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						command.Parameters.Add( "ProfilePicture", SqlDbType.VarChar ).Value = ItemToUpdate.ProfilePicture;

					await command.ExecuteNonQueryAsync();
				}
			}

			return ItemToUpdate;
		}

		public async Task<IUser> CreateSession( IUser ItemToUpdate ) {
			IUser UpdatedItem = ItemToUpdate
				.UpdateSessionToken( Guid.NewGuid() )
				.UpdateSessionExpires( DateTime.UtcNow.AddMinutes( 20 ) );

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "UPDATE Users SET SessionToken = @SessionToken, SessionExpires = @SessionExpires WHERE Id = @Id", connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = UpdatedItem.Id;
					command.Parameters.Add( "SessionToken", SqlDbType.UniqueIdentifier ).Value = UpdatedItem.SessionToken;
					command.Parameters.Add( "SessionExpires", SqlDbType.DateTime ).Value = UpdatedItem.SessionExpires;

					await command.ExecuteNonQueryAsync();
				}
			}

			return UpdatedItem;
		}

		public async Task<IUser> UpdateSession( IUser ItemToUpdate ) {
			IUser UpdatedItem = ItemToUpdate
				.UpdateSessionExpires( DateTime.UtcNow.AddMinutes( 20 ) );

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "UPDATE Users SET SessionExpires = @SessionExpires WHERE Id = @Id", connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = UpdatedItem.Id;
					command.Parameters.Add( "SessionExpires", SqlDbType.DateTime ).Value = UpdatedItem.SessionExpires;

					await command.ExecuteNonQueryAsync();
				}
			}

			return UpdatedItem;
		}

		public Task<bool> Delete( int Id ) {
			throw new NotImplementedException();
		}
	}
}
