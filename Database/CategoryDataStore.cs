using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevSpace.Common;

namespace DevSpace.Database {
	public class CategoryDataStore : IDataStore<ITag> {
		public async Task<ITag> Add( ITag ItemToAdd ) {
			throw new NotImplementedException();
		}

		public Task<bool> Delete( int Id ) {
			throw new NotImplementedException();
		}

		public async Task<ITag> Get( int Id ) {
			try {
				using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
					connection.Open();

					using( SqlCommand command = new SqlCommand( "SELECT * FROM Categories WHERE Id = @Id", connection ) ) {
						command.Parameters.Add( "Id", SqlDbType.Int ).Value = Id;
						using( SqlDataReader dataReader = await command.ExecuteReaderAsync().ConfigureAwait( false ) ) {
							if( await dataReader.ReadAsync() ) {
								return new Models.TagModel( dataReader );
							}
						}
					}
				}
			} catch( Exception ex ) {
				return null;
			}
			return null;
		}

		public async Task<IList<ITag>> Get( string Field, object Value ) {
			throw new NotImplementedException();
		}

		public async Task<IList<ITag>> GetAll() {
			List<ITag> returnList = new List<ITag>();

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "SELECT * FROM Categories", connection ) ) {
					using( SqlDataReader dataReader = await command.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							returnList.Add( new Models.TagModel( dataReader ) );
						}
					}
				}
			}

			return returnList;
		}

		public async Task<ITag> Update( ITag ItemToUpdate ) {
			throw new NotImplementedException();
		}
	}
}
