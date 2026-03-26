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
	/// Interaction logic for DialogPositionTo.axaml
	/// </summary>
	public partial class DialogPositionTo : Window
	{
		//			.								.								.
		private		PnlMap							m_pnlMap						= null;
		private		int								m_nPositionMin					= 0;
		private		int								m_nPositionMax					= 0;

		public DialogPositionTo( PnlMap pnlMap )
		{
			m_pnlMap		= pnlMap;

			InitializeComponent();
		}

		public void DoPositionMinSet( int nPosition )
		{
			m_nPositionMin	= nPosition;
		}

		public void DoPositionMaxSet( int nPosition )
		{
			m_nPositionMax		= nPosition;
		}

		public void SetElementValue()
		{
			m_nPositionMin					= m_pnlMap.PositionMin;
			m_nPositionMax					= m_pnlMap.PositionMax;

			m_rbOther.IsChecked				= true;
			m_tbPositionTo.Text				= m_pnlMap.Position.ToString();
		}

		private bool DoElementCheck()
		{
			string			strPosition		= m_tbPositionTo.Text;
			int				nPosition		= 0;

			bool			b				= int.TryParse( strPosition, out nPosition );
			if( nPosition < m_nPositionMin || nPosition > m_nPositionMax )
			{
				return false;
			}
			else
			{
				return b;
			}
		}

		public int DoPositionGet()
		{
			if( m_rbStart.IsChecked == true )
			{
				return m_nPositionMin;
			}
			else if( m_rbEnd.IsChecked == true )
			{
				return m_nPositionMax;
			}
			else
			{
				string			strPosition		= m_tbPositionTo.Text;
				int				nPosition		= int.Parse( strPosition );

				return nPosition;
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
				string			str				= m_tbPositionTo.Text;

				ErrorMessage.ShowErrorPositionToInvalid( str, m_nPositionMin, m_nPositionMax );
			}
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
		}

		private void OnStartChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbStart.IsChecked == true )
				m_tbPositionTo.IsEnabled		= false;
		}

		private void OnEndChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbEnd.IsChecked == true )
				m_tbPositionTo.IsEnabled		= false;
		}

		private void OnOtherChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbOther.IsChecked == true )
				m_tbPositionTo.IsEnabled		= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_lblPositionMin.Content		= string.Format( "{0:N0}", m_nPositionMin );
			m_lblPositionMax.Content		= string.Format( "{0:N0}", m_nPositionMax );

			m_rbOther.IsChecked				= true;
			m_tbPositionTo.Focus();
		}
	}
}
