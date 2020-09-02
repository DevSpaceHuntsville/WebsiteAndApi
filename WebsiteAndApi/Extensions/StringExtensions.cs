using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevSpace {
	public static class StringExtensions {
		public static string IfNullOrWhiteSpace( this string o, string d ) =>
			string.IsNullOrWhiteSpace( o ) ? d : o;
	}
}