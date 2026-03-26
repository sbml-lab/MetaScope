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
using System.Windows.Interop;

using VugMap.Utility;

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogSetScale.xaml
	/// </summary>
	public partial class DialogSetScale : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogSetScale( PnlMapLane pnlLane )
		{
			m_pnlLane						= pnlLane;

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

		public Nullable< bool > IsNone
		{
			get {	return m_rbNone.IsChecked; }
		}

		public Nullable< bool > IsManual
		{
			get {	return m_rbManual.IsChecked; }
		}

		public string NoneMax
		{
			get {	return m_tbNoneMax.Text; }
		}

		public string NoneMin
		{
			get {	return m_tbNoneMin.Text; }
		}

		public string ManualMax
		{
			get {	return m_tbManualMax.Text; }
		}

		public string ManualMin
		{
			get {	return m_tbManualMin.Text; }
		}

		private void OnOkClick( object obj , RoutedEventArgs ea )
		{
			DialogResult					= true;

			Close();
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		public void SetNone( double dScaleMax, double dScaleMin )
		{
			m_rbNone.IsChecked				= true;
			m_tbNoneMax.Text				= dScaleMax.ToString();
			m_tbNoneMin.Text				= dScaleMin.ToString();
		}
		
		public void SetManual( double dScaleMax, double dScaleMin )
		{
			m_rbManual.IsChecked			= true;			
			m_tbManualMax.Text				= dScaleMax.ToString();
			m_tbManualMin.Text				= dScaleMin.ToString();
		}

		private void OnManualGotFocus( object obj, RoutedEventArgs ea )
		{
			m_rbManual.IsChecked			= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbManualMax.Focus();
		}
	}
}
