using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using AvalonDock;
using VugMap.Utility.Logger;

namespace VugMap.Utility
{
	using		DicPen						= Dictionary< string, Pen >;

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

		public Pen GetPen( Brush bsh )
		{
			SolidColorBrush	scb				= bsh as SolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );
			
			Pen				pen				= GetPen( strColor );

			return pen;
		}
		
		public Pen GetPen( string strColor )
		{
			Pen				pen				= null;
			bool			bReturn			= m_dicPen.TryGetValue( strColor, out pen );

			if( bReturn == false )
			{	
				Brush			bsh				= ManagerBrush.GetManager().GetBrush( strColor );
				
				pen								= new Pen( bsh, 1.0f );
				pen.Freeze();

				m_dicPen.Add( strColor, pen );
			}

			return pen;			
		}
	}
}
