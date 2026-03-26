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

	public partial class DialogLaneOperation : Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;
		private		MainWindow						m_mw							= null;
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

		public DialogLaneOperation( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;
			m_mw			= MainWindow.GetMainWindow( pnlLane );

			InitializeComponent();
		}

		public void DoFillAverage()
		{
			m_tabAverage.IsEnabled			= true;
			m_tbcOperation.SelectedItem = m_tabAverage;

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

			if( m_cbAverageFileExisting.Items.Count > 0 )
				( m_cbAverageFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillSummation()
		{
			m_tabSum.IsVisible				= true;
			m_tbcOperation.SelectedItem = m_tabSum;

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

			if( m_cbSumFileExisting.Items.Count > 0 )
				( m_cbSumFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillFilter()
		{
			m_tabFilter.IsEnabled			= true;
			m_tbcOperation.SelectedItem = m_tabFilter;

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

			if( m_cbFilterFileExisting.Items.Count > 0 )
				( m_cbFilterFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillDifference()
		{
			m_tabDiff.IsVisible				= true;
			m_tbcOperation.SelectedItem = m_tabDiff;

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

			if( m_cbDiffFileExisting.Items.Count > 0 )
				( m_cbDiffFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillMerge()
		{
			m_tabMerge.IsEnabled			= true;
			m_tbcOperation.SelectedItem = m_tabMerge;

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

			if( m_cbMergeFileExisting.Items.Count > 0 )
				( m_cbMergeFileExisting.Items[ 0 ] as ComboBoxItem ).IsSelected
													= true;
		}

		public void DoFillAdjust()
		{
			m_tabAdjust.IsEnabled			= true;
			m_tbcOperation.SelectedItem = m_tabAdjust;

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
			m_tbcOperation.SelectedItem = m_tabAssignId;

			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				string			str				= pnl.DataTypeSelected.Type;

				m_lbAssignIdLane.Items.Add( str );
			}
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( m_tabAverage == m_tbcOperation.SelectedItem )
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
			else if( m_tabMerge == m_tbcOperation.SelectedItem )
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
			else if( m_tabFilter == m_tbcOperation.SelectedItem )
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
			else if( m_tabDiff == m_tbcOperation.SelectedItem )
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
			else if( m_tabSum == m_tbcOperation.SelectedItem )
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
			else if( m_tabAdjust == m_tbcOperation.SelectedItem )
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
			else if( m_tabAssignId == m_tbcOperation.SelectedItem )
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
				Dispatcher.UIThread.Post( DoThreadEndError );
			}
			else
			{
				PnlMapLane		pnlFilter		= pnlMap.LaneSelected.Last();
				ListMapLane		lstLane			= new ListMapLane();
				lstLane.AddRange( pnlMap.LaneSelected );
				lstLane.Remove( pnlFilter );

				m_dfToOperate.AddDataTypeByDiff( lstLane.ToList(), pnlFilter, new Models.DelegateDoProgressSet( DoProgressSet ), this );
				m_dfToOperate.BuildIndex();
				m_dfToOperate.IsEdited		= true;

				// at the end
				Dispatcher.UIThread.Post( DoThreadEnd );
			}
		}

		private void DoThreadFilter()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			PnlMapLane		pnlFilter		= pnlMap.LaneSelected.Last();
			ListMapLane		lstLane			= new ListMapLane();
			lstLane.AddRange( pnlMap.LaneSelected );
			lstLane.Remove( pnlFilter );

			m_dfToOperate.AddDataTypeByFilter( lstLane.ToList(), pnlFilter, m_eFilterMethod, new Models.DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
		}

		private void DoThreadMerge()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			m_dfToOperate.AddDataTypeByMerge( pnlMap.LaneSelected.ToList(), m_eMergeMethod, new Models.DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
		}

		private void DoThreadSum()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			if( m_bSumCompatible == true && DoThreadAverageCheckDataLane( pnlMap ) == false )
			{
				ErrorMessage.ShowErrorAverageNotPossible();
				Dispatcher.UIThread.Post( DoThreadEndError );
			}
			else
			{
				m_dfToOperate.AddDataTypeBySum( pnlMap.LaneSelected.ToList(), new Models.DelegateDoProgressSet( DoProgressSet ), this );
				m_dfToOperate.BuildIndex();
				m_dfToOperate.IsEdited		= true;

				// at the end
				Dispatcher.UIThread.Post( DoThreadEnd );
			}
		}

		private void DoThreadAverage()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			if( m_bAverageCompatible == true && DoThreadAverageCheckDataLane( pnlMap ) == false )
			{
				ErrorMessage.ShowErrorAverageNotPossible();
				Dispatcher.UIThread.Post( DoThreadEndError );
			}
			else
			{
				m_dfToOperate.AddDataTypeByAverage( pnlMap.LaneSelected.ToList(), new Models.DelegateDoProgressSet( DoProgressSet ), this, m_bAverageByMedian );
				m_dfToOperate.BuildIndex();
				m_dfToOperate.IsEdited		= true;

				// at the end
				Dispatcher.UIThread.Post( DoThreadEnd );
			}
		}

		private void DoThreadAdjust()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pml in pnlMap.LaneSelected )
			{
				pml.DataTypeSelected.DoAdjust( m_dAdjustMultiply, m_dAdjustShift, m_nAdjustWidth, new Models.DelegateDoProgressSet( DoProgressSet ), this );
				pml.DataTypeSelected.DataFile.IsEdited
												= true;
			}

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
		}

		private void DoThreadAssignId()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pml in pnlMap.LaneSelected )
			{
				pml.DataTypeSelected.DoAssignId( m_strAssignIdPattern, new Models.DelegateDoProgressSet( DoProgressSet ), this );
				pml.DataTypeSelected.DataFile.IsEdited
												= true;
			}

			// at the end
			Dispatcher.UIThread.Post( DoThreadEnd );
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
			Dispatcher.UIThread.Post( () =>
			{
				m_pgbProgress.Value				= nCurrent;
				m_pgbProgress.Minimum			= 0;
				m_pgbProgress.Maximum			= nTotal;
			} );
		}

		private void DoThreadEndError()
		{
			Close( false );
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

		private void DoShowButton()
		{
			m_spButton.IsEnabled			= true;
			m_grdProgress.IsVisible			= false;

			m_btnOk.IsEnabled				= true;
			m_btnCancel.IsEnabled			= true;
		}

		private void DoShowProgress()
		{
			m_spButton.IsVisible			= false;
			m_grdProgress.IsEnabled			= true;

			m_btnOk.IsEnabled				= false;
			m_btnCancel.IsEnabled			= false;
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
		}

		private async void OnSumFileNewClick( object obj, RoutedEventArgs ea )
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
				m_tbSumFileNew.Text				= dlg.Path.LocalPath;
			}
		}

		private async void OnDiffFileNewClick( object obj, RoutedEventArgs ea )
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
				m_tbDiffFileNew.Text			= dlg.Path.LocalPath;
			}
		}

		private async void OnFilterFileNewClick( object obj, RoutedEventArgs ea )
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
				m_tbFilterFileNew.Text			= dlg.Path.LocalPath;
			}
		}

		private async void OnAverageFileNewClick( object obj, RoutedEventArgs ea )
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
				m_tbAverageFileNew.Text			= dlg.Path.LocalPath;
			}
		}

		private async void OnMergeFileNewClick( object obj, RoutedEventArgs ea )
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
				m_tbMergeFileNew.Text			= dlg.Path.LocalPath;
			}
		}
	}
}
