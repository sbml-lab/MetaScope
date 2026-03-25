using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using VugMap.Utility;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;
using VugMap.Utility.Reader;

namespace VugMap.Window
{
	using			ListString						= List< string >;
	using			ListMapLane						= List< PnlMapLane >;
	
	public partial class DialogIntegrationOperation : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;
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
		public		delegate void DelegateDoProgressSet( int nCurrent, int nTotal );		

		public DialogIntegrationOperation( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
		}

		public void DoFillPorf()
		{
			m_tabPorf.IsEnabled				= true;
			m_tabPorf.Focus();
			
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

			( m_cbPorfStart.Items[ 0 ] as ComboBoxItem ).IsSelected	
												= true;
			( m_cbPorfStop.Items[ 1 ] as ComboBoxItem ).IsSelected		
												= true;
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

			( m_cbPorfFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillRts()
		{
			m_tabRts.IsEnabled				= true;
			m_tabRts.Focus();
			
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

			( m_cbRtsRbr.Items[ 0 ] as ComboBoxItem ).IsSelected	= true;
			( m_cbRtsTd.Items[ 1 ] as ComboBoxItem ).IsSelected		= true;
			
			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbRtsFileExisting.Items.Add( cbi );
			}	

			( m_cbRtsFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillTu()
		{
			m_tabTu.IsEnabled				= true;
			m_tabTu.Focus();
			
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

			( m_cbTuTss.Items[ 0 ] as ComboBoxItem ).IsSelected	= true;
			( m_cbTuRts.Items[ 1 ] as ComboBoxItem ).IsSelected	= true;
			( m_cbTuPorf.Items[ 2 ] as ComboBoxItem ).IsSelected= true;
			
			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbTuFileExisting.Items.Add( cbi );
			}	

			( m_cbTuFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillTrn()
		{
		}

		private void DoProgressSet( int nCurrent, int nTotal )
		{
			m_pgbProgress.Value				= nCurrent;
			m_pgbProgress.Minimum			= 0;
			m_pgbProgress.Maximum			= nTotal;
		}

		private void DoThreadEnd()
		{
			DialogResult					= true;
			Close();

			MainWindow		mw				= MainWindow.GetMainWindow();
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

			m_dfToOperate.AddDataTypeByIntegrationPorf( m_dtPorfStart, m_dtPorfStop, m_dtPorfProteome, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEnd ), DispatcherPriority.Normal, null );			
		}

		private void DoThreadRts()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			m_dfToOperate.AddDataTypeByIntegrationRts( m_dtRtsRbr, m_dtRtsTd, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEnd ), DispatcherPriority.Normal, null );			
		}

		private void DoThreadTu()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			m_dfToOperate.AddDataTypeByIntegrationTu( m_dtTuTss, m_dtTuRts, m_dtTuPorf, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEnd ), DispatcherPriority.Normal, null );			
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( m_tabPorf.IsEnabled == true )
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
			else if( m_tabRts.IsEnabled == true )
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
			else if( m_tabTu.IsEnabled == true )
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
			Close();
		}

		private void DoShowButton()
		{
			m_spButton.Visibility			= Visibility.Visible;
			m_grdProgress.Visibility		= Visibility.Collapsed;

			m_btnOk.IsEnabled				= true;
			m_btnCancel.IsEnabled			= true;
		}

		private void DoShowProgress()
		{
			m_spButton.Visibility			= Visibility.Collapsed;
			m_grdProgress.Visibility		= Visibility.Visible;

			m_btnOk.IsEnabled				= false;
			m_btnCancel.IsEnabled			= false;
		}

		private void OnPorfFileNewClick( object obj, RoutedEventArgs ea )
		{
			Microsoft.Win32.SaveFileDialog
							dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Select a file";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;			
			dlg.DefaultExt					= ".gff";
			dlg.Filter						= "GFF file (.gff)|*.gff";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strFile			= dlg.FileName;				

				m_tbPorfFileNew.Text				= strFile;
			}
		}

		private void OnRtsFileNewClick( object obj, RoutedEventArgs ea )
		{
			Microsoft.Win32.SaveFileDialog
							dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Select a file";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;			
			dlg.DefaultExt					= ".gff";
			dlg.Filter						= "GFF file (.gff)|*.gff";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strFile			= dlg.FileName;				

				m_tbRtsFileNew.Text				= strFile;
			}
		}

		private void OnTuFileNewClick( object obj, RoutedEventArgs ea )
		{
			Microsoft.Win32.SaveFileDialog
							dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Select a file";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;			
			dlg.DefaultExt					= ".gff";
			dlg.Filter						= "GFF file (.gff)|*.gff";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strFile			= dlg.FileName;				

				m_tbTuFileNew.Text				= strFile;
			}
		}
	}
}
