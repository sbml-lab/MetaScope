using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Interop;

using VugMap.Utility;
using VugMap.Utility.Error;

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogZoomTo.xaml
	/// </summary>
	public partial class DialogZoomTo : System.Windows.Window
	{
		//			.								.								.
		private		DocMap							m_docMap						= null;
		private		double							m_dZoomMin						= 0.0f;
		private		double							m_dZoomMax						= 0.0f;

		public DialogZoomTo( DocMap dm )
		{
			m_docMap		= dm;

			InitializeComponent();
		}

		protected override void OnSourceInitialized( EventArgs ea )
		{
			base.OnSourceInitialized( ea );

			HwndSource		hwndSource		= PresentationSource.FromVisual( this ) as HwndSource;

			if( hwndSource != null )
			{
				hwndSource.AddHook( UtilityWindow.HwndSourceHook );
			}
		}

		public void DoZoomMinSet( double dZoom )
		{
			m_dZoomMin		= dZoom;
		}

		public void DoZoomMaxSet( double dZoom )
		{
			m_dZoomMax		= dZoom;
		}

		public void SetElementValue()
		{
			m_dZoomMin						= PnlMap.N_MAP_ZOOMMIN;
			m_dZoomMax						= PnlMap.N_MAP_ZOOMMAX;

			m_rbOther.IsChecked				= true;
			m_tbZoomTo.Text					= m_docMap.PanelActive.Zoom.ToString();
		}

		private bool DoElementCheck()
		{
			string			strZoom			= m_tbZoomTo.Text;
			double			dZoom			= 0.0f;

			bool			b				= double.TryParse( strZoom, out dZoom );
			if( dZoom < m_dZoomMin || dZoom > m_dZoomMax )
			{
				return false;
			}
			else
			{
				return b;
			}			
		}

		public double DoZoomGet()
		{
			if( m_rbMinimum.IsChecked == true )
			{
				return m_dZoomMin;
			}
			else if( m_rbMaximum.IsChecked == true )
			{
				return m_dZoomMax;
			}
			else
			{
				string			strZoom			= m_tbZoomTo.Text;
				double			dZoom			= double.Parse( strZoom );

				return dZoom;
			}		
		}


		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			bool			b				= DoElementCheck();

			if( b == true )
			{
				DialogResult						= true;
				
				Close();
			}	
			else
			{
				string			str				= m_tbZoomTo.Text;

				ErrorMessage.ShowErrorZoomToInvalid( str, m_dZoomMin, m_dZoomMax );
			}	
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnMinimumChecked( object obj, RoutedEventArgs ea )
		{
			m_tbZoomTo.IsEnabled			= false;
		}

		private void OnMaximumChecked( object obj, RoutedEventArgs ea )
		{
			m_tbZoomTo.IsEnabled			= false;
		}

		private void OnOtherChecked( object obj, RoutedEventArgs ea )
		{
			m_tbZoomTo.IsEnabled			= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_lblZoomMin.Content			= string.Format( "{0}X", m_dZoomMin );
			m_lblZoomMax.Content			= string.Format( "{0}X", m_dZoomMax );

			m_rbOther.IsChecked				= true;
			m_tbZoomTo.Focus();
		}
	}
}
