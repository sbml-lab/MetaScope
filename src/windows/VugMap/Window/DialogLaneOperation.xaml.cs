using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Interop;

using VugMap.Utility;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;
using VugMap.Utility.Reader;

namespace VugMap.Window
{
	using			ListString						= List< string >;
	using			ListMapLane						= List< PnlMapLane >;

	public partial class DialogLaneOperation : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;
		private		DataFile						m_dfToOperate					= null;
		private		bool							m_bAverageByMedian				= true;
		private		bool							m_bAverageCompatible			= true;
		private		bool							m_bSumCompatible				= true;
		private		EMergeMethod					m_eMergeMethod					= EMergeMethod.MEDIAN;
		private		EFilterMethod					m_eFilterMethod					= EFilterMethod.OUTSIDE;
		private		double							m_dAdjustMultiply				= double.NaN;
		private		double							m_dAdjustShift					= double.NaN;
		private		int								m_nAdjustWidth					= 0;
		private		string							m_strAssignIdPattern			= null;

		public		delegate void DelegateDoThreadAverageEnd();
		public		delegate void DelegateDoProgressSet( int nCurrent, int nTotal );		

		public DialogLaneOperation( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
		}

		public void DoFillAverage()
		{
			m_tabAverage.IsEnabled			= true;
			m_tabAverage.Focus();
			
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				m_lbAverageLane.Items.Add( str );
			}
			
			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbAverageFileExisting.Items.Add( cbi );
			}

			( m_cbAverageFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillSummation()
		{
			m_tabSum.IsEnabled				= true;
			m_tabSum.Focus();
			
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				m_lbSumLane.Items.Add( str );
			}
			
			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbSumFileExisting.Items.Add( cbi );
			}	

			( m_cbSumFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillFilter()
		{
			m_tabFilter.IsEnabled			= true;
			m_tabFilter.Focus();
			
			ListString		lstFile			= new ListString();
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( pnl != pnlMap.LaneSelected.Last() )
				{
					string			str				= pnl.DataTypeSelected.Type;

					m_lbFilterLane.Items.Add( str );
				}

				if( lstFile.Contains( pnl.DataTypeSelected.DataFile.FileName ) == false )
					lstFile.Add( pnl.DataTypeSelected.DataFile.FileName );
			}

			{
				// Last one is the filter
				PnlMapLane		pnl				= pnlMap.LaneSelected.Last();
				
				string			str				= pnl.DataTypeSelected.Type;

				m_tbFilterBy.Text				= str;
			}
			
			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbFilterFileExisting.Items.Add( cbi );
			}

			( m_cbFilterFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillDifference()
		{
			m_tabDiff.IsEnabled				= true;
			m_tabDiff.Focus();
			
			ListString		lstFile			= new ListString();
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( pnl != pnlMap.LaneSelected.Last() )
				{
					string			str				= pnl.DataTypeSelected.Type;

					m_lbDiffLane.Items.Add( str );
				}

				if( lstFile.Contains( pnl.DataTypeSelected.DataFile.FileName ) == false )
					lstFile.Add( pnl.DataTypeSelected.DataFile.FileName );
			}

			{
				// Last one is the filter
				PnlMapLane		pnl				= pnlMap.LaneSelected.Last();
				
				string			str				= pnl.DataTypeSelected.Type;

				m_tbDiffBy.Text					= str;
			}

			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbDiffFileExisting.Items.Add( cbi );
			}

			( m_cbDiffFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillMerge()
		{
			m_tabMerge.IsEnabled			= true;
			m_tabMerge.Focus();
			
			ListString		lstFile			= new ListString();
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				m_lbMergeLane.Items.Add( str );

				if( lstFile.Contains( pnl.DataTypeSelected.DataFile.FileName ) == false )
					lstFile.Add( pnl.DataTypeSelected.DataFile.FileName );
			}

			ManagerData		md				= ManagerData.GetManager();
			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				string			str				= md.GetDataFile( i ).FileName;

				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbMergeFileExisting.Items.Add( cbi );
			}	

			( m_cbMergeFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected		
												= true;
		}

		public void DoFillAdjust()
		{
			m_tabAdjust.IsEnabled			= true;
			m_tabAdjust.Focus();
			
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				m_lbAdjustLane.Items.Add( str );
			}
		}

		public void DoFillAssignId()
		{
			m_tabAssignId.IsEnabled			= true;
			m_tabAssignId.Focus();
			
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				m_lbAssignIdLane.Items.Add( str );
			}
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

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( m_tabAverage.IsEnabled == true )
			{
				if( m_rbAverageFileNew.IsChecked == true && m_tbAverageFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();					
				}			
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbAverageFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbAverageFileExisting.SelectedItem as ComboBoxItem;						
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbAverageFileNew.IsChecked == true )
					{
						strFile			= m_tbAverageFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );						
						md.DoDataFileAdd( m_dfToOperate );
					}

					if( m_rbAverageCompatible.IsChecked == true )
						m_bAverageCompatible			= true;
					else if( m_rbAverageNotcompatible.IsChecked == true )
						m_bAverageCompatible			= false;
				
					if( m_rbAverageByAverage.IsChecked == true )
						m_bAverageByMedian				= false;
					else if( m_rbAverageByMedian.IsChecked == true )
						m_bAverageByMedian				= true;
					
					DoShowProgress();
						
					Thread			thd				= new Thread( new ThreadStart( DoThreadAverage ) );
					thd.Start();					
				}
			}	
			else if( m_tabMerge.IsEnabled == true )
			{
				if( m_rbMergeFileNew.IsChecked == true && m_tbMergeFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();					
				}			
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbMergeFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbMergeFileExisting.SelectedItem as ComboBoxItem;						
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbMergeFileNew.IsChecked == true )
					{
						strFile			= m_tbMergeFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );						
						md.DoDataFileAdd( m_dfToOperate );
					}																	
					
					if( m_rbMergeMedian.IsChecked == true )
						m_eMergeMethod		= EMergeMethod.MEDIAN;
					else if( m_rbMergeAverage.IsChecked == true )
						m_eMergeMethod		= EMergeMethod.AVERAGE;

					DoShowProgress();
						
					Thread			thd				= new Thread( new ThreadStart( DoThreadMerge ) );
					thd.Start();					
				}
			}	
			else if( m_tabFilter.IsEnabled == true )
			{
				if( m_rbFilterFileNew.IsChecked == true && m_tbFilterFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();					
				}			
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbFilterFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbFilterFileExisting.SelectedItem as ComboBoxItem;						
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbFilterFileNew.IsChecked == true )
					{
						strFile			= m_tbFilterFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );						
						md.DoDataFileAdd( m_dfToOperate );
					}	
					
					if( m_rbFilterOutside.IsChecked == true )
					{
						m_eFilterMethod					= EFilterMethod.OUTSIDE;
					}
					else
					{
						m_eFilterMethod					= EFilterMethod.INSIDE;
					}
					
					DoShowProgress();
						
					Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
					thd.Start();					
				}
			}
			else if( m_tabDiff.IsEnabled == true )
			{
				if( m_rbDiffFileNew.IsChecked == true && m_tbDiffFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();					
				}			
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbDiffFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbDiffFileExisting.SelectedItem as ComboBoxItem;						
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbDiffFileNew.IsChecked == true )
					{
						strFile			= m_tbDiffFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );						
						md.DoDataFileAdd( m_dfToOperate );
					}																	
					
					DoShowProgress();
						
					Thread			thd				= new Thread( new ThreadStart( DoThreadDiff ) );
					thd.Start();					
				}
			}
			else if( m_tabSum.IsEnabled == true )
			{
				if( m_rbSumFileNew.IsChecked == true && m_tbSumFileNew.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();					
				}			
				else
				{
					ManagerData		md				= ManagerData.GetManager();
					string			strFile			= null;

					if( m_rbSumFileExisting.IsChecked == true )
					{
						ComboBoxItem	cbi				= m_cbSumFileExisting.SelectedItem as ComboBoxItem;						
						strFile							= cbi.Content.ToString();

						m_dfToOperate	= md.GetDataFile( strFile );
					}
					else if( m_rbSumFileNew.IsChecked == true )
					{
						strFile			= m_tbSumFileNew.Text;

						if( md.GetDataFile( strFile ) != null )
						{
							ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
							return;
						}

						m_dfToOperate	= new DataFile( md, strFile );						
						md.DoDataFileAdd( m_dfToOperate );
					}

					if( m_rbSumCompatible.IsChecked == true )
						m_bSumCompatible				= true;
					else if( m_rbSumNotcompatible.IsChecked == true )
						m_bSumCompatible				= false;
				
					DoShowProgress();
						
					Thread			thd				= new Thread( new ThreadStart( DoThreadSum ) );
					thd.Start();					
				}
			}
			else if( m_tabAdjust.IsEnabled == true )
			{				
				if( m_rbAdjustMultiply.IsChecked == true )
				{
					bool			b				= double.TryParse( m_tbAdjustMultiply.Text, out m_dAdjustMultiply );
					if( b == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbAdjustMultiply.Text );
						return;
					}
				}
				else if( m_rbAdjustShift.IsChecked == true )
				{
					bool			b				= double.TryParse( m_tbAdjustShift.Text, out m_dAdjustShift );
					if( b == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbAdjustShift.Text );
						return;
					}
				}
				else
				{
					bool			b				= int.TryParse( m_tbAdjustWidth.Text, out m_nAdjustWidth );
					if( b == false || m_nAdjustWidth <= 0 )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbAdjustWidth.Text );
						return;
					}
				}

				Thread			thd				= new Thread( new ThreadStart( DoThreadAdjust ) );
				thd.Start();									
			}
			else if( m_tabAssignId.IsEnabled == true )
			{
				if( m_tbAssignIdPattern.Text == "" )
				{
					ErrorMessage.ShowErrorFileNotSelected();					
				}
				else
				{
					ManagerData		md				= ManagerData.GetManager();					
					
					m_strAssignIdPattern			= m_tbAssignIdPattern.Text;					
					
					DoShowProgress();
						
					Thread			thd				= new Thread( new ThreadStart( DoThreadAssignId ) );
					thd.Start();					
				}
			}
		}

		private void DoThreadDiff()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			if( DoThreadAverageCheckDataLane( pnlMap ) == false )
			{
				ErrorMessage.ShowErrorAverageNotPossible();
				Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEndError ), DispatcherPriority.Normal, null );
			}
			else
			{
				PnlMapLane		pnlFilter		= pnlMap.LaneSelected.Last();
				ListMapLane		lstLane			= new ListMapLane();
				lstLane.AddRange( pnlMap.LaneSelected );
				lstLane.Remove( pnlFilter );			
			
				m_dfToOperate.AddDataTypeByDiff( lstLane, pnlFilter, new DelegateDoProgressSet( DoProgressSet ), this );
				m_dfToOperate.BuildIndex();
				m_dfToOperate.IsEdited		= true;

				// at the end
				Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );			
			}			
		}

		private void DoThreadFilter()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			PnlMapLane		pnlFilter		= pnlMap.LaneSelected.Last();
			ListMapLane		lstLane			= new ListMapLane();
			lstLane.AddRange( pnlMap.LaneSelected );
			lstLane.Remove( pnlFilter );			
			
			m_dfToOperate.AddDataTypeByFilter( lstLane, pnlFilter, m_eFilterMethod, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );	
		}

		private void DoThreadMerge()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			m_dfToOperate.AddDataTypeByMerge( pnlMap.LaneSelected, m_eMergeMethod ,new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );							
		}

		private void DoThreadSum()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			if( m_bSumCompatible == true && DoThreadAverageCheckDataLane( pnlMap ) == false )
			{
				ErrorMessage.ShowErrorAverageNotPossible();
				Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEndError ), DispatcherPriority.Normal, null );
			}
			else
			{
				m_dfToOperate.AddDataTypeBySum( pnlMap.LaneSelected, new DelegateDoProgressSet( DoProgressSet ), this );
				m_dfToOperate.BuildIndex();
				m_dfToOperate.IsEdited		= true;

				// at the end
				Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );				
			}	
		}

		private void DoThreadAverage()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			if( m_bAverageCompatible == true && DoThreadAverageCheckDataLane( pnlMap ) == false )
			{
				ErrorMessage.ShowErrorAverageNotPossible();
				Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEndError ), DispatcherPriority.Normal, null );
			}
			else
			{
				m_dfToOperate.AddDataTypeByAverage( pnlMap.LaneSelected, new DelegateDoProgressSet( DoProgressSet ), this, m_bAverageByMedian );
				m_dfToOperate.BuildIndex();
				m_dfToOperate.IsEdited		= true;

				// at the end
				Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );				
			}			
		}

		private void DoThreadAdjust()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pml in pnlMap.LaneSelected )
			{
				pml.DataTypeSelected.DoAdjust( m_dAdjustMultiply, m_dAdjustShift, m_nAdjustWidth, new DelegateDoProgressSet( DoProgressSet ), this );
				pml.DataTypeSelected.DataFile.IsEdited
												= true;
			}

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );							
		}

		private void DoThreadAssignId()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pml in pnlMap.LaneSelected )
			{
				pml.DataTypeSelected.DoAssignId( m_strAssignIdPattern, new DelegateDoProgressSet( DoProgressSet ), this );
				pml.DataTypeSelected.DataFile.IsEdited
												= true;
			}

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadAverageEnd( DoThreadEnd ), DispatcherPriority.Normal, null );							
		}

		private bool DoThreadAverageCheckDataLane( PnlMap pnlMap )
		{
			DataType		dtFirst			= pnlMap.LaneSelected[ 0 ].DataTypeSelected;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( dtFirst == pnl.DataTypeSelected )
				{
					continue;
				}
				
				if( dtFirst.DoCheckCompatible( pnl.DataTypeSelected ) == false )
				{
					return false;
				}
			}

			return true;
		}

		private void DoProgressSet( int nCurrent, int nTotal )
		{
			m_pgbProgress.Value				= nCurrent;
			m_pgbProgress.Minimum			= 0;
			m_pgbProgress.Maximum			= nTotal;
		}

		private void DoThreadEndError()
		{
			DialogResult					= false;
			Close();
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

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnSumFileNewClick( object obj, RoutedEventArgs ea )
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

				m_tbSumFileNew.Text				= strFile;
			}
		}

		private void OnDiffFileNewClick( object obj, RoutedEventArgs ea )
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

				m_tbDiffFileNew.Text			= strFile;
			}
		}

		private void OnFilteFileNewClick( object obj, RoutedEventArgs ea )
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

				m_tbFilterFileNew.Text			= strFile;
			}
		}

		private void OnAverageFileNewClick( object obj, RoutedEventArgs ea )
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

				m_tbAverageFileNew.Text				= strFile;
			}
		}

		private void OnMergeFileNewClick( object obj, RoutedEventArgs ea )
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

				m_tbMergeFileNew.Text				= strFile;
			}
		}		
	}

	public enum EFilterMethod
	{
		OUTSIDE						= 0x0,
		INSIDE,
	}

	public enum EMergeMethod
	{
		MEDIAN						= 0x0,
		AVERAGE,
	}
}
