using System.Data.SqlClient;
using System.Reflection;
using DevSpace.Common;

namespace DevSpace.Database.Models {
	internal class SponsorLevelModel : ISponsorLevel {
		internal SponsorLevelModel( SqlDataReader dataReader ) {
			for( int lcv = 0; lcv < dataReader.FieldCount; ++lcv ) {
				GetType().GetProperty( dataReader.GetName( lcv ), BindingFlags.Instance | BindingFlags.Public )?.SetValue( this, dataReader.GetValue( lcv ) );
			}
		}

		public bool DisplayInEmails { get; internal set; }
		public bool DisplayInSidebar { get; internal set; }
		public string DisplayName { get; internal set; }
		public int DisplayOrder { get; internal set; }
		public int Id { get; internal set; }
		public bool DisplayLink { get; internal set; }
		public int TimeOnScreen { get; internal set; }
	}
}
