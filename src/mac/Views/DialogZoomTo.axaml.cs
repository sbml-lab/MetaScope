using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using MetaScope.Controls;
using MetaScope.Services.Error;

namespace MetaScope.Views
{
	/// <summary>
	/// Interaction logic for DialogZoomTo.axaml
	/// </summary>
	public partial class DialogZoomTo : Window
	{
		//			.								.								.
		private		PnlMap							m_pnlMap						= null;
		private		double							m_dZoomMin						= 0.0;
		private		double							m_dZoomMax						= 0.0;

		public DialogZoomTo( PnlMap pnlMap )
		{
			m_pnlMap		= pnlMap;

			InitializeComponent();
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
			m_tbZoomTo.Text					= m_pnlMap.Zoom.ToString();
		}

		private bool DoElementCheck()
		{
			string			strZoom			= m_tbZoomTo.Text;
			double			dZoom			= 0.0;

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
				Close( true );
			}
			else
			{
				string			str				= m_tbZoomTo.Text;

				ErrorMessage.ShowErrorZoomToInvalid( str, m_dZoomMin, m_dZoomMax );
			}
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
		}

		private void OnMinimumChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbMinimum.IsChecked == true )
				m_tbZoomTo.IsEnabled			= false;
		}

		private void OnMaximumChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbMaximum.IsChecked == true )
				m_tbZoomTo.IsEnabled			= false;
		}

		private void OnOtherChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbOther.IsChecked == true )
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
