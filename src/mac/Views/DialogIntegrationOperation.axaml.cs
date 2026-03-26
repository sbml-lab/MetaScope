using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using MetaScope.Controls;
using MetaScope.Models;
using MetaScope.Services;
using MetaScope.Services.Error;

namespace MetaScope.Views
{
	using			ListString						= List< string >;
	using			ListMapLane						= List< PnlMapLane >;

	public partial class DialogIntegrationOperation : Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;
		private		MainWindow						m_mw							= null;
		private		DataFile						m_dfToOperate					= null;
		private		DataType						m_dtPorfStart					= null;
		private		DataType						m_dtPorfStop					= null;
		private		DataType						m_dtPorfProteome				= null;
		private		DataType						m_dtTuTss						= null;
		private		DataType						m_dtTuRts						= null;
		private		DataType						m_dtTuPorf						= null;
		private		DataType						m_dtRtsRbr						= null;
		private		DataType						m_dtRtsTd						= null;

		public		delegate void DelegateDoThreadEnd();

		public DialogIntegrationOperation( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;
			m_mw			= MainWindow.GetMainWindow( pnlLane );

			InitializeComponent();
		}

		public void DoFillPorf()
		{
			m_tabPorf.IsEnabled				= true;
			m_tbcOperation.SelectedItem = m_tabPorf;

			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				ComboBoxItem	cbiStart		= new ComboBoxItem();
				ComboBoxItem	cbiStop			= new ComboBoxItem();
				ComboBoxItem	cbiProt			= new ComboBoxItem();

				cbiStart.Content				= str;
				cbiStop.Content					= str;
				cbiProt.Content					= str;

				m_cbPorfStart.Items.Add( cbiStart );
				m_cbPorfStop.Items.Add( cbiStop );
				m_cbPorfProteome.Items.Add( cbiProt );
			}

			if( m_cbPorfStart.Items.Count > 0 )
				( m_cbPorfStart.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
			if( m_cbPorfStop.Items.Count > 1 )
				( m_cbPorfStop.Items[ 1 ] as ComboBoxItem ).IsSelected
													= true;
			if( m_cbPorfProteome.Items.Count > 2 )
				( m_cbPorfProteome.Items[ 2 ] as ComboBoxItem ).IsSelected
													= true;

			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;

				m_cbPorfFileExisting.Items.Add( cbi );
			}

			if( m_cbPorfFileExisting.Items.Count > 0 )
				( m_cbPorfFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillRts()
		{
			m_tabRts.IsEnabled				= true;
			m_tbcOperation.SelectedItem = m_tabRts;

			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				ComboBoxItem	cbiRbr			= new ComboBoxItem();
				ComboBoxItem	cbiTd			= new ComboBoxItem();

				cbiRbr.Content					= str;
				cbiTd.Content					= str;

				m_cbRtsRbr.Items.Add( cbiRbr );
				m_cbRtsTd.Items.Add( cbiTd );
			}

			if( m_cbRtsRbr.Items.Count > 0 )
				( m_cbRtsRbr.Items[ 0 ] as ComboBoxItem ).IsSelected	= true;
			if( m_cbRtsTd.Items.Count > 1 )
				( m_cbRtsTd.Items[ 1 ] as ComboBoxItem ).IsSelected		= true;

			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;

				m_cbRtsFileExisting.Items.Add( cbi );
			}

			if( m_cbRtsFileExisting.Items.Count > 0 )
				( m_cbRtsFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillTu()
		{
			m_tabTu.IsEnabled				= true;
			m_tbcOperation.SelectedItem = m_tabTu;

			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				ComboBoxItem	cbiTss			= new ComboBoxItem();
				ComboBoxItem	cbiRts			= new ComboBoxItem();
				ComboBoxItem	cbiPorf			= new ComboBoxItem();

				cbiTss.Content					= str;
				cbiRts.Content					= str;
				cbiPorf.Content					= str;

				m_cbTuTss.Items.Add( cbiTss );
				m_cbTuRts.Items.Add( cbiRts );
				m_cbTuPorf.Items.Add( cbiPorf );
			}

			if( m_cbTuTss.Items.Count > 0 )
				( m_cbTuTss.Items[ 0 ] as ComboBoxItem ).IsSelected	= true;
			if( m_cbTuRts.Items.Count > 1 )
				( m_cbTuRts.Items[ 1 ] as ComboBoxItem ).IsSelected	= true;
			if( m_cbTuPorf.Items.Count > 2 )
				( m_cbTuPorf.Items[ 2 ] as ComboBoxItem ).IsSelected	= true;

			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;

				m_cbTuFileExisting.Items.Add( cbi );
			}

			if( m_cbTuFileExisting.Items.Count > 0 )
				( m_cbTuFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillTrn()
		{
		}

		private void DoProgressSet( int nCurrent, int nTotal )
		{
			Dispatcher.UIThread.Post( () =>
			{
				m_pgbProgress.Value				= nCurrent;
				m_pgbProgress.Minimum			= 0;
				m_pgbProgress.Maximum			= nTotal;
			} );
		}

		private void DoThreadEnd()
		{
			Close( true );

			MainWindow		mw				= m_mw;
			DocMap			dm				= mw.DoDocumentActive();

			if( m_dfToOperate != null )
			{
				dm.DoPanelLaneAdd( m_dfToOperate );
			}

			dm.DoUpdateView();

			mw.DoExplorerUpdate();
		}

		private void DoThreadPorf()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			m_dfToOperate.AddDataTypeByIntegrationPorf( m_dtPorfStart, m_dtPorfStop, m_dtPorfProteome, new Models.DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
		}

		private void DoThreadRts()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			m_dfToOperate.AddDataTypeByIntegrationRts( m_dtRtsRbr, m_dtRtsTd, new Models.DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
		}

		private void DoThreadTu()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			m_dfToOperate.AddDataTypeByIntegrationTu( m_dtTuTss, m_dtTuRts, m_dtTuPorf, new Models.DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( m_tabPorf == m_tbcOperation.SelectedItem )
			{
				if( m_rbPorfFileNew.IsChecked == true && m_tbPorfFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();
				}
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbPorfFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbPorfFileExisting.SelectedItem as ComboBoxItem;
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbPorfFileNew.IsChecked == true )
					{
						strFile			= m_tbPorfFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );
						md.DoDataFileAdd( m_dfToOperate );
					}

					string			strSeqId		= m_pnlLane.DataTypeSelected.SequenceId;

					string			strTypeStart	= ( m_cbPorfStart.SelectedItem as ComboBoxItem ).Content.ToString();
					string			strTypeStop		= ( m_cbPorfStop.SelectedItem as ComboBoxItem ).Content.ToString();
					string			strTypeProteome	= ( m_cbPorfProteome.SelectedItem as ComboBoxItem ).Content.ToString();

					m_dtPorfStart					= md.GetDataType( strSeqId, strTypeStart );
					m_dtPorfStop					= md.GetDataType( strSeqId, strTypeStop );
					m_dtPorfProteome				= md.GetDataType( strSeqId, strTypeProteome );

					DoShowProgress();

					Thread			thd				= new Thread( new ThreadStart( DoThreadPorf ) );
					thd.Start();
				}
			}
			else if( m_tabRts == m_tbcOperation.SelectedItem )
			{
				if( m_rbRtsFileNew.IsChecked == true && m_tbRtsFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();
				}
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbRtsFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbRtsFileExisting.SelectedItem as ComboBoxItem;
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbRtsFileNew.IsChecked == true )
					{
						strFile			= m_tbRtsFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );
						md.DoDataFileAdd( m_dfToOperate );
					}

					string			strSeqId		= m_pnlLane.DataTypeSelected.SequenceId;

					string			strTypeRbr		= ( m_cbRtsRbr.SelectedItem as ComboBoxItem ).Content.ToString();
					string			strTypeTd		= ( m_cbRtsTd.SelectedItem as ComboBoxItem ).Content.ToString();

					m_dtRtsRbr						= md.GetDataType( strSeqId, strTypeRbr );
					m_dtRtsTd						= md.GetDataType( strSeqId, strTypeTd );

					DoShowProgress();

					Thread			thd				= new Thread( new ThreadStart( DoThreadRts ) );
					thd.Start();
				}
			}
			else if( m_tabTu == m_tbcOperation.SelectedItem )
			{
				if( m_rbTuFileNew.IsChecked == true && m_tbTuFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();
				}
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbTuFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbTuFileExisting.SelectedItem as ComboBoxItem;
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbTuFileNew.IsChecked == true )
					{
						strFile			= m_tbTuFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );
						md.DoDataFileAdd( m_dfToOperate );
					}

					string			strSeqId		= m_pnlLane.DataTypeSelected.SequenceId;

					string			strTypeTss		= ( m_cbTuTss.SelectedItem as ComboBoxItem ).Content.ToString();
					string			strTypeRts		= ( m_cbTuRts.SelectedItem as ComboBoxItem ).Content.ToString();
					string			strTypePorf		= ( m_cbTuPorf.SelectedItem as ComboBoxItem ).Content.ToString();

					m_dtTuTss						= md.GetDataType( strSeqId, strTypeTss );
					m_dtTuRts						= md.GetDataType( strSeqId, strTypeRts );
					m_dtTuPorf						= md.GetDataType( strSeqId, strTypePorf );

					DoShowProgress();

					Thread			thd				= new Thread( new ThreadStart( DoThreadTu ) );
					thd.Start();
				}
			}
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
		}

		private void DoShowButton()
		{
			m_spButton.IsVisible			= true;
			m_grdProgress.IsVisible			= false;

			m_btnOk.IsEnabled				= true;
			m_btnCancel.IsEnabled			= true;
		}

		private void DoShowProgress()
		{
			m_spButton.IsVisible			= false;
			m_grdProgress.IsVisible			= true;

			m_btnOk.IsEnabled				= false;
			m_btnCancel.IsEnabled			= false;
		}

		private async void OnPorfFileNewClick( object obj, RoutedEventArgs ea )
		{
			var		dlg		= await StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
			{
				Title				= "Select a file",
				DefaultExtension	= "gff",
				FileTypeChoices		= new[]
				{
					new FilePickerFileType( "GFF file" )	{ Patterns = new[] { "*.gff" } }
				}
			} );

			if( dlg != null )
			{
				m_tbPorfFileNew.Text			= dlg.Path.LocalPath;
			}
		}

		private async void OnRtsFileNewClick( object obj, RoutedEventArgs ea )
		{
			var		dlg		= await StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
			{
				Title				= "Select a file",
				DefaultExtension	= "gff",
				FileTypeChoices		= new[]
				{
					new FilePickerFileType( "GFF file" )	{ Patterns = new[] { "*.gff" } }
				}
			} );

			if( dlg != null )
			{
				m_tbRtsFileNew.Text				= dlg.Path.LocalPath;
			}
		}

		private async void OnTuFileNewClick( object obj, RoutedEventArgs ea )
		{
			var		dlg		= await StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
			{
				Title				= "Select a file",
				DefaultExtension	= "gff",
				FileTypeChoices		= new[]
				{
					new FilePickerFileType( "GFF file" )	{ Patterns = new[] { "*.gff" } }
				}
			} );

			if( dlg != null )
			{
				m_tbTuFileNew.Text				= dlg.Path.LocalPath;
			}
		}
	}
}
