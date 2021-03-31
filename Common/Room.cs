using System.Runtime.Serialization;

namespace DevSpace.Common {
	[DataContract]
	public class Room : IRoom {
		private Room() { }

		// Needed for Session
		internal Room( IRoom Room ) {
			this.Id = Room.Id;
			this.DisplayName = Room.DisplayName;
		}

		#region IRoom
		[DataMember]	public int Id { get; private set; }
		[DataMember]	public string DisplayName { get; private set; }

		public IRoom UpdateId( int newId ) {
			return new Room {
				Id = newId,
				DisplayName = this.DisplayName,
			};
		}

		public IRoom UpdateDisplayName( string newDisplayName ) {
			return new Room {
				Id = this.Id,
				DisplayName = newDisplayName,
			};
		}
		#endregion
	}
}
