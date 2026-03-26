using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VugMap.Utility
{
	class UtilityString
	{
		public static string GetNumberCommaed( long lNumber )
		{
			string			strCommaed		= lNumber.ToString( "N0", CultureInfo.InvariantCulture );

			return strCommaed;
		}
	}
}
