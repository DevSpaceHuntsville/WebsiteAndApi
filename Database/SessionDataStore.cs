using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevSpace.Common;

namespace DevSpace.Database {
	public class SessionDataStore : IDataStore<ISession> {
		public async Task<ISession> Add( ISession ItemToAdd ) {
			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				// HACK: Currentlyhas hardcoded EventId
				ISession addedSession = null;
				using( SqlCommand sessionCommand = new SqlCommand( "INSERT Sessions ( UserId, Title, Abstract, Notes, SessionLength, EventId ) VALUES ( @UserId, @Title, @Abstract, @Notes, @SessionLength, 2019 ); SELECT SCOPE_IDENTITY();", connection ) ) {
					sessionCommand.Parameters.Add( "UserId", SqlDbType.Int ).Value = ItemToAdd.UserId;
					sessionCommand.Parameters.Add( "Title", SqlDbType.VarChar ).Value = ItemToAdd.Title;
					sessionCommand.Parameters.Add( "Abstract", SqlDbType.VarChar ).Value = ItemToAdd.Abstract;
					sessionCommand.Parameters.Add( "SessionLength", SqlDbType.Int ).Value = ItemToAdd.SessionLength;

					if( string.IsNullOrWhiteSpace( ItemToAdd.Notes ) )
						sessionCommand.Parameters.Add( "Notes", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						sessionCommand.Parameters.Add( "Notes", SqlDbType.VarChar ).Value = ItemToAdd.Notes;

					addedSession = ItemToAdd.UpdateId( Convert.ToInt32( await sessionCommand.ExecuteScalarAsync() ) );
				}

				// TODO: Make sure there is a level tag (mostly for copy)

				if( 0 < addedSession.Tags.Count ) {
					using( SqlCommand tagCommand = new SqlCommand( "INSERT SessionTags ( SessionId, TagId ) VALUES ( @SessionId, @TagId );", connection ) ) {
						tagCommand.Parameters.Add( "SessionId", SqlDbType.Int ).Value = addedSession.Id;
						SqlParameter tagIdParameter = tagCommand.Parameters.Add( "TagId", SqlDbType.Int );
						foreach( ITag tag in ItemToAdd.Tags.Add( addedSession.Level ) ) {
							tagIdParameter.Value = tag.Id;
							await tagCommand.ExecuteNonQueryAsync();
						}
					}
				}

				return addedSession
					.UpdateEventId( 2019 );
			}
		}

		public async Task<bool> Delete( int Id ) {
			bool Deleted = false;

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand command = new SqlCommand( "DELETE Sessions WHERE Id = @Id", connection ) ) {
					command.Parameters.Add( "Id", SqlDbType.Int ).Value = Id;

					Deleted = 0 < await command.ExecuteNonQueryAsync();
				}
			}

			return Deleted;
		}

		public async Task<ISession> Get( int Id ) {
			ISession returnValue = null;

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand sessionCommand = new SqlCommand( "SELECT * FROM Sessions WHERE Id = @Id", connection ) ) {
					sessionCommand.Parameters.Add( "Id", SqlDbType.Int ).Value = Id;

					using( SqlDataReader dataReader = await sessionCommand.ExecuteReaderAsync() ) {
						if( await dataReader.ReadAsync() ) {
							returnValue = new Models.SessionModel( dataReader );
						}
					}
				}

				if( null != returnValue ) {
					using( SqlCommand tagCommand = new SqlCommand( "SELECT * FROM Tags WHERE Id IN ( SELECT TagId FROM SessionTags WHERE SessionId = @SessionId );", connection ) ) {
						tagCommand.Parameters.Add( "SessionId", SqlDbType.Int ).Value = returnValue.Id;

						using( SqlDataReader dataReader = await tagCommand.ExecuteReaderAsync() ) {
							while( await dataReader.ReadAsync() ) {
								ITag tag = new Models.TagModel( dataReader );
								if( tag.Id < 4 )
									returnValue = returnValue.UpdateLevel( tag );
								else
									returnValue = returnValue.AddTag( tag );
							}
						}
					}
				}
			}

			TimeSlotDataStore timeSlots = new TimeSlotDataStore();
			returnValue = returnValue.UpdateTimeSlot( await timeSlots.Get( ( returnValue as Models.SessionModel ).TimeSlotId ) );

			RoomDataStore rooms = new RoomDataStore();
			return returnValue.UpdateRoom( await rooms.Get( ( returnValue as Models.SessionModel ).RoomId ) );
		}

		public async Task<IList<ISession>> Get( string Field, object Value ) {
			List<ISession> returnList = new List<ISession>();
			List<ISession> sessionList = new List<ISession>();

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand sessionCommand = new SqlCommand( string.Format( "SELECT * FROM Sessions WHERE {0} = @value", Field ), connection ) ) {
					sessionCommand.Parameters.AddWithValue( "value", Value );

					using( SqlDataReader dataReader = await sessionCommand.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							sessionList.Add( new Models.SessionModel( dataReader ) );
						}
					}
				}

				List<Tuple<int, ITag>> TagData = new List<Tuple<int, ITag>>();
				using( SqlCommand tagCommand = new SqlCommand( "SELECT SessionId, TagId AS Id, Text FROM Tags INNER JOIN SessionTags ON Id = TagId ORDER BY SessionId, TagId;", connection ) ) {
					using( SqlDataReader dataReader = await tagCommand.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							TagData.Add( new Tuple<int, ITag>( dataReader.GetInt32( 0 ), new Models.TagModel( dataReader ) ) );
						}
					}
				}

				ISession sessionWithTags = null;
				foreach( ISession session in sessionList ) {
					sessionWithTags = session;

					foreach( Tuple<int, ITag> Tag in TagData.Where( data => data.Item1 == session.Id ) ) {
						if( Tag.Item2.Id < 4 )
							sessionWithTags = sessionWithTags.UpdateLevel( Tag.Item2 );
						else
							sessionWithTags = sessionWithTags.AddTag( Tag.Item2 );
					}

					returnList.Add( sessionWithTags );
				}
			}

			return returnList;
		}

		public async Task<IList<ISession>> GetAll() {
			List<ISession> returnList = new List<ISession>();
			List<Models.SessionModel> sessionList = new List<Models.SessionModel>();

			TimeSlotDataStore timeSlots = new TimeSlotDataStore();
			IList<ITimeSlot> timeSlotList = await timeSlots.GetAll();

			RoomDataStore rooms = new RoomDataStore();
			IList<IRoom> roomList = await rooms.GetAll();

			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand sessionCommand = new SqlCommand( "SELECT * FROM Sessions", connection ) ) {
					using( SqlDataReader dataReader = await sessionCommand.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							sessionList.Add( new Models.SessionModel( dataReader ) );
						}
					}
				}

				List<Tuple<int, ITag>> TagData = new List<Tuple<int, ITag>>();
				using( SqlCommand tagCommand = new SqlCommand( "SELECT SessionId, TagId AS Id, Text FROM Tags INNER JOIN SessionTags ON Id = TagId ORDER BY SessionId, TagId;", connection ) ) {
					using( SqlDataReader dataReader = await tagCommand.ExecuteReaderAsync() ) {
						while( await dataReader.ReadAsync() ) {
							TagData.Add( new Tuple<int, ITag>( dataReader.GetInt32( 0 ), new Models.TagModel( dataReader ) ) );
						}
					}
				}

				ISession sessionWithTags = null;
				foreach( Models.SessionModel session in sessionList ) {
					sessionWithTags = session
						.UpdateTimeSlot( timeSlotList.FirstOrDefault( ts => ts.Id == session.TimeSlotId ) )
						.UpdateRoom( roomList.FirstOrDefault( r => r.Id == session.RoomId ) );

					foreach( Tuple<int, ITag> Tag in TagData.Where( data => data.Item1 == session.Id ) ) {
						if( Tag.Item2.Id < 4 )
							sessionWithTags = sessionWithTags.UpdateLevel( Tag.Item2 );
						else
							sessionWithTags = sessionWithTags.AddTag( Tag.Item2 );
					}

					returnList.Add( sessionWithTags );
				}
			}

			return returnList;
		}

		public async Task<ISession> Update( ISession ItemToUpdate ) {
			using( SqlConnection connection = new SqlConnection( Settings.ConnectionString ) ) {
				connection.Open();

				using( SqlCommand sessionCommand = new SqlCommand( "UPDATE Sessions SET UserId = @UserId, Title = @Title, Abstract = @Abstract, Notes = @Notes, SessionLength = @SessionLength WHERE Id = @Id; DELETE FROM SessionTags WHERE SessionId = @Id;", connection ) ) {
					sessionCommand.Parameters.Add( "Id", SqlDbType.Int ).Value = ItemToUpdate.Id;
					sessionCommand.Parameters.Add( "UserId", SqlDbType.Int ).Value = ItemToUpdate.UserId;
					sessionCommand.Parameters.Add( "Title", SqlDbType.VarChar ).Value = ItemToUpdate.Title;
					sessionCommand.Parameters.Add( "Abstract", SqlDbType.VarChar ).Value = ItemToUpdate.Abstract;
					sessionCommand.Parameters.Add( "SessionLength", SqlDbType.Int ).Value = ItemToUpdate.SessionLength;

					if( string.IsNullOrWhiteSpace( ItemToUpdate.Notes ) )
						sessionCommand.Parameters.Add( "Notes", SqlDbType.VarChar ).Value = DBNull.Value;
					else
						sessionCommand.Parameters.Add( "Notes", SqlDbType.VarChar ).Value = ItemToUpdate.Notes;

					await sessionCommand.ExecuteNonQueryAsync();
				}

				if( 0 < ItemToUpdate.Tags.Count ) {
					using( SqlCommand tagCommand = new SqlCommand( "INSERT SessionTags ( SessionId, TagId ) VALUES ( @SessionId, @TagId );", connection ) ) {
						tagCommand.Parameters.Add( "SessionId", SqlDbType.Int ).Value = ItemToUpdate.Id;
						SqlParameter tagIdParameter = tagCommand.Parameters.Add( "TagId", SqlDbType.Int );
						foreach( ITag tag in ItemToUpdate.Tags.Add( ItemToUpdate.Level ) ) {
							tagIdParameter.Value = tag.Id;
							await tagCommand.ExecuteNonQueryAsync();
						}
					}
				}

				return ItemToUpdate;
			}
		}
	}
}
