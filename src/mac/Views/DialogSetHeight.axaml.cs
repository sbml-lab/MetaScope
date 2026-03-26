using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using MetaScope.Controls;

namespace MetaScope.Views
{
	/// <summary>
	/// Interaction logic for DialogSetHeight.axaml
	/// </summary>
	public partial class DialogSetHeight : Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogSetHeight( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
		}

		public void SetElementValue()
		{
			if( m_pnlLane.LaneHeight == 0.0 )
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

		public bool? IsAutomatic
		{
			get {	return m_rbAutomatic.IsChecked; }
		}

		public bool? IsManual
		{
			get {	return m_rbManual.IsChecked; }
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			Close( true );
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
		}

		private void OnAutomaticChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbAutomatic.IsChecked == true )
				m_tbHeight.IsEnabled			= false;
		}

		private void OnManualChecked( object obj, RoutedEventArgs ea )
		{
			if( m_rbManual.IsChecked == true )
				m_tbHeight.IsEnabled			= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbHeight.Focus();
		}
	}
}
