using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using AvalonDock;
using VugMap.Utility;
using VugMap.Utility.Command;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;
using VugMap.Window;

namespace VugMap.Window
{
	using			ListMap							= List< DocMap >;
	using			ListFeature						= List< DataFeature >;
	using			ListListFeature					= List< List< DataFeature > >;
	using			ListDataType					= List< DataType >;
	using			ListString						= List< string >;

	public enum ESearchLookIn
	{
		CurrentSequenceId,
		AllSequenceId
	}

	/// <summary>
	/// Interaction logic for DialogSearch.xaml
	/// </summary>
	public partial class DialogSearch : System.Windows.Window
	{
		//			.								.								.		
		private		bool							m_bSearching					= false;
		private		string							m_strSearch						= null;
		private		ESearchLookIn					m_eLookIn						= ESearchLookIn.CurrentSequenceId;
		private		bool							m_bCase							= false;
		private		bool							m_bClear						= true;
		private		ListDataType					m_lstType						= null;
		private		ListString						m_lstSequenceId					= null;
		
		public		delegate void DelegateDoSearchCurrentSequenceIdEnd( string strSequenceId, ListFeature lst );
		public		delegate void DelegateDoSearchAllSequenceIdEnd( ListListFeature lst );
		public		delegate void DelegateDoSearchProgressUpdate( int nCurrent, int nTotal );
		public		delegate void DelegateDoSearchProgressUpdateUI( int nCurrnet, int nTotal );

		public DialogSearch()
		{
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

		public bool IsSearching
		{
			get {	return m_bSearching; }
			set {	m_bSearching = value; }
		}

		public string Search
		{
			get {	return m_tbSearch.Text; }
		}

		public ESearchLookIn LookIn
		{
			get {	return GetLookIn(); }
		}

		public bool CaseSensitive
		{
			get {	return m_ckbCase.IsChecked == true ? true : false; }
		}

		public ESearchLookIn GetLookIn()
		{
			if( m_cbiCurrentId.IsSelected == true )
			{
				return ESearchLookIn.CurrentSequenceId;
			}
			else if( m_cbiAllId.IsSelected == true )
			{
				return ESearchLookIn.AllSequenceId;
			}
			else
			{
				Debug.Assert( false );
				return ESearchLookIn.AllSequenceId;
			}
		}

		public void DoSearch()
		{
			MainWindow			mw				= MainWindow.GetMainWindow();

			switch( m_eLookIn )
			{
				case ESearchLookIn.CurrentSequenceId :
				{
					ManagedContent	mc				= mw.m_dckmVugmap.ActiveDocument;
					if( mc is DocMap )
					{
						DocMap			doc				= mc as DocMap;
						string			strId			= doc.SequenceId;

						DoSearchCurrentSequenceId( strId );
					}
					else
					{
						ErrorMessage.ShowErrorSearchNotIntroduction();

						Dispatcher.BeginInvoke( new DelegateDoSearchCurrentSequenceIdEnd( DoSearchCurrentSequenceIdEnd ), DispatcherPriority.Normal, null, null );						
					}

					break;
				}

				case ESearchLookIn.AllSequenceId :
				{
					DoSearchAllSequenceId();
					break;
				}
			}
		}

		private void DoSearchCurrentSequenceId( string strSequenceId )
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			DocMap			doc				= mw.DoDocumentFind( strSequenceId );
			ListFeature		lst				= new ListFeature();

			m_lstType						= new ListDataType();
			m_lstSequenceId					= new ListString();

			for( int i = 0; i < doc.PanelActive.GetCountLane(); i++ )
			{
				PnlMapLane		pnl				= doc.PanelActive.GetLane( i );
				
				foreach( DataType dt in pnl.DataTypeList )
					m_lstType.Add( dt );				

				m_lstSequenceId.Add( strSequenceId );
			}	
						
			Thread			thd				= new Thread( new ThreadStart( DoSearchCurrentSequenceIdThread ) );	
			thd.Start();
		}

		private void DoSearchAllSequenceId()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			ListFeature		lst				= new ListFeature();

			m_lstType						= new ListDataType();
			m_lstSequenceId					= new ListString();

			foreach( object obj in mw.ListDocument )
			{
				if( obj is DocMap )
				{
					DocMap			doc				= obj as DocMap;
					string			strSequenceId	= doc.SequenceId;
					
					for( int i = 0; i < doc.PanelActive.GetCountLane(); i++ )
					{
						PnlMapLane		pnl				= doc.PanelActive.GetLane( i );
						
						foreach( DataType dt in pnl.DataTypeList )
						{
							m_lstType.Add( dt );
							m_lstSequenceId.Add( strSequenceId );
						}					
					}	
				}
			}

			Thread			thd				= new Thread( new ThreadStart( DoSearchAllSequenceIdThread ) );	
			thd.Start();
		}

		private void DoSearchAllSequenceIdThread()
		{			
			ListListFeature		lst				= new ListListFeature();

			for( int i = 0; i < m_lstType.Count; i++ )
			{
				DataType			dt				= m_lstType[ i ];				
				lst.Add( new ListFeature() );

				dt.DoSearch( m_strSearch, m_bCase, lst[ i ], new DelegateDoSearchProgressUpdate( DoSearchProgressUpdate ) );
			}

			Dispatcher.BeginInvoke( new DelegateDoSearchAllSequenceIdEnd( DoSearchAllSequenceIdEnd ), DispatcherPriority.Normal, lst );
		}

		private void DoSearchAllSequenceIdEnd( ListListFeature llst )
		{			
			MainWindow			mw				= MainWindow.GetMainWindow();

			if( m_bClear == true )
			{
				mw.m_ltvSearch.Items.Clear();
			}

			for( int i = 0; i < m_lstType.Count; i++ )
			{
				ListFeature			lst				= llst[ i ];
				string				strSequenceId	= m_lstSequenceId[ i ];

				foreach( DataFeature df in lst )
				{
					string			str				= string.Format( "{0}\t{1:N0}-{2:N0}\t{3}", strSequenceId, df.Start, df.End, df.Attribute );

					ListViewItem	lvi				= new ListViewItem();
					lvi.Content						= str;
					
					mw.m_ltvSearch.Items.Add( lvi );
				}
			}

			mw.m_dcntSearch.SetAsActive();
						
			Close();
		}

		private void DoSearchCurrentSequenceIdThread()
		{			
			ListFeature			lst				= new ListFeature();

			foreach( DataType dt in m_lstType )
			{
				dt.DoSearch( m_strSearch, m_bCase, lst, new DelegateDoSearchProgressUpdate( DoSearchProgressUpdate ) );
			}

			Dispatcher.BeginInvoke( new DelegateDoSearchCurrentSequenceIdEnd( DoSearchCurrentSequenceIdEnd ), DispatcherPriority.Normal, m_lstSequenceId[ 0 ], lst );
		}
	
		private void DoSearchProgressUpdate( int nCurrent, int nTotal )
		{
			Dispatcher.BeginInvoke( new DelegateDoSearchProgressUpdateUI( DoSearchProgressUpdateUI ), DispatcherPriority.Normal, nCurrent, nTotal );		
		}

		private void DoSearchProgressUpdateUI( int nCurrent, int nTotal )
		{
			m_pbSearch.Minimum					= 0;
			m_pbSearch.Maximum					= nTotal;
			m_pbSearch.Value					= nCurrent;
		}

		private void DoSearchCurrentSequenceIdEnd( string strSequenceId, ListFeature lst )
		{			
			MainWindow			mw				= MainWindow.GetMainWindow();

			if( strSequenceId != null && lst != null )
			{
				if( m_bClear == true )
				{
					mw.m_ltvSearch.Items.Clear();
				}

				foreach( DataFeature df in lst )
				{
					string			str				= string.Format( "{0}\t{1:N0}-{2:N0}\t{3}", strSequenceId, df.Start, df.End, df.Attribute );
					mw.m_ltvSearch.Items.Add( str );
				}

				mw.m_dcntSearch.SetAsActive();
			}
						
			Close();
		}	

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			m_strSearch		= Search;
			m_eLookIn		= LookIn;
			m_bCase			= CaseSensitive;

			Thread			thd				= new Thread( new ThreadStart( DoSearch ) );
			thd.Start();

			m_splButton.Visibility			= Visibility.Collapsed;
			m_pbSearch.Visibility			= Visibility.Visible;			
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbSearch.Focus();
		}
	}
}
