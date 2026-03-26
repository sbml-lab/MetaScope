using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using MetaScope.Controls;
using MetaScope.Models;
using MetaScope.Services;
using MetaScope.Services.Error;

namespace MetaScope.Views
{
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
	/// Interaction logic for DialogSearch.axaml
	/// </summary>
	public partial class DialogSearch : Window
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
			Opened += ( s, e ) => OnLoaded( s, null );
			KeyDown += ( s, e ) => { if( e.Key == Avalonia.Input.Key.Escape ) Close(); };
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
			MainWindow			mw				= MainWindow.GetMainWindow( this );

			switch( m_eLookIn )
			{
				case ESearchLookIn.CurrentSequenceId :
				{
					DocMap			doc				= mw.GetActiveDocMap();
					if( doc != null )
					{
						string			strId			= doc.SequenceId;

						DoSearchCurrentSequenceId( strId );
					}
					else
					{
						ErrorMessage.ShowErrorSearchNotIntroduction();

						Dispatcher.UIThread.InvokeAsync( () => DoSearchCurrentSequenceIdEnd( null, null ) );
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
			MainWindow		mw				= MainWindow.GetMainWindow( this );
			DocMap			doc				= mw.DoDocumentFind( strSequenceId );

			m_lstType						= new ListDataType();
			m_lstSequenceId					= new ListString();

			if( doc != null )
			{
				for( int i = 0; i < doc.PanelActive.GetCountLane(); i++ )
				{
					PnlMapLane		pnl				= doc.PanelActive.GetLane( i );

					foreach( DataType dt in pnl.DataTypeList )
						m_lstType.Add( dt );

					m_lstSequenceId.Add( strSequenceId );
				}
			}

			Thread			thd				= new Thread( new ThreadStart( DoSearchCurrentSequenceIdThread ) );
			thd.Start();
		}

		private void DoSearchAllSequenceId()
		{
			MainWindow		mw				= MainWindow.GetMainWindow( this );

			m_lstType						= new ListDataType();
			m_lstSequenceId					= new ListString();

			foreach( DocMap doc in mw.ListDocument )
			{
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

				dt.DoSearch( m_strSearch, m_bCase, lst[ i ], new Models.DelegateDoSearchProgressUpdate( DoSearchProgressUpdate ) );
			}

			Dispatcher.UIThread.InvokeAsync( () => DoSearchAllSequenceIdEnd( lst ) );
		}

		private void DoSearchAllSequenceIdEnd( ListListFeature llst )
		{
			MainWindow			mw				= MainWindow.GetMainWindow( this );

			if( m_bClear == true )
			{
				mw.DoSearchResultsClear();
			}

			for( int i = 0; i < m_lstType.Count; i++ )
			{
				ListFeature			lst				= llst[ i ];
				string				strSequenceId	= m_lstSequenceId[ i ];

				foreach( DataFeature df in lst )
				{
					string			str				= string.Format( "{0}\t{1:N0}-{2:N0}\t{3}", strSequenceId, df.Start, df.End, df.Attribute );

					mw.DoSearchResultAdd( str );
				}
			}

			mw.DoSearchResultsActivate();

			Close();
		}

		private void DoSearchCurrentSequenceIdThread()
		{
			ListFeature			lst				= new ListFeature();

			foreach( DataType dt in m_lstType )
			{
				dt.DoSearch( m_strSearch, m_bCase, lst, new Models.DelegateDoSearchProgressUpdate( DoSearchProgressUpdate ) );
			}

			string				strSequenceId	= m_lstSequenceId.Count > 0 ? m_lstSequenceId[ 0 ] : null;

			Dispatcher.UIThread.InvokeAsync( () => DoSearchCurrentSequenceIdEnd( strSequenceId, lst ) );
		}

		private void DoSearchProgressUpdate( int nCurrent, int nTotal )
		{
			Dispatcher.UIThread.InvokeAsync( () => DoSearchProgressUpdateUI( nCurrent, nTotal ) );
		}

		private void DoSearchProgressUpdateUI( int nCurrent, int nTotal )
		{
			m_pbSearch.Minimum					= 0;
			m_pbSearch.Maximum					= nTotal;
			m_pbSearch.Value					= nCurrent;
		}

		private void DoSearchCurrentSequenceIdEnd( string strSequenceId, ListFeature lst )
		{
			MainWindow			mw				= MainWindow.GetMainWindow( this );

			if( strSequenceId != null && lst != null )
			{
				if( m_bClear == true )
				{
					mw.DoSearchResultsClear();
				}

				foreach( DataFeature df in lst )
				{
					string			str				= string.Format( "{0}\t{1:N0}-{2:N0}\t{3}", strSequenceId, df.Start, df.End, df.Attribute );
					mw.DoSearchResultAdd( str );
				}

				mw.DoSearchResultsActivate();
			}

			Close();
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			if( string.IsNullOrWhiteSpace( Search ) )
			{
				Close();
				return;
			}

			m_strSearch		= Search;
			m_eLookIn		= LookIn;
			m_bCase			= CaseSensitive;

			try
			{
				Thread			thd				= new Thread( new ThreadStart( DoSearch ) );
				thd.Start();

				m_splButton.IsVisible			= false;
				m_pbSearch.IsVisible			= true;
			}
			catch( Exception ex )
			{
				MetaScope.Services.Logger.PrintLine( "# Search error: {0}", ex.Message );
				Close();
			}
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
