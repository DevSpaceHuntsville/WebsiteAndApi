using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevSpace.Common;

namespace DevSpace.Database {
	public class ContentDataStore : IDataStore<IArticle> {
		public async Task<IArticle> Get( int Id ) {
			try {
				using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
					connection.Open();

					using( SqlCommand command = new SqlCommand( "SELECT * FROM Content WHERE Id = @Id", connection ) ) {
						command.Parameters.Add( "Id", SqlDbType.Int ).Value = Id;
						using( SqlDataReader dataReader = await command.ExecuteReaderAsync().ConfigureAwait( false ) ) {
							if( await dataReader.ReadAsync() ) {
								return new Models.ArticleModel( dataReader );
							}
						}
					}
				}
			} catch( Exception ex ) {
				return null;
			}
			return null;
		}
		
		public async Task<IList<IArticle>> GetAll() {
			List<IArticle> returnList = new List<IArticle>();

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "SELECT * FROM Content", connection ) ) {
					using( SqlDataReader dataReader = await command.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							returnList.Add( new Models.ArticleModel( dataReader ) );
						}
					}
				}
			}

			return returnList;
		}

		public async Task<IArticle> Add( IArticle ItemToAdd ) {
			if( string.IsNullOrWhiteSpace( ItemToAdd.Title ) ) return null;
			if( string.IsNullOrWhiteSpace( ItemToAdd.Body ) ) return null;

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "INSERT Content ( Title, Body, PublishDate, ExpireDate ) OUTPUT INSERTED.Id VALUES ( @Title, @Body, @PublishDate, @ExpireDate );", connection ) ) {
					command.Parameters.Add( "Title", SqlDbType.NVarChar, 60 ).Value = ItemToAdd.Title;
					command.Parameters.Add( "Body", SqlDbType.NVarChar, -1 ).Value = ItemToAdd.Body;
					command.Parameters.Add( "PublishDate", SqlDbType.SmallDateTime ).Value = ItemToAdd.PublishDate;
					command.Parameters.Add( "ExpireDate", SqlDbType.SmallDateTime ).Value = ItemToAdd.ExpireDate;
					return ItemToAdd.UpdateId( Convert.ToInt32( await command.ExecuteScalarAsync() ) );
				}
			}
		}

		public async Task<IArticle> Update( IArticle ItemToUpdate ) {
			if( string.IsNullOrWhiteSpace( ItemToUpdate.Title ) ) return null;
			if( string.IsNullOrWhiteSpace( ItemToUpdate.Body ) ) return null;

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "UPDATE Content SET Title = @Title, Body = @Body, PublishDate = @PublishDate, ExpireDate = @ExpireDate WHERE Id = @Id;", connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = ItemToUpdate.Id;
					command.Parameters.Add( "Title", SqlDbType.NVarChar, 60 ).Value = ItemToUpdate.Title;
					command.Parameters.Add( "Body", SqlDbType.NVarChar, -1 ).Value = ItemToUpdate.Body;
					command.Parameters.Add( "PublishDate", SqlDbType.SmallDateTime ).Value = ItemToUpdate.PublishDate;
					command.Parameters.Add( "ExpireDate", SqlDbType.SmallDateTime ).Value = ItemToUpdate.ExpireDate;
					return 1 == await command.ExecuteNonQueryAsync()
						? ItemToUpdate
						: null;
				}
			}
		}

		public async Task<bool> Delete( int Id ) {
			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "DELETE Content WHERE Id = @Id;", connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = Id;
					return 0 < await command.ExecuteNonQueryAsync();
				}
			}
		}

		public async Task<IList<IArticle>> Get( string Field, object Value ) {
			await Task.Delay( 1 );
			throw new NotImplementedException();
		}
	}
}
