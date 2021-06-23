using System;
using System.Data.SqlClient;
using System.Reflection;
using DevSpace.Common;

namespace DevSpace.Database.Models {
	public class ArticleModel : IArticle {
		private ArticleModel() { }
		private ArticleModel(
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

		internal ArticleModel( IArticle article ) {
			this.Id = article.Id;
			this.Title = article.Title;
			this.Body = article.Body;
			this.PublishDate = article.PublishDate;
			this.ExpireDate = article.ExpireDate;
		}

		internal ArticleModel( SqlDataReader dataReader ) {
			for( int lcv = 0; lcv < dataReader.FieldCount; ++lcv ) {
				GetType()
					.GetProperty( dataReader.GetName( lcv ), BindingFlags.Instance | BindingFlags.Public )
					?.SetValue( this, dataReader.GetValue( lcv ) );
			}
		}

		public int Id { get; internal set; }
		public string Title { get; internal set; }
		public string Body { get; internal set; }
		public DateTime PublishDate { get; internal set; }
		public DateTime ExpireDate { get; internal set; }

		public IArticle UpdateId( int id ) => new ArticleModel( id, this.Title, this.Body, this.PublishDate, this.ExpireDate );
		public IArticle UpdateTitle( string title ) => new ArticleModel( this.Id, title, this.Body, this.PublishDate, this.ExpireDate );
		public IArticle UpdateBody( string body ) => new ArticleModel( this.Id, this.Title, body, this.PublishDate, this.ExpireDate );
		public IArticle UpdatePublishDate( DateTime publishDate ) => new ArticleModel( this.Id, this.Title, this.Body, publishDate, this.ExpireDate );
		public IArticle UpdateExpireDate( DateTime expireDate ) => new ArticleModel( this.Id, this.Title, this.Body, this.PublishDate, expireDate );
	}
}
