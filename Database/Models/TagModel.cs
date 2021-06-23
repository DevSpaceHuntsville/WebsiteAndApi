using System.Data.SqlClient;
using System.Reflection;
using DevSpace.Common;

namespace DevSpace.Database.Models {
	public class TagModel : ITag {
		private TagModel() {}

		internal TagModel( ITag tag ) {
			this.Id = tag.Id;
			this.Text = tag.Text;
		}

		internal TagModel( SqlDataReader dataReader ) {
			for( int lcv = 0; lcv < dataReader.FieldCount; ++lcv ) {
				GetType().GetProperty( dataReader.GetName( lcv ), BindingFlags.Instance | BindingFlags.Public )?.SetValue( this, dataReader.GetValue( lcv ) );
			}
		}

		#region ITag
		public int Id { get; internal set; }
		public string Text { get; internal set; }

		public ITag UpdateId( int newId ) {
			return new TagModel {
				Id = newId,
				Text = string.Copy( this.Text )
			};
		}

		public ITag UpdateText( string newText ) {
			return new TagModel {
				Id = this.Id,
				Text = string.Copy( newText )
			};
		}
		#endregion
	}
}
