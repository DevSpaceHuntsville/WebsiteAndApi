using System;

namespace DevSpace.Common {
	public interface IArticle {
		int Id { get; }
		string Title { get; }
		string Body { get; }
		DateTime PublishDate { get; }
		DateTime ExpireDate { get; }

		IArticle UpdateId( int id );
		IArticle UpdateTitle( string title );
		IArticle UpdateBody( string body );
		IArticle UpdatePublishDate( DateTime publishDate );
		IArticle UpdateExpireDate( DateTime expireDate );
	}
}
