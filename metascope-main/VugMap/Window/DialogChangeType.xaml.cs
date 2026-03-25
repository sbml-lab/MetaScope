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
using VugMap.Utility.Error;

namespace VugMap.Window
{	
	public partial class DialogChangeType : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogChangeType( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();

			m_tbToSet.Focus();
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
			m_tbCurrent.Text				= m_pnlLane.DataTypeSelected.Type;
			m_tbToSet.Text					= m_pnlLane.DataTypeSelected.Type;
		}
		
		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( m_tbToSet.Text == "" )
			{
				ErrorMessage.ShowErrorEmptyFeature();				
			}
					
			string			strType			= m_tbToSet.Text;
			m_pnlLane.DoTypeChange( strType );

			MainWindow		mw				= MainWindow.GetMainWindow();
			Debug.Assert( mw != null );

			DocMap			doc				= mw.m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			doc.DoFileUpdate();
			Close();
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbToSet.Focus();
		}
	}
}
