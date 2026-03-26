using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

using MetaScope.Controls;
using MetaScope.Models;
using MetaScope.Services.Error;

namespace MetaScope.Views
{
	public partial class DialogChangeType : Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogChangeType( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
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
				return;
			}

			string			strType			= m_tbToSet.Text;
			m_pnlLane.DoTypeChange( strType );

			MainWindow		mw				= MainWindow.GetMainWindow( this );
			Debug.Assert( mw != null );

			DocMap			doc				= mw.DoDocumentActive();
			Debug.Assert( doc != null );

			doc.DoFileUpdate();
			this.Close( true );
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			this.Close( false );
		}

		protected override void OnOpened( EventArgs ea )
		{
			base.OnOpened( ea );

			m_tbToSet.Focus();
		}
	}
}
