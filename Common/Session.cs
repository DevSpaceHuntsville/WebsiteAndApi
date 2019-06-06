using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace DevSpace.Common {
	[DataContract]
	[KnownType( typeof( Tag ) )]
	[KnownType( typeof( TimeSlot ) )]
	[KnownType( typeof( Room ) )]
	public class Session : ISession {
		private Session() {
			this._tags = new List<Tag>();
		}

		#region ISession
		[DataMember]public string Abstract { get; private set; }
		[DataMember]public bool? Accepted { get; private set; }
		[DataMember]public int Id { get; private set; }
		[DataMember]public string Notes { get; private set; }
		[DataMember]public string Title { get; private set; }
		[DataMember]public int SessionLength { get; private set; }
		[DataMember]public int UserId { get; private set; }

		[DataMember( Name = "Level" )] private Tag _level;
		public ITag Level {
			get {
				return _level;
			}
			private set {
				_level = new Tag( value );
			}
		}

		[DataMember( Name = "Tags" )]private List<Tag> _tags;
		public ImmutableList<ITag> Tags {
			get {
				return ImmutableList<ITag>.Empty.AddRange( _tags );
			}
		}

		[DataMember]
		private TimeSlot _timeSlot;
		public ITimeSlot TimeSlot {
			get {
				return _timeSlot;
			}
			private set {
				_timeSlot = new TimeSlot( value );
			}
		}

		[DataMember]
		private Room _room;
		public IRoom Room {
			get {
				return _room;
			}
			private set {
				_room = new Room( value );
			}
		}

		[DataMember] public int EventId { get; private set; }
		[DataMember] public int? SessionizeId { get; private set; }

		public ISession UpdateAbstract( string value ) {
			Session newSession = this.Clone();
			newSession.Abstract = value;
			return newSession;
		}

		public ISession UpdateAccepted( bool? value ) {
			Session newSession = this.Clone();
			newSession.Accepted = value;
			return newSession;
		}

		public ISession UpdateId( int value ) {
			Session newSession = this.Clone();
			newSession.Id = value;
			return newSession;
		}

		public ISession UpdateNotes( string value ) {
			Session newSession = this.Clone();
			newSession.Notes = value;
			return newSession;
		}

		public ISession UpdateTitle( string value ) {
			Session newSession = this.Clone();
			newSession.Title = value;
			return newSession;
		}

		public ISession UpdateSessionLength( int value ) {
			Session newSession = this.Clone();
			newSession.SessionLength = value;
			return newSession;
		}

		public ISession UpdateUserId( int value ) {
			Session newSession = this.Clone();
			newSession.UserId = value;
			return newSession;
		}

		public ISession UpdateLevel( ITag value ) {
			Session newSession = this.Clone();
			newSession.Level = value;
			return newSession;
		}

		public ISession AddTag( ITag value ) {
			Session newSession = this.Clone();
			newSession._tags.Add( new Tag( value ) );
			return newSession;
		}

		public ISession RemoveTag( ITag value ) {
			Session newSession = this.Clone();
			newSession._tags.Remove( new Tag( value ) );
			return newSession;
		}

		public ISession UpdateTimeSlot( ITimeSlot value ) {
			Session newSession = this.Clone();
			newSession.TimeSlot = value;
			return newSession;
		}

		public ISession UpdateRoom( IRoom value ) {
			Session newSession = this.Clone();
			newSession.Room = value;
			return newSession;
		}

		public ISession UpdateEventId( int value ) {
			Session newSession = this.Clone();
			newSession.EventId = value;
			return newSession;
		}

		public ISession UpdateSessionizeId( int? value ) {
			Session newSession = this.Clone();
			newSession.SessionizeId = value;
			return newSession;
		}
		#endregion

		private Session Clone() {
			Session cloned = new Session {
				Id = this.Id,
				UserId = this.UserId,
				Title = string.Copy( this.Title ),
				Abstract = string.Copy( this.Abstract ),
				SessionLength = this.SessionLength,
				Level = this.Level,
				Accepted = this.Accepted,
				EventId = this.EventId,
				SessionizeId = this.SessionizeId
			};

			if( null != TimeSlot )
				cloned.TimeSlot = new TimeSlot( this.TimeSlot );

			if( null != Room )
				cloned.Room = new Room( this.Room );

			if( !string.IsNullOrWhiteSpace( this.Notes ) )
				cloned.Notes = string.Copy( this.Notes );

			foreach( ITag tag in this._tags ) {
				cloned._tags.Add( new Tag( tag ) );
			}

			return cloned;
		}
	}
}
