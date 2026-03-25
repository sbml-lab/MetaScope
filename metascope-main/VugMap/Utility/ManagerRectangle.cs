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
	using				ListRectangle					= List< Rectangle >;

	class ManagerRectangle
	{
		//				.								.								.
		private			static ManagerRectangle			S_MANAGER						= null;

		static ManagerRectangle()
		{
			S_MANAGER				= new ManagerRectangle();
		}

		public static ManagerRectangle GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER				= new ManagerRectangle( N_COUNTREADY );
			}

			return S_MANAGER;
		}

		private			static int						N_COUNTREADY					= 2000 * 5;		

		private			ListRectangle					m_lstRect						= null;
		private			ListRectangle					m_lstReady						= null;

		public ManagerRectangle( int nCountReady )
			: this()
		{
			m_lstRect		= new ListRectangle( nCountReady );
			m_lstReady		= new ListRectangle( nCountReady );

			lock( this )
			{
				for( int i = 0; i < nCountReady; i++ )
				{
					Rectangle		rt				= MakeRectangle();

					m_lstRect.Add( rt );
					m_lstReady.Add( rt );
				}
			}
		}


		public ManagerRectangle()
		{
			m_lstRect		= new ListRectangle();
			m_lstReady		= new ListRectangle();
		}

		public Rectangle GetRectangle()
		{
			Rectangle		rtReturn		= null;

			lock( this )
			{
				if( m_lstReady.Count == 0 )
				{
					rtReturn		= MakeRectangle();
					
					m_lstRect.Add( rtReturn );
				}
				else
				{
					rtReturn		= m_lstReady[ 0 ];
					m_lstReady.RemoveAt( 0 );
				}
			}

			return rtReturn;
		}

		public void ReleaseRectangle( Rectangle rt )
		{
			lock( this )
			{
				m_lstReady.Add( rt );
			}
		}

		public void ReleaseRectangle( List< Rectangle > lst )
		{
			lock( this )
			{
				m_lstReady.AddRange( lst );
			}
		}

		private Rectangle MakeRectangle()
		{
			Rectangle		rt				= new Rectangle();
			rt.Visibility					= Visibility.Collapsed;
			rt.RenderTransform				= new TranslateTransform();

			return rt;
		}
					
	}
}
