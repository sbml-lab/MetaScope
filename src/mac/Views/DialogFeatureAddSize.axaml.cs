using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MetaScope.Views
{
	public partial class DialogFeatureAddSize : Window
	{
		public DialogFeatureAddSize()
		{
			InitializeComponent();
		}

		public bool IsPositionStart()
		{
			if( m_rbPosStart.IsChecked == true )
				return true;
			else
				return false;
		}

		public bool PositionStart
		{
			get {	return IsPositionStart(); }
		}

		public int GetWidth()
		{
			if( m_rbWidth50.IsChecked == true )
				return 50;
			else if( m_rbWidth100.IsChecked == true )
				return 100;
			else if( m_rbWidth150.IsChecked == true )
				return 150;
			else if( m_rbWidth200.IsChecked == true )
				return 200;
			else if( m_rbWidth250.IsChecked == true )
				return 250;
			else if( m_rbWidth300.IsChecked == true )
				return 300;
			else
			{
				int				nWidth			= int.Parse( m_tbWidth.Text );

				return nWidth;
			}
		}

		public void SetWidth( int nWidth )
		{
			m_tbWidth.Text					= nWidth.ToString();
			m_rbWidthArbitrary.IsChecked	= true;
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			this.Close( false );
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			this.Close( true );
		}

		protected override void OnOpened( EventArgs ea )
		{
			base.OnOpened( ea );

			m_tbWidth.Focus();
		}
	}
}
