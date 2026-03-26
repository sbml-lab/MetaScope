using System;

namespace MetaScope.Services.Error
{
	public class ExceptionAssertion : ExceptionVugmap
	{
		public static ExceptionAssertion	MakeException( object objArgument, string strReason )
		{
			ExceptionAssertion	ea				= new ExceptionAssertion( objArgument, strReason );

			return ea;
		}

		public static ExceptionAssertion	MakeException( object objArgument )
		{
			ExceptionAssertion	ea				= new ExceptionAssertion( objArgument, null );

			return ea;
		}

		public ExceptionAssertion( string strArgument )
			: this( strArgument, null )
		{
		}

		public ExceptionAssertion( object objArgument, string strReason )
			: base( String.Format( "FALSE ASSERTION : {0}('{1}') {2})",
				objArgument == null ? null : objArgument.GetType(),
				objArgument == null ? "null" : objArgument.ToString(), strReason ) )
		{
		}
	}
}
