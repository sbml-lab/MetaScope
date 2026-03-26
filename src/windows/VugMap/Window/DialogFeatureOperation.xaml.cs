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
using VugMap.Utility.Command;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;
using VugMap.Utility.Reader;

namespace VugMap.Window
{	
	using			ListFeature						= List< DataFeature >;
	using			ListString						= List< string >;
	using			HashFeature						= HashSet< DataFeature >;

	public partial class DialogFeatureOperation : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;
		private		double							m_dFilterOver					= double.NaN;
		private		double							m_dFilterUnder					= double.NaN;
		private		double							m_dFilterBwMin					= double.NaN;
		private		double							m_dFilterBwMax					= double.NaN;
		private		double							m_dFilterPercent				= double.NaN;
		private		double							m_dFilterTop					= double.NaN;
		private		double							m_dFilterTopPercent				= double.NaN;
		private		int								m_nFilterSlide					= 0;
		private		ListFeature						m_lstFilter						= null;
		private		DataFile						m_dfToOperate					= null;
		private		EMergeMethod					m_eMergeMethod					= EMergeMethod.MEDIAN;
		private		PnlMapLane						m_pnlMoveTo						= null;

		public		delegate void DelegateDoThreadEnd();
		public		delegate void DelegateDoProgressSet( int nCurrent, int nTotal );		

		public DialogFeatureOperation( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
		}

		private void DoProgressSet( int nCurrent, int nTotal )
		{
			m_pgbProgress.Value				= nCurrent;
			m_pgbProgress.Minimum			= 0;
			m_pgbProgress.Maximum			= nTotal;
		}

		public void DoFillFilter()
		{
			m_tabFilter.IsEnabled			= true;
			m_tabFilter.Focus();
			
			int				nCountFeature	= m_pnlLane.GetCountFeatureSelected();

			m_tbFilterFeatCount.Text		= nCountFeature.ToString();
		}

		public void DoFillMove()
		{
			m_tabMove.IsEnabled				= true;
			m_tabMove.Focus();
			
			int				nCountFeature	= m_pnlLane.GetCountFeatureSelected();

			m_tbMoveFeatCount.Text			= nCountFeature.ToString();

			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pml in pnlMap.LaneList )
			{
				ComboBoxItem	cbi				= new ComboBoxItem();
				string			str				= pml.DataTypeSelected.Type;

				cbi.Content						= str;

				m_cbMoveLaneTo.Items.Add( cbi );

				if( pml == pnlMap.LaneList.First() )
				{
					cbi.IsSelected					= true;
				}
			}
		}

		public void DoFillCopy()
		{
			m_tabCopy.IsEnabled				= true;
			m_tabCopy.Focus();
			
			int				nCountFeature	= m_pnlLane.GetCountFeatureSelected();

			m_tbCopyFeatCount.Text			= nCountFeature.ToString();

			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;

			foreach( PnlMapLane pml in pnlMap.LaneList )
			{
				ComboBoxItem	cbi				= new ComboBoxItem();
				string			str				= pml.DataTypeSelected.Type;

				cbi.Content						= str;

				m_cbCopyLaneTo.Items.Add( cbi );

				if( pml == pnlMap.LaneList.First() )
				{
					cbi.IsSelected					= true;
				}
			}
		}

		public void DoFillMerge()
		{
			m_tabMerge.IsEnabled			= true;
			m_tabMerge.Focus();
			
			ListString		lstFile			= new ListString();
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			foreach( PnlMapLane pnl in pnlMap.ListLaneEditable )
			{
				if( pnl.ListFeatureSelected.Count > 0 )
				{
					string			str				= pnl.DataTypeSelected.Type;

					m_lbMergeLane.Items.Add( str );

					if( lstFile.Contains( pnl.DataTypeSelected.DataFile.FileName ) == false )
						lstFile.Add( pnl.DataTypeSelected.DataFile.FileName );
				}
			}
			
			m_rbMergeFileNew.IsChecked		= true;

			foreach( string str in lstFile )
			{
				ComboBoxItem	cbi				= new ComboBoxItem();

				cbi.Content						= str;				
				
				m_cbMergeFileExisting.Items.Add( cbi );
			}	
	
			ComboBoxItem	cbiFirst		= m_cbMergeFileExisting.Items[ 0 ] as ComboBoxItem;
			cbiFirst.IsSelected				= true;
		}

		private void DoThreadEndError()
		{
			DialogResult					= false;
			Close();
		}

		private void DoThreadEndCopy()
		{
			DialogResult					= true;

			MainWindow		mw				= MainWindow.GetMainWindow();
			DocMap			dm				= mw.DoDocumentActive();

			dm.DoPanelLaneAdd( m_dfToOperate );
			dm.DoUpdateView();

			mw.DoExplorerUpdate();

			mw.DoAutoSaveImmediate();

			Close();
		}

		private void DoThreadEndMove()
		{
			DialogResult					= true;

			MainWindow		mw				= MainWindow.GetMainWindow();
			DocMap			dm				= mw.DoDocumentActive();

			dm.DoPanelLaneAdd( m_dfToOperate );
			dm.DoUpdateView();

			mw.DoExplorerUpdate();

			mw.DoAutoSaveImmediate();

			Close();
		}

		private void DoThreadEndMerge()
		{
			DialogResult					= true;

			MainWindow		mw				= MainWindow.GetMainWindow();
			DocMap			dm				= mw.DoDocumentActive();

			dm.DoPanelLaneAdd( m_dfToOperate );
			dm.DoUpdateView();

			mw.DoExplorerUpdate();

			mw.DoAutoSaveImmediate();

			Close();
		}

		private void DoThreadEndFilter()
		{
			DialogResult					= true;

			MainWindow		mw				= MainWindow.GetMainWindow();
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandDelete	cmd				= me.MakeCommandDelete();

			cmd.DoFeatureAdd( m_pnlLane, m_lstFilter, null );

			m_pnlLane.ListFeatureSelected.Clear();

			m_pnlLane.DoLayoutUpdate();

			mw.DoExplorerUpdate();
			mw.DoEditUpdate();

			mw.DoAutoSaveImmediate();

			Close();
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( m_tabFilter.IsEnabled == true )
			{	
				if( m_rbFilterOver.IsChecked == true )
				{					
					bool			b				= double.TryParse( m_tbScoreOver.Text, out m_dFilterOver );

					if( b == true )
					{
						DoShowProgress();

						Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
						thd.Start();
					}
					else
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbScoreOver.Text );
						return;
					}					
				}
				else if( m_rbFilterUnder.IsChecked == true )
				{
					bool			b				= double.TryParse( m_tbScoreUnder.Text, out m_dFilterUnder );

					if( b == true )
					{
						DoShowProgress();

						Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
						thd.Start();
					}
					else
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbScoreUnder.Text );
						return;
					}					
				}
				else if( m_rbFilterBetween.IsChecked == true )
				{
					bool			bMin			= double.TryParse( m_tbScoreBwMin.Text, out m_dFilterBwMin );
					bool			bMax			= double.TryParse( m_tbScoreBwMax.Text, out m_dFilterBwMax );

					if( bMin == true && bMax == true )
					{
						DoShowProgress();

						Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
						thd.Start();
					}
					else if( bMin == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbScoreBwMin.Text );
						return;
					}
					else
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbScoreBwMax.Text );
						return;
					}
				}
				else if( m_rbFilterPercent.IsChecked == true )
				{
					bool			bPercent		= double.TryParse( m_tbFilterPercent.Text, out m_dFilterPercent );					
					if( bPercent == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbFilterPercent.Text );
						return;						
					}					

					DoShowProgress();

					Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
					thd.Start();
				}
				else if( m_rbSelectTop.IsChecked == true )
				{
					int				nTop			= 0;
					bool			bTop			= int.TryParse( m_tbSelectTop.Text, out nTop );					
					if( bTop == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbSelectTop.Text );
						return;						
					}

					bool			bSlide			= int.TryParse( m_tbFilterSlide.Text, out m_nFilterSlide );
					if( bSlide == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbFilterSlide.Text );
						return;						
					}					

					m_dFilterTop					= nTop;

					DoShowProgress();

					Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
					thd.Start();
				}
				else if( m_rbSelectTopPercent.IsChecked == true )
				{
					bool			bTop			= double.TryParse( m_tbSelectTopPercent.Text, out m_dFilterTopPercent );					
					if( bTop == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbSelectTopPercent.Text );
						return;
					}					

					bool			bSlide			= int.TryParse( m_tbFilterSlide.Text, out m_nFilterSlide );
					if( bSlide == false )
					{
						ErrorMessage.ShowErrorScoreInvalid( m_tbFilterSlide.Text );
						return;						
					}

					DoShowProgress();

					Thread			thd				= new Thread( new ThreadStart( DoThreadFilter ) );
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
			else if( m_tabMove.IsEnabled == true )
			{
				ComboBoxItem	cbi				= m_cbMoveLaneTo.SelectedItem as ComboBoxItem;
				string			strType			= cbi.Content.ToString();

				PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
				m_pnlMoveTo						= pnlMap.DoLaneFind( strType );

				m_dfToOperate					= m_pnlMoveTo.DataTypeSelected.DataFile;

				DoShowProgress();
						
				Thread			thd				= new Thread( new ThreadStart( DoThreadMove ) );
				thd.Start();				
			}
			else if( m_tabCopy.IsEnabled == true )
			{
				ComboBoxItem	cbi				= m_cbCopyLaneTo.SelectedItem as ComboBoxItem;
				string			strType			= cbi.Content.ToString();

				PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
				m_pnlMoveTo						= pnlMap.DoLaneFind( strType );

				m_dfToOperate					= m_pnlMoveTo.DataTypeSelected.DataFile;

				DoShowProgress();
						
				Thread			thd				= new Thread( new ThreadStart( DoThreadCopy ) );
				thd.Start();				
			}
		}

		private void DoThreadCopy()
		{
			m_dfToOperate.AddDataTypeByCopyFeature( m_pnlLane, m_pnlMoveTo, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEndCopy ), DispatcherPriority.Normal, null );	
		}

		private void DoThreadMove()
		{
			m_dfToOperate.AddDataTypeByMoveFeature( m_pnlLane, m_pnlMoveTo, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEndMove ), DispatcherPriority.Normal, null );	
		}

		private void DoThreadMerge()
		{
			PnlMap			pnlMap			= m_pnlLane.Parent as PnlMap;
			
			m_dfToOperate.AddDataTypeByMergeFeature( pnlMap.ListLaneEditable, m_eMergeMethod, new DelegateDoProgressSet( DoProgressSet ), this );
			m_dfToOperate.BuildIndex();
			m_dfToOperate.IsEdited		= true;

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEndMerge ), DispatcherPriority.Normal, null );	
		}

		private void DoThreadFilter()
		{
			DelegateDoProgressSet	dlg		= new DelegateDoProgressSet( DoProgressSet );

			if( double.IsNaN( m_dFilterOver ) == false )
			{
				m_lstFilter						= new ListFeature();

				int				nCount			= 1;
				int				nTotal			= m_pnlLane.ListFeatureSelected.Count;

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				foreach( DataFeature df in m_pnlLane.ListFeatureSelected )
				{
					if( df.Score >= m_dFilterOver )
					{
						m_lstFilter.Add( df );
					}

					sw.Stop();

					if( sw.ElapsedMilliseconds >= 100 )
					{
						Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, nCount, nTotal );					
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}
				
				m_pnlLane.DataTypeSelected.DoFeatureRemove( m_lstFilter );

				Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, 100, 100 );		
			}
			else if( double.IsNaN( m_dFilterUnder ) == false )
			{
				m_lstFilter						= new ListFeature();

				int				nCount			= 1;
				int				nTotal			= m_pnlLane.ListFeatureSelected.Count;

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				foreach( DataFeature df in m_pnlLane.ListFeatureSelected )
				{
					if( df.Score <= m_dFilterUnder )
					{
						m_lstFilter.Add( df );
					}

					if( sw.ElapsedMilliseconds >= 100 )
					{
						Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, nCount, nTotal );					
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}

				m_pnlLane.DataTypeSelected.DoFeatureRemove( m_lstFilter );

				Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, 100, 100 );		
			}
			else if( double.IsNaN( m_dFilterBwMin ) == false && double.IsNaN( m_dFilterBwMax ) == false )
			{
				m_lstFilter						= new ListFeature();

				int				nCount			= 1;
				int				nTotal			= m_pnlLane.ListFeatureSelected.Count;

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				foreach( DataFeature df in m_pnlLane.ListFeatureSelected )
				{
					if( df.Score <= m_dFilterBwMax && df.Score >= m_dFilterBwMin )
					{
						m_lstFilter.Add( df );
					}

					if( sw.ElapsedMilliseconds >= 100 )
					{
						Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, nCount, nTotal );					
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}

				m_pnlLane.DataTypeSelected.DoFeatureRemove( m_lstFilter );

				Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, 100, 100 );		
			}
			else if( double.IsNaN( m_dFilterPercent ) == false )
			{
				int				nCount			= 1;
				int				nTotal			= m_pnlLane.ListFeatureSelected.Count;

				ListFeature		lst				= new ListFeature();
				lst.AddRange( m_pnlLane.ListFeatureSelected );
				lst.Sort( new ComparerFeatureScore() );

				double			dMaximum		= lst[ 0 ].ScoreReal;
				dMaximum						= dMaximum * m_dFilterPercent / 100;
								
				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				for( int i = 0; i < lst.Count; i++ )
				{
					if( lst[ i ].ScoreReal < dMaximum )
					{
						m_lstFilter.Add( lst[ i ] );
					}

					if( sw.ElapsedMilliseconds >= 100 )
					{
						Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, nCount, nTotal );					
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}				

				m_pnlLane.DataTypeSelected.DoFeatureRemove( m_lstFilter );

				Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, 100, 100 );	
			}
			else if( double.IsNaN( m_dFilterTop ) == false )
			{
				m_lstFilter						= new ListFeature( m_pnlLane.ListFeatureSelected.Count );

				int				nStart			= m_pnlLane.ListFeatureSelected.First().Start;
				int				nEnd			= m_pnlLane.ListFeatureSelected.Last().Start;
				int				nWidth			= nEnd - nStart + 1;

				if( m_nFilterSlide == 0 )
				{
					m_nFilterSlide					= nWidth;
				}

				HashFeature		hshFeature		= new HashFeature();		

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				int				nCount			= 1;
				int				nTotal			= m_pnlLane.ListFeatureSelected.Count;
				ListFeature		lst				= new ListFeature();				

				for( int i = 0; i < nTotal; i++ )
				{
					DataFeature		dfFirst			= m_pnlLane.ListFeatureSelected[ i ];
					
					lst.Clear();
					
					for( int j = i; j < nTotal; j++ )
					{
						DataFeature		df				= m_pnlLane.ListFeatureSelected[ j ];

						if( df.Start >= dfFirst.Start && df.End <= dfFirst.End + m_nFilterSlide )
						{
							lst.Add( df );
						}

						if( df.End > dfFirst.End + m_nFilterSlide )
							break;
					}
					
					lst.Sort( new ComparerFeatureScore() );

					int				nFilterTop		= ( int ) m_dFilterTop;
					nFilterTop						= Math.Min( nFilterTop, lst.Count );

					for( int j = nFilterTop; j < lst.Count; j++ )
					{
						if( hshFeature.Contains( lst[ j ] ) == false )
						{
							hshFeature.Add( lst[ j ] );
						}
					}

					if( sw.ElapsedMilliseconds >= 100 )
					{
						Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, nCount, nTotal );					
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}
				
				sw.Stop();

				m_lstFilter.AddRange( hshFeature );

				m_pnlLane.DataTypeSelected.DoFeatureRemove( m_lstFilter );

				Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, 100, 100 );	
			}
			else if( double.IsNaN( m_dFilterTopPercent ) == false )
			{
				m_lstFilter						= new ListFeature( m_pnlLane.ListFeatureSelected.Count );

				int				nStart			= m_pnlLane.ListFeatureSelected.First().Start;
				int				nEnd			= m_pnlLane.ListFeatureSelected.Last().Start;
				int				nWidth			= nEnd - nStart + 1;

				if( m_nFilterSlide == 0 )
				{
					m_nFilterSlide					= nWidth;
				}

				HashFeature		hshFeature		= new HashFeature();		

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				int				nCount			= 1;
				int				nTotal			= m_pnlLane.ListFeatureSelected.Count;
				ListFeature		lst				= new ListFeature();				

				for( int i = 0; i < nTotal; i++ )
				{
					DataFeature		dfFirst			= m_pnlLane.ListFeatureSelected[ i ];
					
					lst.Clear();
					
					for( int j = i; j < nTotal; j++ )
					{
						DataFeature		df				= m_pnlLane.ListFeatureSelected[ j ];

						if( df.Start >= dfFirst.Start && df.End <= dfFirst.End + m_nFilterSlide )
						{
							lst.Add( df );
						}

						if( df.End > dfFirst.End + m_nFilterSlide )
							break;
					}
					
					lst.Sort( new ComparerFeatureScore() );
										
					int				nTop			= ( int ) ( lst.Count * m_dFilterTopPercent / 100 );					

					for( int j = nTop; j < lst.Count; j++ )
					{
						if( hshFeature.Contains( lst[ j ] ) == false )
						{
							hshFeature.Add( lst[ j ] );
						}
					}

					if( sw.ElapsedMilliseconds >= 100 )
					{
						Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, nCount, nTotal );					
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}
				
				sw.Stop();

				m_lstFilter.AddRange( hshFeature );

				m_pnlLane.DataTypeSelected.DoFeatureRemove( m_lstFilter );

				Dispatcher.BeginInvoke( dlg, DispatcherPriority.Normal, 100, 100 );	
			}

			// at the end
			Dispatcher.BeginInvoke( new DelegateDoThreadEnd( DoThreadEndFilter ), DispatcherPriority.Normal, null );							
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

		private void OnFilterSlideChecked( object obj, RoutedEventArgs ea )
		{
			m_tbFilterSlide.IsEnabled		= true;
		}

		private void OnFilterOverChecked( object obj, RoutedEventArgs ea )
		{
			if( m_cbFilterSlide != null )
			{
				m_cbFilterSlide.IsEnabled		= false;
			}
		}

		private void OnSelectTopChecked( object obj, RoutedEventArgs ea )
		{
			m_cbFilterSlide.IsEnabled		= true;
		}
	}
}
