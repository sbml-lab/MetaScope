using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VugMap.Utility.Error
{
	public class ExceptionVugmap : System.ApplicationException
	{
		public ExceptionVugmap()
		{
		}

		public ExceptionVugmap( string strException )
			: base( strException )
		{				
		}

		public override string ToString()
		{
			string	strMessage			= string.Format( "{0}{1}{1}***STACK***{1}{2}",
											Message,
											Environment.NewLine,
											StackTrace );

			return strMessage;
		}
	}
}