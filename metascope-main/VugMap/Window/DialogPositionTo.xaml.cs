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
	/// Interaction logic for DialogPositionTo.xaml
	/// </summary>
	public partial class DialogPositionTo : System.Windows.Window
	{
		//			.								.								.
		private		DocMap							m_docMap						= null;
		private		int								m_nPositionMin					= 0;
		private		int								m_nPositionMax					= 0;

		public DialogPositionTo( DocMap dm )
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
			m_nPositionMin					= m_docMap.PanelActive.PositionMin;
			m_nPositionMax					= m_docMap.PanelActive.PositionMax;

			m_rbOther.IsChecked				= true;
			m_tbPositionTo.Text				= m_docMap.PanelActive.Position.ToString();
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
				DialogResult						= true;
				
				Close();
			}	
			else
			{
				string			str				= m_tbPositionTo.Text;

				ErrorMessage.ShowErrorPositionToInvalid( str, m_nPositionMin, m_nPositionMax );
			}	
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnStartChecked( object obj, RoutedEventArgs ea )
		{
			m_tbPositionTo.IsEnabled		= false;
		}

		private void OnEndChecked( object obj, RoutedEventArgs ea )
		{
			m_tbPositionTo.IsEnabled		= false;
		}

		private void OnOtherChecked( object obj, RoutedEventArgs ea )
		{
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
