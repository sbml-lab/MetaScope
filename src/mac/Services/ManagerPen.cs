using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace MetaScope.Services
{
	using		DicPen						= Dictionary< string, IPen >;

	class ManagerPen
	{
		//			.								.								.
		private		static ManagerPen				S_MANAGER						= null;

		static ManagerPen()
		{
			S_MANAGER			= new ManagerPen();
		}

		public static ManagerPen GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER			= new ManagerPen();
			}

			return S_MANAGER;
		}

		private		DicPen							m_dicPen						= null;

		public ManagerPen()
		{
			m_dicPen			= new DicPen();
		}

		public IPen GetPen( IBrush bsh )
		{
			ISolidColorBrush	scb				= bsh as ISolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );

			IPen				pen				= GetPen( strColor );

			return pen;
		}

		public IPen GetPen( string strColor )
		{
			IPen				pen				= null;
			bool			bReturn			= m_dicPen.TryGetValue( strColor, out pen );

			if( bReturn == false )
			{
				IBrush			bsh				= ManagerBrush.GetManager().GetBrush( strColor );

				pen								= new Pen( bsh, 1.0 );

				m_dicPen.Add( strColor, pen );
			}

			return pen;
		}
	}
}
