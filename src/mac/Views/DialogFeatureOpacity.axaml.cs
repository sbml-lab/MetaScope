using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

using MetaScope.Services;
using MetaScope.Services.Error;

namespace MetaScope.Views
{
	public partial class DialogFeatureOpacity : Window
	{
		public DialogFeatureOpacity()
		{
			InitializeComponent();
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
				this.Close( true );
			}
			else
			{
				string			str				= m_tbOpacity.Text;

				ErrorMessage.ShowErrorOpacityInvalid( str );
			}
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			this.Close( false );
		}

		private void OnDefaultChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbDefault.IsChecked == true )
			{
				m_tbOpacity.IsEnabled			= false;
			}
		}

		private void OnManualChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbManual.IsChecked == true )
			{
				m_tbOpacity.IsEnabled			= true;
			}
		}

		protected override void OnOpened( EventArgs ea )
		{
			base.OnOpened( ea );

			m_tbOpacity.Focus();
		}
	}
}
