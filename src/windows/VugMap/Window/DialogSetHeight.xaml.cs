using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogSetHeight.xaml
	/// </summary>
	public partial class DialogSetHeight : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogSetHeight( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;
							
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
			if( m_pnlLane.LaneHeight == 0.0f )
			{
				// Auto
				m_rbAutomatic.IsChecked			= true;								
				m_rbManual.IsChecked			= false;
				m_tbHeight.IsEnabled			= false;
				m_tbHeight.Text					= m_pnlLane.LaneHeightActual.ToString();
			}
			else
			{
				// Manual
				m_rbAutomatic.IsChecked			= false;												
				m_rbManual.IsChecked			= true;				
				m_tbHeight.IsEnabled			= true;
				m_tbHeight.Text					= m_pnlLane.LaneHeight.ToString();
			}
		}

		public string LaneHeight
		{
			get {	return m_tbHeight.Text; }
		}

		public Nullable< bool > IsAutomatic
		{
			get {	return m_rbAutomatic.IsChecked; }
		}

		public Nullable< bool > IsManual
		{
			get {	return m_rbManual.IsChecked; }
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			DialogResult						= true;

			Close();
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnAutomaticChecked( object obj, RoutedEventArgs ea )
		{
			m_tbHeight.IsEnabled			= false;
		}

		private void OnManualChecked( object obj, RoutedEventArgs ea )
		{
			m_tbHeight.IsEnabled			= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbHeight.Focus();
		}
	}
}
