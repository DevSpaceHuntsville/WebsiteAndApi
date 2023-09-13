using System;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Reflection;
using DevSpace.Common;

namespace DevSpace.Database.Models {
	public class SessionModel : ISession {
		private SessionModel() {
			Tags = ImmutableList<ITag>.Empty;
			UserIds = ImmutableList<int>.Empty;
		}

		internal SessionModel( SqlDataReader dataReader ) : this() {
			for( int lcv = 0; lcv < dataReader.FieldCount; ++lcv ) {
				PropertyInfo property = GetType().GetProperty( dataReader.GetName( lcv ), BindingFlags.Instance | BindingFlags.Public );
				if( null == property )
					continue;

				object value = dataReader.GetValue( lcv );
				if( DBNull.Value == value )
					value = null;

				property.SetValue( this, value );
			}
		}

		#region ISession
		public string Abstract { get; internal set; }
		public bool? Accepted { get; internal set; }
		public int Id { get; internal set; }
		public string Notes { get; internal set; }
		public ImmutableList<ITag> Tags { get; private set; }
		public string Title { get; internal set; }
		public ImmutableList<int> UserIds { get; private set; }
		public int SessionLength { get; internal set; }

		public int LevelId { get; internal set; }
		public ITag Level { get; internal set; }

		public int CategoryId { get; internal set; }
		public ITag Category { get; internal set; }

		public int TimeSlotId { get; internal set; }
		public ITimeSlot TimeSlot { get; internal set; }

		public int RoomId { get; internal set; }
		public IRoom Room { get; internal set; }

		public int EventId { get; internal set; }
		public int? SessionizeId { get; internal set; }

		public ISession UpdateAbstract( string value ) {
			SessionModel newSession = this.Clone();
			newSession.Abstract = value;
			return newSession;
		}

		public ISession UpdateAccepted( bool? value ) {
			SessionModel newSession = this.Clone();
			newSession.Accepted = value;
			return newSession;
		}

		public ISession UpdateId( int value ) {
			SessionModel newSession = this.Clone();
			newSession.Id = value;
			return newSession;
		}

		public ISession UpdateNotes( string value ) {
			SessionModel newSession = this.Clone();
			newSession.Notes = value;
			return newSession;
		}

		public ISession UpdateTitle( string value ) {
			SessionModel newSession = this.Clone();
			newSession.Title = value;
			return newSession;
		}

		public ISession UpdateSessionLength( int value ) {
			SessionModel newSession = this.Clone();
			newSession.SessionLength = value;
			return newSession;
		}

		public ISession UpdateLevel( ITag value ) {
			SessionModel newSession = this.Clone();
			if( null == value ) {
				newSession.LevelId = 0;
				newSession.Level = null;
			} else {
				newSession.LevelId = value.Id;
				newSession.Level = new TagModel( value );
			}
			newSession.Level = value;
			return newSession;
		}

		public ISession UpdateCategory( ITag value ) {
			SessionModel newSession = this.Clone();
			if( null == value ) {
				newSession.CategoryId = 0;
				newSession.Category = null;
			} else {
				newSession.CategoryId = value.Id;
				newSession.Category = new TagModel( value );
			}
			newSession.Category = value;
			return newSession;
		}

		public ISession AddUserId( int value ) {
			SessionModel newSession = this.Clone();
			newSession.UserIds = this.UserIds.Add( value );
			return newSession;
		}

		public ISession RemoveUserId( int value ) {
			SessionModel newSession = this.Clone();
			newSession.UserIds = this.UserIds.Remove( value );
			return newSession;
		}

		public ISession AddTag( ITag value ) {
			SessionModel newSession = this.Clone();
			newSession.Tags = this.Tags.Add( value );
			return newSession;
		}

		public ISession RemoveTag( ITag value ) {
			SessionModel newSession = this.Clone();
			newSession.Tags = this.Tags.Remove( value );
			return newSession;
		}

		public ISession UpdateTimeSlot( ITimeSlot value ) {
			SessionModel newSession = this.Clone();
			if( null == value ) {
				newSession.TimeSlotId = 0;
				newSession.TimeSlot = null;
			} else {
				newSession.TimeSlotId = value.Id;
				newSession.TimeSlot = new TimeSlotModel( value );
			}
			return newSession;
		}

		public ISession UpdateRoom( IRoom value ) {
			SessionModel newSession = this.Clone();
			if( null == value ) {
				newSession.RoomId = 0;
				newSession.Room = null;
			} else {
				newSession.RoomId = value.Id;
				newSession.Room = new RoomModel( value );
			}
			return newSession;
		}

		public ISession UpdateEventId( int value ) {
			SessionModel newSession = this.Clone();
			newSession.EventId = value;
			return newSession;
		}

		public ISession UpdateSessionizeId( int? value ) {
			SessionModel newSession = this.Clone();
			newSession.SessionizeId = value;
			return newSession;
		}
		#endregion

		private SessionModel Clone() {
			SessionModel cloned = new SessionModel {
				Id = this.Id,
				UserIds = this.UserIds?.ToImmutableList(),
				Title = string.Copy( this.Title ),
				Abstract = string.Copy( this.Abstract ),
				SessionLength = this.SessionLength,
				Level = this.Level,
				LevelId = this.LevelId,
				Category = this.Category,
				CategoryId = this.CategoryId,
				Accepted = this.Accepted,
				Tags = this.Tags?.ToImmutableList(),
				TimeSlotId = this.TimeSlotId,
				RoomId = this.RoomId,
				EventId = this.EventId,
				SessionizeId = this.SessionizeId
			};

			if( null != this.TimeSlot )
				cloned.TimeSlot = new TimeSlotModel( this.TimeSlot );

			if( null != this.Room )
				cloned.Room = new RoomModel( this.Room );

			if( !string.IsNullOrWhiteSpace( cloned.Notes ) )
				cloned.Notes = string.Copy( this.Notes );

			return cloned;
		}
	}
}
