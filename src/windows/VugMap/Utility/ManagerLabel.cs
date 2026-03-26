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
	using				ListLabel						= List< Label >;

	public class ManagerLabel
	{
		//				.								.								.
		private			static ManagerLabel				S_MANAGER						= null;

		static ManagerLabel()
		{
			S_MANAGER				= new ManagerLabel();
		}

		public static ManagerLabel GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER				= new ManagerLabel( N_COUNTREADY );
			}

			return S_MANAGER;
		}

		private			static int					N_COUNTREADY					= 30;		

		private			ListLabel					m_lstLabel						= null;
		private			ListLabel					m_lstReady						= null;

		public ManagerLabel( int nCountReady )
			: this()
		{
			m_lstLabel		= new ListLabel( nCountReady );
			m_lstReady		= new ListLabel( nCountReady );

			lock( this )
			{
				for( int i = 0; i < nCountReady; i++ )
				{
					Label			lbl				= MakeLabel();

					m_lstLabel.Add( lbl );
					m_lstReady.Add( lbl );
				}
			}
		}


		public ManagerLabel()
		{
			m_lstLabel		= new ListLabel();
			m_lstReady		= new ListLabel();
		}

		public Label GetLabel()
		{
			Label			lblReturn		= null;

			lock( this )
			{
				if( m_lstReady.Count == 0 )
				{
					lblReturn		= MakeLabel();
					
					m_lstLabel.Add( lblReturn );
				}
				else
				{
					lblReturn		= m_lstReady[ 0 ];
					m_lstReady.RemoveAt( 0 );
				}
			}

			return lblReturn;
		}

		public void ReleaseLabel( Label lbl )
		{
			lock( this )
			{
				m_lstReady.Add( lbl );
			}
		}

		public void ReleaseLine( List< Label > lst )
		{
			lock( this )
			{
				m_lstReady.AddRange( lst );
			}
		}

		private Label MakeLabel()
		{
			Label			lbl				= new Label();
			lbl.FontFamily					= new FontFamily( "Calibri" );
			lbl.Padding						= new Thickness( 1.0f );			
			lbl.Visibility					= Visibility.Visible;
			
			return lbl;
		}
					
	}
}
