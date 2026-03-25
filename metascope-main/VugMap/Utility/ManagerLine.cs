using System;
using System.Collections.Generic;
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
	using				ListLine						= List< Line >;

	public class ManagerLine
	{
		//				.								.								.
		private			static ManagerLine				S_MANAGER						= null;

		static ManagerLine()
		{
			S_MANAGER				= new ManagerLine();
		}

		public static ManagerLine GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER				= new ManagerLine( N_COUNTREADY );
			}

			return S_MANAGER;
		}

		private			static int					N_COUNTREADY					= 30;		

		private			ListLine					m_lstLine						= null;
		private			ListLine					m_lstReady						= null;

		public ManagerLine( int nCountReady )
			: this()
		{
			m_lstLine		= new ListLine( nCountReady );
			m_lstReady		= new ListLine( nCountReady );

			lock( this )
			{
				for( int i = 0; i < nCountReady; i++ )
				{
					Line			rt				= MakeLine();

					m_lstLine.Add( rt );
					m_lstReady.Add( rt );
				}
			}
		}


		public ManagerLine()
		{
			m_lstLine		= new ListLine();
			m_lstReady		= new ListLine();
		}

		public Line GetLine()
		{
			Line			lnReturn		= null;

			lock( this )
			{
				if( m_lstReady.Count == 0 )
				{
					lnReturn		= MakeLine();
					
					m_lstLine.Add( lnReturn );
				}
				else
				{
					lnReturn		= m_lstReady[ 0 ];
					m_lstReady.RemoveAt( 0 );
				}
			}

			return lnReturn;
		}

		public void ReleaseLine( Line ln )
		{
			lock( this )
			{
				m_lstReady.Add( ln );
			}
		}

		public void ReleaseLine( List< Line > lst )
		{
			lock( this )
			{
				m_lstReady.AddRange( lst );
			}
		}

		private Line MakeLine()
		{
			Line			ln				= new Line();
			ln								= new Line();			
			ln.Stroke						= Brushes.Black;
			ln.StrokeThickness				= 1.0f;
			ln.HorizontalAlignment			= HorizontalAlignment.Center;
			ln.VerticalAlignment			= VerticalAlignment.Center;
			ln.SnapsToDevicePixels			= true;
			ln.Visibility					= Visibility.Collapsed;

			return ln;
		}
					
	}
}
