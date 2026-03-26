using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaScope.Services
{
	public class UtilityMath
	{
		public static int DoRound( double d )
		{
			int				n				= ( int ) ( d + 0.5 );

			return n;
		}

		public static double GetMedian( double[] dA )
		{
			int				nSize			= dA.Length;
			int				nMid			= nSize / 2;

			double			dMedian			= ( nSize % 2 != 0 ) ? dA[ nMid ] : ( dA[ nMid ] + dA[ nMid - 1 ] ) / 2;

			return dMedian;
		}

		public static double GetAverage( double[] dA )
		{
			double			dSum			= 0;
			foreach( double d in dA )
			{
				dSum			+= d;
			}

			double			dAverage		= dSum / dA.Length;

			return dAverage;
		}
	}
}
