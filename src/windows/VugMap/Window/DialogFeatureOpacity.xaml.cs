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
	/// Interaction logic for DialogFeatureOpacity.xaml
	/// </summary>
	public partial class DialogFeatureOpacity : System.Windows.Window
	{
		public DialogFeatureOpacity()
		{
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

		public void SetElementValue()
		{
			string			strOpacity		= string.Format( "0x{0}", ManagerBrush.DoOpacityGet() );
											

			m_rbManual.IsChecked			= true;
			m_tbOpacity.Text				= strOpacity;
		}

		public string DoOpacityGet()
		{
			string			strOpacity		= m_tbOpacity.Text;

			if( strOpacity.StartsWith( "0x" ) == true )
			{
				strOpacity		= strOpacity.Substring( 2 );
			}
			else 
			{
				byte			b				= byte.Parse( strOpacity );
				strOpacity						= string.Format( "{0:X2}", b );
			}

			return strOpacity;
		}

		private bool DoElementCheck()
		{
			string			strOpacity			= m_tbOpacity.Text;
			byte			bOpacity			= 0;

			if( strOpacity.StartsWith( "0x" ) == true )
			{
				strOpacity		= strOpacity.Substring( 2 );

				bool			b				= byte.TryParse( strOpacity, NumberStyles.HexNumber, null, out bOpacity );

				return b;
			}
			else
			{				
				bool			b				= byte.TryParse( strOpacity, out bOpacity );

				return b;
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
				string			str				= m_tbOpacity.Text;

				ErrorMessage.ShowErrorOpacityInvalid( str );
			}			
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnDefaultChecked( object obj, RoutedEventArgs ea )
		{
			m_tbOpacity.IsEnabled			= false;
		}

		private void OnManualChecked( object obj, RoutedEventArgs ea )
		{
			m_tbOpacity.IsEnabled			= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbOpacity.Focus();
		}
	}
}
