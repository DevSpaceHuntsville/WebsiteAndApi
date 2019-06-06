using System;

namespace DevSpace.Common {
	public interface IUser {
		int Id { get; }
		string DisplayName { get; }
		string EmailAddress { get; }
		string Bio { get; }
		string Twitter { get; }
		byte Permissions { get; }
		string PasswordHash { get; }
		string Website { get; }
		Guid SessionToken { get; }
		DateTime SessionExpires { get; }
		string Blog { get; }
		string ProfilePicture { get; }
		Guid? SessionizeId { get; }

		IUser UpdateId( int newId );
		IUser UpdateDisplayName( string newDisplayName );
		IUser UpdateEmailAddress( string newEmailAddress );
		IUser UpdateBio( string newBio );
		IUser UpdateTwitter( string newTwitter );
		IUser UpdatePermissions( byte newPermissions );
		IUser UpdatePasswordHash( string newPasswordHash );
		IUser UpdateWebsite( string newWebsite );
		IUser UpdateSessionToken( Guid newSessionToken );
		IUser UpdateSessionExpires( DateTime newSessionExpires );
		IUser UpdateBlog( string newBlog );
		IUser UpdateProfilePicture( string newProfilePicture );
		IUser UpdateSessionizeId( Guid? newSessionizeId );
	}
}
