using System;
using System.Runtime.Serialization;

namespace DevSpace.Common {
	[DataContract]
	public class Article : IArticle {
		private Article() { }
		internal Article(
			int id,
			string title,
			string body,
			DateTime publishDate,
			DateTime expireDate
		) {
			this.Id = id;
			this.Title = title;
			this.Body = body;
			this.PublishDate = publishDate;
			this.ExpireDate = expireDate;
		}

		[DataMember]public int Id { get; private set; }
		[DataMember]public string Title { get; private set; }
		[DataMember]public string Body { get; private set; }
		[DataMember]public DateTime PublishDate { get; private set; }
		[DataMember]public DateTime ExpireDate { get; private set; }

		public IArticle UpdateId( int id ) => new Article( id, this.Title, this.Body, this.PublishDate, this.ExpireDate );
		public IArticle UpdateTitle( string title ) => new Article( this.Id, title, this.Body, this.PublishDate, this.ExpireDate );
		public IArticle UpdateBody( string body ) => new Article( this.Id, this.Title, body, this.PublishDate, this.ExpireDate );
		public IArticle UpdatePublishDate( DateTime publishDate ) => new Article( this.Id, this.Title, this.Body, publishDate, this.ExpireDate );
		public IArticle UpdateExpireDate( DateTime expireDate ) => new Article( this.Id, this.Title, this.Body, this.PublishDate, expireDate );
	}
}
