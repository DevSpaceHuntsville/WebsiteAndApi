using System.Collections.Immutable;

namespace DevSpace.Common {
	public interface ISession {
		int Id { get; }
		int UserId { get; }
		string Title { get; }
		string Abstract { get; }
		string Notes { get; }
		int SessionLength { get; }
		ITag Level { get; }
		bool? Accepted { get; }
		ImmutableList<ITag> Tags { get; }
		ITimeSlot TimeSlot { get; }
		IRoom Room { get; }
		int EventId { get; }
		int? SessionizeId { get; }

		ISession UpdateId( int value );
		ISession UpdateUserId( int value );
		ISession UpdateTitle( string value );
		ISession UpdateAbstract( string value );
		ISession UpdateNotes( string value );
		ISession UpdateAccepted( bool? value );
		ISession UpdateSessionLength( int value );
		ISession UpdateLevel( ITag value );
		ISession AddTag( ITag value );
		ISession RemoveTag( ITag value );
		ISession UpdateTimeSlot( ITimeSlot value );
		ISession UpdateRoom( IRoom value );
		ISession UpdateEventId( int value );
		ISession UpdateSessionizeId( int? value );
	}
}
