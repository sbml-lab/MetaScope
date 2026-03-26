using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using MetaScope.Controls;

namespace MetaScope.Views
{
	/// <summary>
	/// Interaction logic for DialogSetScale.axaml
	/// </summary>
	public partial class DialogSetScale : Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogSetScale( PnlMapLane pnlLane )
		{
			m_pnlLane						= pnlLane;

			InitializeComponent();
		}

		public bool? IsNone
		{
			get {	return m_rbNone.IsChecked; }
		}

		public bool? IsManual
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

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			Close( true );
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
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

		private void OnManualGotFocus( object obj, GotFocusEventArgs ea )
		{
			m_rbManual.IsChecked			= true;
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbManualMax.Focus();
		}
	}
}
