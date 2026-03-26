using System;

namespace MetaScope.Services.Error
{
	public class ExceptionInvalidArgument : ExceptionVugmap
	{
		public static ExceptionInvalidArgument	MakeException( object objArgument, string strReason )
		{
			ExceptionInvalidArgument	eia		= new ExceptionInvalidArgument( objArgument, strReason );

			return eia;
		}

		public static ExceptionInvalidArgument	MakeException( object objArgument )
		{
			ExceptionInvalidArgument	eia		= new ExceptionInvalidArgument( objArgument, null );

			return eia;
		}

		public ExceptionInvalidArgument( string strArgument )
			: this( strArgument, null )
		{
		}

		public ExceptionInvalidArgument( object objArgument, string strReason )
			: base( String.Format( "INVALID ARGUEMENT : {0}('{1}') {2})",
			objArgument == null ? null : objArgument.GetType(),
			objArgument == null ? "null" : objArgument.ToString(), strReason ) )
		{
		}
	}
}
