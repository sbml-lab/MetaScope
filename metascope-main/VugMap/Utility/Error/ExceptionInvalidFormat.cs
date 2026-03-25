using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VugMap.Utility.Error
{
	public class ExceptionInvalidFormat : ExceptionVugmap
	{
		public static ExceptionInvalidFormat	MakeException( string strReason )
		{
			ExceptionInvalidFormat		eia		= new ExceptionInvalidFormat( strReason );

			return eia;
		}

		public ExceptionInvalidFormat( string strReason )			
			: base( String.Format( "INVALID FORMAT : {0})", strReason ) )
		{
		}
	}
}