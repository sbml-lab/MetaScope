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
using System.Windows.Shapes;

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogFeatureAddSize.xaml
	/// </summary>
	public partial class DialogFeatureAddSize : System.Windows.Window
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
			Close();
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			DialogResult					= true;

			Close();
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbWidth.Focus();
		}
	}
}
