using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

using MetaScope.Models;
using MetaScope.Services;
using MetaScope.Services.Command;
using MetaScope.Services.Error;

namespace MetaScope.Controls
{
	using			ListPnlMapLane					= List< PnlMapLane >;

	public class PnlMap : Panel
	{
		//			.								.								.
		public		static int						N_LANE_VERTICALGAP				= 10;

		public		static int						N_RULER_TOP						= 60;
		public		static int						N_RULER_LEFT					= 50;
		public		static int						N_RULER_TIPSPAN					= 20;
		public		static int						N_RULER_NOTCHHEIGHT				= 4;
		public		static int						N_RULERTEXT_FONTSIZE			= 12;
		public		static int						N_RULERTEXT_WIDTH				= 8;
		public		static int						N_RULERTEXT_HEIGHT				= 12;

		public		static Color					CLR_LANE_BACK					= Color.FromArgb( 255, 240, 240, 240 );
		public		static Color					CLR_LANE_BACKOVER				= Color.FromArgb( 255, 245, 245, 245 );
		public		static Color					CLR_LANE_SELECTION				= Color.FromArgb( 125, 121, 176, 245 );

		public		static string					STR_PANEL_RULER					= "Ruler";
		public		static string					STR_ELEMENT_LANEBACK			= "Lane.Back";
		public		static string					STR_ELEMENT_DRAGBOX				= "Drag.Box";
		public		static string					STR_ELEMENT_SELECTION			= "Select";
		public		static string					STR_ELEMENT_DRAGPLACER			= "Placer";

		public		static double					N_MAP_ZOOMMAX					= 1024 * 128;
		public		static double					N_MAP_ZOOMMIN					= 1;
		public		static double					N_MAP_ZOOMTHRESHOULD			= 16;

		private		object							m_docMap						= null;
		private		ListPnlMapLane					m_lstLane						= null;

		private		int								m_nPositionMin					= 0;
		private		int								m_nPositionMax					= 0;
		private		int								m_nPosition						= 0;
		private		double							m_dZoom							= 1.0f;

		private		int								m_nHighlightStart				= -1;
		private		int								m_nHighlightEnd					= -1;

		// Selection overlay brush (drawn at PnlMap level spanning all lanes — gotcha #3)
		private		IBrush							m_bshSelection					= null;

		// Selection overlay state
		private		bool							m_bSelectionVisible				= false;
		private		double							m_dSelectionX					= 0.0;
		private		double							m_dSelectionWidth				= 0.0;

		// Ruler
		private		PnlMapRuler						m_pnlRuler						= null;

		// Lane
		private		ListPnlMapLane					m_lstLaneEditable				= null;
		private		ListPnlMapLane					m_lstLaneSelected				= null;

		// Drag box state
		private		Point							m_ptDragStart;
		private		Point							m_ptDragEnd;
		private		bool							m_bDragBox						= false;
		private		bool							m_bDragLane						= false;

		// Drag box visual state (rendered in Render override instead of WPF Rectangle children)
		private		bool							m_bDragBoxVisible				= false;
		private		double							m_dDragBoxX						= 0.0;
		private		double							m_dDragBoxY						= 0.0;
		private		double							m_dDragBoxWidth					= 0.0;
		private		double							m_dDragBoxHeight				= 0.0;

		// Placer visual state
		private		bool							m_bPlacerVisible				= false;
		private		double							m_dPlacerY						= 0.0;

		// Lane back rectangle state
		private		double							m_dLaneBackWidth				= 0.0;
		private		double							m_dLaneBackHeight				= 0.0;

		/// <summary>
		/// Raised when the main window needs a status bar update.
		/// </summary>
		public static event Action					OnStatusBarUpdateRequested;

		/// <summary>
		/// Raised when the main window needs to debounce workspace saving.
		/// </summary>
		public static event Action					OnWorkspaceSaveDebounceRequested;

		/// <summary>
		/// Raised when explorer panel needs updating.
		/// </summary>
		public static event Action					OnExplorerUpdateRequested;

		/// <summary>
		/// Raised when edit panel needs updating.
		/// </summary>
		public static event Action					OnEditUpdateRequested;

		/// <summary>
		/// Raised when an auto-save should be triggered immediately.
		/// </summary>
		public static event Action					OnAutoSaveImmediateRequested;

		/// <summary>
		/// Raised when a document should be closed.
		/// Signature: Action(docMap) — the DocMap object to close.
		/// </summary>
		public static event Action< object >		OnDocumentCloseRequested;

		/// <summary>
		/// Raised when files are dropped onto the panel.
		/// Signature: Action(string[] filePaths)
		/// </summary>
		public static event Action< string[] >		OnFilesDropped;

		/// <summary>
		/// Raised when the scrollbar state needs updating.
		/// Signature: Action(docMap) — the DocMap whose scrollbar should refresh.
		/// </summary>
		public static event Action< object >		OnScrollSetRequested;

		/// <summary>
		/// Delegate to check if selection is by position only (no score filtering).
		/// Returns true if select-by-position mode is active.
		/// </summary>
		public static Func< bool >					IsSelectByPositionFunc;

		/// <summary>
		/// Raised when zoom-in is requested (may propagate to split panel via DocMap).
		/// </summary>
		public event Action							OnZoomInRequested;

		/// <summary>
		/// Raised when zoom-out is requested.
		/// </summary>
		public event Action							OnZoomOutRequested;

		/// <summary>
		/// Raised when scroll-left is requested.
		/// </summary>
		public event Action							OnScrollLeftRequested;

		/// <summary>
		/// Raised when scroll-right is requested.
		/// </summary>
		public event Action							OnScrollRightRequested;

		public PnlMap()
		{
			m_lstLane							= new ListPnlMapLane();
			m_lstLaneEditable					= new ListPnlMapLane();
			m_lstLaneSelected					= new ListPnlMapLane();

			m_bshSelection						= new ImmutableSolidColorBrush( CLR_LANE_SELECTION );

			BuildElementRuler();
			EnsureOverlay();

			ClipToBounds						= true;
			Focusable							= true;

			// Pointer events — use Bubble + handledEventsToo so we see events even after lanes handle them
			AddHandler( Avalonia.Input.InputElement.PointerPressedEvent, OnPointerPressed, Avalonia.Interactivity.RoutingStrategies.Bubble, true );
			AddHandler( Avalonia.Input.InputElement.PointerReleasedEvent, OnPointerReleased, Avalonia.Interactivity.RoutingStrategies.Bubble, true );
			AddHandler( Avalonia.Input.InputElement.PointerMovedEvent, OnPointerMoved, Avalonia.Interactivity.RoutingStrategies.Bubble, true );
			PointerWheelChanged					+= OnPointerWheelChanged;
		}

		private		bool							m_bNeedsInitialUpdate			= false;

		protected override void OnAttachedToVisualTree( VisualTreeAttachmentEventArgs e )
		{
			base.OnAttachedToVisualTree( e );
			m_bNeedsInitialUpdate = true;
		}


		protected override void OnSizeChanged( SizeChangedEventArgs e )
		{
			base.OnSizeChanged( e );

			Logger.PrintLine( "# PnlMap:OnSizeChanged()" );

			DoUpdateView();
		}

		// ─────────────────────────────────────────────────
		//  Properties
		// ─────────────────────────────────────────────────

		public object DocMap
		{
			get {	return m_docMap; }
			set {	m_docMap = value; }
		}

		public ListPnlMapLane LaneList
		{
			get {	return m_lstLane; }
			set {	m_lstLane = value; }
		}

		public double ZoomThreshould
		{
			get {	return N_MAP_ZOOMTHRESHOULD; }
		}

		public int PositionMax
		{
			get {	return m_nPositionMax; }
		}

		public int PositionMin
		{
			get {	return m_nPositionMin; }
		}

		public int PositionRange
		{
			get
			{
				int				nRange			= PositionMax - PositionMin;
				return nRange;
			}
		}

		public int Position
		{
			get {	return m_nPosition; }
		}

		public double Zoom
		{
			get
			{
				return m_dZoom;
			}

			set
			{
				m_dZoom			= value;
			}
		}

		public ListPnlMapLane LaneSelected
		{
			get {	return m_lstLaneSelected; }
		}

		public ListPnlMapLane ListLaneEditable
		{
			get {	return m_lstLaneEditable; }
		}

		// ─────────────────────────────────────────────────
		//  DocMap helper — access SequenceId via dynamic
		// ─────────────────────────────────────────────────

		private string GetDocMapSequenceId()
		{
			if( m_docMap == null )
				return null;

			// DocMap is typed as object to avoid circular dependency; access via dynamic
			return ((dynamic) m_docMap).SequenceId;
		}

		// ─────────────────────────────────────────────────
		//  Close
		// ─────────────────────────────────────────────────

		public void DoClose()
		{
			foreach( PnlMapLane pnl in m_lstLane )
			{
				pnl.DoClose();
			}

			m_lstLane.Clear();
		}

		// ─────────────────────────────────────────────────
		//  Layout Copy (for split view)
		// ─────────────────────────────────────────────────

		public void DoLaneLayoutCopy( PnlMap pnl )
		{
			Zoom							= pnl.Zoom;
			SetPosition( pnl.Position );

			DoLaneRemoveAll();

			m_lstLane.Clear();
			m_lstLaneSelected.Clear();

			foreach( PnlMapLane pnlEach in pnl.LaneList )
			{
				PnlMapLane		pnlLane			= new PnlMapLane();

				foreach( DataType dt in pnlEach.DataTypeList )
				{
					pnlLane.DoDataTypeAdd( dt );
				}

				pnlLane.LaneHeight			= pnlEach.LaneHeight;

				DoLaneAdd( pnlLane );
			}

			DoUpdateView();
		}

		// ─────────────────────────────────────────────────
		//  Position / Zoom
		// ─────────────────────────────────────────────────

		public void SetPosition( int nPosition )
		{
			int				nMax				= UtilityMath.DoRound( PositionMax - PositionRange / Zoom );
			nPosition							= Math.Min( nPosition, nMax );
			nPosition							= Math.Max( nPosition, PositionMin );

			m_nPosition							= nPosition;
		}

		public void DoZoomTo( double dZoom )
		{
			Zoom			= Math.Max( Math.Min( dZoom, N_MAP_ZOOMMAX ), N_MAP_ZOOMMIN );

			m_nPosition		= Math.Min( m_nPositionMax - ( int ) ( PositionRange / Zoom ), m_nPosition );

			DoSelection();
			DoUpdateView();
		}

		public void DoZoomIn()
		{
			Zoom			= Math.Min( Zoom * 2, N_MAP_ZOOMMAX );

			DoSelection();
			DoUpdateView();
		}

		public void DoZoomOut()
		{
			Zoom			= Math.Max( Zoom / 2, N_MAP_ZOOMMIN );

			m_nPosition		= Math.Min( m_nPositionMax - ( int ) ( PositionRange / Zoom ), m_nPosition );

			DoSelection();
			DoUpdateView();
		}

		public void DoScrollLeft()
		{
			int				nPage			= UtilityMath.DoRound( ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom );
			int				nPosition		= m_nPosition - nPage / 2;

			m_nPosition		= Math.Max( m_nPositionMin, nPosition );

			DoSelection();
			DoUpdateView();
		}

		public void DoScrollRight()
		{
			int				nPage			= UtilityMath.DoRound( ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom );
			int				nPosition		= m_nPosition + nPage / 2;

			m_nPosition		= Math.Min( m_nPositionMax - nPage, nPosition );

			DoSelection();
			DoUpdateView();
		}

		public void DoScrollLeftSmall()
		{
			int				nPage			= UtilityMath.DoRound( ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom );
			int				nPosition		= m_nPosition - nPage / 8;

			m_nPosition		= Math.Max( m_nPositionMin, nPosition );

			DoSelection();
			DoUpdateView();
		}

		public void DoScrollRightSmall()
		{
			int				nPage			= UtilityMath.DoRound( ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom );
			int				nPosition		= m_nPosition + nPage / 8;

			m_nPosition		= Math.Min( m_nPositionMax - nPage, nPosition );

			DoSelection();
			DoUpdateView();
		}

		// ─────────────────────────────────────────────────
		//  Export
		// ─────────────────────────────────────────────────

		public void DoExportPng( string strPath, double dDpi )
		{
			try
			{
				var pixelSize = new Avalonia.PixelSize( (int)( Bounds.Width * dDpi / 96.0 ), (int)( Bounds.Height * dDpi / 96.0 ) );
				var dpiVector = new Avalonia.Vector( dDpi, dDpi );
				using( var rtb = new Avalonia.Media.Imaging.RenderTargetBitmap( pixelSize, dpiVector ) )
				{
					rtb.Render( this );
					rtb.Save( strPath );
				}
			}
			catch( Exception e )
			{
				Logger.PrintLine( "# PnlMap:DoExportPng — {0}", e.Message );
			}
		}

		public void DoExportSvg( string strPath )
		{
			SvgExporter.DoExport( this, Bounds.Width, Bounds.Height, strPath );
		}

		// ─────────────────────────────────────────────────
		//  Lane Selection (UI selection of lanes, not feature selection)
		// ─────────────────────────────────────────────────

		public int GetCountLaneSelected()
		{
			int				nCount			= m_lstLaneSelected.Count;

			return nCount;
		}

		public void DoLaneSelectedAdd( PnlMapLane pnlSelected )
		{
			if( pnlSelected.IsSelected == false )
			{
				pnlSelected.IsSelected			= true;
				pnlSelected.DoLayoutUpdate();
			}

			if( m_lstLaneSelected.Contains( pnlSelected ) == false )
				m_lstLaneSelected.Add( pnlSelected );
		}

		public void DoLaneSelected( PnlMapLane pnlSelected )
		{
			m_lstLaneSelected.Clear();

			foreach( PnlMapLane pnl in m_lstLane )
			{
				if( pnl != pnlSelected && pnl.IsSelected == true )
				{
					pnl.IsSelected					= false;
					pnl.DoLayoutUpdate();
				}
			}

			if( pnlSelected.IsSelected == false )
			{
				pnlSelected.IsSelected			= true;
				pnlSelected.DoLayoutUpdate();
			}

			m_lstLaneSelected.Add( pnlSelected );
		}

		public void DoLaneSelectedUpdate()
		{
			m_lstLaneSelected.Clear();

			foreach( PnlMapLane pnl in m_lstLane )
			{
				if( pnl.IsSelected == true )
				{
					m_lstLaneSelected.Add( pnl );
				}
			}
		}

		// ─────────────────────────────────────────────────
		//  Lane Feature Operations (unite, delete selected)
		// ─────────────────────────────────────────────────

		public void DoLaneFeatureUniteSelected()
		{
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandReplace	cmd				= me.MakeCommandReplace();

			foreach( PnlMapLane pnl in m_lstLaneEditable )
			{
				pnl.DoFeatureUniteSelected( cmd );
			}

			cmd.DoLaneUpdate();
			OnExplorerUpdateRequested?.Invoke();
			OnEditUpdateRequested?.Invoke();
			OnAutoSaveImmediateRequested?.Invoke();
		}

		public void DoLaneFeatureDeleteSelected()
		{
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandDelete	cmd				= me.MakeCommandDelete();

			foreach( PnlMapLane pnl in m_lstLaneEditable )
			{
				pnl.DoFeatureDeleteSelected( cmd );
			}

			OnExplorerUpdateRequested?.Invoke();
			OnEditUpdateRequested?.Invoke();
			OnAutoSaveImmediateRequested?.Invoke();
		}

		// ─────────────────────────────────────────────────
		//  Lane Editable
		// ─────────────────────────────────────────────────

		public void DoLaneSetEditable( PnlMapLane pnlToEdit )
		{
			if( m_lstLaneEditable.Contains( pnlToEdit ) == true )
			{
				pnlToEdit.SetEditable( false );
				m_lstLaneEditable.Remove( pnlToEdit );
			}
			else
			{
				pnlToEdit.SetEditable( true );
				m_lstLaneEditable.Add( pnlToEdit );
			}
		}

		// ─────────────────────────────────────────────────
		//  Lane Count / Access
		// ─────────────────────────────────────────────────

		public int GetCountLane()
		{
			int				nCount			= m_lstLane.Count;

			return nCount;
		}

		public PnlMapLane GetLane( string strType )
		{
			foreach( PnlMapLane pnl in m_lstLane )
			{
				if( pnl.DataTypeSelected.Type == strType )
					return pnl;
			}

			return null;
		}

		public PnlMapLane GetLane( int nIndex )
		{
			PnlMapLane		dl				= m_lstLane[ nIndex ];

			return dl;
		}

		public int GetCountLaneWOHeight()
		{
			int				nCountAll		= GetCountLane();
			int				nCountWHeight	= 0;

			foreach( PnlMapLane dl in m_lstLane )
			{
				if( dl.LaneHeight != 0.0f )
				{
					nCountWHeight++;
				}
			}

			int				nCount			= nCountAll - nCountWHeight;

			return nCount;
		}

		public double GetLaneHeightSum()
		{
			double			dHeightSum		= 0;

			foreach( PnlMapLane dl in m_lstLane )
			{
				if( dl.LaneHeight != 0.0f )
				{
					dHeightSum		+= dl.LaneHeight;
				}
			}

			return dHeightSum;
		}

		public void SetLaneHeight( double dHeight )
		{
			foreach( PnlMapLane dl in m_lstLane )
			{
				if( dl.LaneHeight != 0.0f )
				{
					dl.SetLaneHeightActual( dl.LaneHeight );
				}
				else
				{
					dl.SetLaneHeightActual( dHeight );
				}
			}
		}

		public PnlMapLane DoLaneFind( string strType )
		{
			foreach( PnlMapLane pnlLane in m_lstLane )
			{
				if( pnlLane.DataTypeSelected.Type == strType )
				{
					return pnlLane;
				}
			}

			return null;
		}

		public int GetDataLaneIndex( DataType dt )
		{
			for( int i = 0; i < m_lstLane.Count; i++ )
			{
				if( m_lstLane[ i ].DataTypeSelected == dt )
				{
					return i;
				}
			}

			return -1;
		}

		// ─────────────────────────────────────────────────
		//  Lane Move (reorder)
		// ─────────────────────────────────────────────────

		public void DoLaneMoveUp( PnlMapLane pnlLane )
		{
			if( !m_lstLane.Contains( pnlLane ) )		return;
			if( m_lstLane[ 0 ] == pnlLane )				return;

			int				nIndex			= m_lstLane.IndexOf( pnlLane );

			PnlMapLane		pnlPrev			= m_lstLane[ nIndex - 1 ];
			m_lstLane[ nIndex - 1 ]			= pnlLane;
			m_lstLane[ nIndex ]				= pnlPrev;

			DoUpdateView();
		}

		public void DoLaneMoveDown( PnlMapLane pnlLane )
		{
			if( !m_lstLane.Contains( pnlLane ) )		return;
			if( m_lstLane.Last() == pnlLane )			return;

			int				nIndex			= m_lstLane.IndexOf( pnlLane );

			PnlMapLane		pnlNext			= m_lstLane[ nIndex + 1 ];
			m_lstLane[ nIndex + 1 ]			= pnlLane;
			m_lstLane[ nIndex ]				= pnlNext;

			DoUpdateView();
		}

		public void DoLaneMoveSelectedAfter( int nIndex )
		{
			if( nIndex >= 0 && nIndex < GetCountLane() )
			{
				ListPnlMapLane	lstNew			= new ListPnlMapLane();
				PnlMapLane		pnlIndex		= GetLane( nIndex );

				foreach( PnlMapLane pnlEach in m_lstLane )
				{
					if( pnlEach != pnlIndex && m_lstLaneSelected.Contains( pnlEach ) == false )
					{
						lstNew.Add( pnlEach );
					}
					else if( pnlEach == pnlIndex )
					{
						lstNew.AddRange( m_lstLaneSelected );
						if( m_lstLaneSelected.Contains( pnlIndex ) == false )
							lstNew.Add( pnlIndex );
					}
				}

				m_lstLane						= lstNew;
				DoUpdateView();
			}
			else
			{
				ListPnlMapLane	lstNew			= new ListPnlMapLane();

				foreach( PnlMapLane pnlEach in m_lstLane )
				{
					if( m_lstLaneSelected.Contains( pnlEach ) == false )
					{
						lstNew.Add( pnlEach );
					}
				}

				lstNew.AddRange( m_lstLaneSelected );

				m_lstLane						= lstNew;
				DoUpdateView();
			}
		}

		public int GetLanePlacerPosition()
		{
			// In Avalonia we track position from the last PointerMoved event
			Point			pt				= m_ptDragEnd;

			for( int i = 0; i < GetCountLane(); i++ )
			{
				PnlMapLane		pnl				= GetLane( i );

				double			dLaneY			= pnl.LaneTopOffset;
				double			dLaneH			= pnl.Bounds.Height;

				if( pt.Y >= dLaneY - N_LANE_VERTICALGAP * 2 && pt.Y <= dLaneY + N_LANE_VERTICALGAP * 2 )
					return i;
				else if( pt.Y >= dLaneY + dLaneH - N_LANE_VERTICALGAP * 2 && pt.Y <= dLaneY + dLaneH + N_LANE_VERTICALGAP * 2 )
					return i + 1;
			}

			return -1;
		}

		// ─────────────────────────────────────────────────
		//  Lane Group / Ungroup
		// ─────────────────────────────────────────────────

		public void DoLaneUngroup()
		{
			Debug.Assert( m_lstLaneSelected.Count == 1 );
			Debug.Assert( m_lstLaneSelected[ 0 ].GetCountDataType() > 1 );

			PnlMapLane		pnl				= m_lstLaneSelected[ 0 ];

			m_lstLaneSelected.Clear();

			for( int i = 1; i < pnl.GetCountDataType(); i++ )
			{
				DataType		dt				= pnl.DoDataTypeGet( i );
				pnl.DoDataTypeRemove( dt );

				PnlMapLane		pnlNew			= DoLaneAddAfter( dt, pnl.DataTypeSelected );

				m_lstLaneSelected.Add( pnlNew );
			}

			DoUpdateView();
		}

		public void DoLaneGroup()
		{
			Debug.Assert( m_lstLaneSelected.Count >= 1 );

			PnlMapLane		pnlBottom			= m_lstLaneSelected[ 0 ];

			foreach( PnlMapLane pnl in m_lstLaneSelected )
			{
				if( pnl != pnlBottom )
				{
					for( int i = 0; i < pnl.GetCountDataType(); i++ )
					{
						pnlBottom.DoDataTypeAdd( pnl.DoDataTypeGet( i ) );
					}

					DoLaneRemove( pnl );
					Children.Remove( pnl );
				}
			}

			m_lstLaneSelected.Clear();
			m_lstLaneSelected.Add( pnlBottom );

			DoUpdateView();
		}

		// ─────────────────────────────────────────────────
		//  Lane Show / Add / Remove
		// ─────────────────────────────────────────────────

		public void DoLaneShow( string strType )
		{
			ManagerData		md				= ManagerData.GetManager();

			string			strSeqId		= GetDocMapSequenceId();
			DataType		dt				= md.GetDataType( strSeqId, strType );

			if( dt != null )
			{
				DoLaneAdd( dt );
				DoUpdateView();
			}
		}

		public void DoLaneRemove()
		{
			if( m_docMap == null )
				return;

			ManagerData		mgr				= ManagerData.GetManager();
			if( mgr == null )
				return;

			string			strSequenceId	= GetDocMapSequenceId();

			ListPnlMapLane	lstLane			= new ListPnlMapLane();

			foreach( PnlMapLane pnl in m_lstLane )
			{
				pnl.DoDataTypeRemove();

				if( pnl.GetCountDataType() == 0 )
				{
					lstLane.Add( pnl );
				}
			}

			foreach( PnlMapLane pnl in lstLane )
			{
				DoLaneRemove( pnl );
			}

			m_lstLaneSelected.Clear();
		}

		public void DoLaneAdd( string[] strFileA )
		{
			if( m_docMap == null )
				return;

			ManagerData		mgr				= ManagerData.GetManager();
			if( mgr == null )
				return;

			string			strSequenceId	= GetDocMapSequenceId();

			for( int i = 0; i < mgr.GetCountDataFile(); i++ )
			{
				DataFile		df				= mgr.GetDataFile( i );

				if( strFileA.Contains( df.File ) == true )
				{
					DoLaneAdd( df );
				}
			}

			m_lstLaneSelected.Clear();
		}

		public void DoLaneAdd( DataFile df )
		{
			string			strSequenceId	= GetDocMapSequenceId();

			for( int i = 0; i < df.GetCountDataType(); i++ )
			{
				DataType		dt			= df.GetDataType( i );

				if( dt.SequenceId == strSequenceId )
				{
					DoLaneAdd( dt );
				}
			}
		}

		public PnlMapLane DoLaneAddAfter( DataType dt, DataType dtAfter )
		{
			foreach( PnlMapLane dlEach in m_lstLane )
			{
				if( dlEach.IsContainingDataType( dt ) == true )
				{
					return null;
				}
			}

			PnlMapLane		pnlLane			= new PnlMapLane( dt );

			pnlLane.IsVisible				= true;

			ListPnlMapLane	lstLane			= new ListPnlMapLane();

			foreach( PnlMapLane pnl in m_lstLane )
			{
				if( pnl.DataTypeSelected == dtAfter )
				{
					lstLane.Add( pnl );
					lstLane.Add( pnlLane );
				}
				else
				{
					lstLane.Add( pnl );
				}
			}

			m_lstLane						= lstLane;

			Children.Add( pnlLane );

			return pnlLane;
		}

		public PnlMapLane DoLaneAdd( DataType dt )
		{
			foreach( PnlMapLane dlEach in m_lstLane )
			{
				if( dlEach.IsContainingDataType( dt ) == true )
				{
					return null;
				}
			}

			PnlMapLane		pnlLane			= new PnlMapLane( dt );

			pnlLane.IsVisible				= true;

			// Wire feature selection events to MainWindow — v1.1.11 updates both panels
			pnlLane.OnFeatureSelected		+= ( s, df ) =>
			{
				var mw = MainWindow.GetMainWindow( this );
				mw?.DoFeatureDisplay( df );
				if( df != null )
				{
					var lst = new List<DataFeature> { df };
					mw?.DoFeatureSelectedDisplay( lst );
				}
				else
				{
					mw?.DoFeatureSelectedDisplay( null );
				}
			};
			pnlLane.OnFeatureListSelected	+= ( s, lst ) =>
			{
				var mw = MainWindow.GetMainWindow( this );
				mw?.DoFeatureSelectedDisplay( lst );
				if( lst != null && lst.Count > 0 )
					mw?.DoFeatureDisplay( lst.First() );
			};
			pnlLane.OnFeatureHover			+= ( s, df ) =>
			{
				MainWindow.GetMainWindow( this )?.DoFeatureHoverDisplay( df );
			};

			m_lstLane.Add( pnlLane );
			Children.Add( pnlLane );

			return pnlLane;
		}

		public PnlMapLane DoLaneAdd( PnlMapLane pnl )
		{
			if( m_lstLane.Contains( pnl ) == true )
			{
				return null;
			}

			pnl.IsVisible					= true;

			m_lstLane.Add( pnl );
			Children.Add( pnl );

			return pnl;
		}

		public void DoLaneRemoveAll()
		{
			foreach( PnlMapLane pnl in m_lstLane )
			{
				Children.Remove( pnl );
			}

			m_lstLane.Clear();
			m_lstLaneSelected.Clear();
		}

		public void DoLaneRemoveSelected()
		{
			Debug.Assert( m_lstLaneSelected.Count > 0 );

			ListPnlMapLane	lst				= m_lstLaneSelected.GetRange( 0, m_lstLaneSelected.Count );

			foreach( PnlMapLane pnl in lst )
			{
				DoLaneRemove( pnl );
			}

			m_lstLaneSelected.Clear();
		}

		public void DoLaneRemove( DataFile df )
		{
			for( int i = 0; i < df.GetCountDataType(); i++ )
			{
				DataType		dt				= df.GetDataType( i );

				DoLaneRemove( dt );
			}
		}

		public void DoLaneRemove( PnlMapLane pnl )
		{
			m_lstLane.Remove( pnl );
			Children.Remove( pnl );
		}

		public void DoLaneRemove( DataType dt )
		{
			int				nIndex			= GetDataLaneIndex( dt );

			if( nIndex == -1 )
				return;

			PnlMapLane		pnlLane			= m_lstLane[ nIndex ];

			m_lstLane.RemoveAt( nIndex );
			Children.Remove( pnlLane );

			if( m_lstLane.Count == 0 )
			{
				if( m_docMap == null )
					return;

				OnDocumentCloseRequested?.Invoke( m_docMap );
			}
		}

		// ─────────────────────────────────────────────────
		//  Ruler
		// ─────────────────────────────────────────────────

		private void BuildElementRuler()
		{
			m_pnlRuler						= new PnlMapRuler();
			m_pnlRuler.MapParent			= this;
			m_pnlRuler.IsVisible			= true;

			Children.Add( m_pnlRuler );
		}

		// ─────────────────────────────────────────────────
		//  Selection Overlay (blue highlight — gotcha #3: drawn at PnlMap level)
		// ─────────────────────────────────────────────────

		public void DoSelectUpdate()
		{
			if( m_docMap == null )
				return;
		}

		public void DoSelection()
		{
			m_bSelectionVisible				= false;
			InvalidateOverlay();
		}

		public void DoSelection( double dX, double dWidth )
		{
			m_dSelectionX					= dX;
			m_dSelectionWidth				= dWidth;
			m_bSelectionVisible				= true;
			InvalidateOverlay();
		}

		public void DoFeatureHighlightSet( int nStart, int nEnd )
		{
			m_nHighlightStart	= nStart;
			m_nHighlightEnd		= nEnd;
			DoFeatureHighlightUpdate();
		}

		public void DoFeatureHighlightClear()
		{
			m_nHighlightStart	= -1;
			m_nHighlightEnd		= -1;
			DoSelection();
		}

		private void DoFeatureHighlightUpdate()
		{
			if( m_nHighlightStart < 0 )
				return;
			double			dX			= GetPixelFromPosition( m_nHighlightStart );
			double			dXEnd		= GetPixelFromPosition( m_nHighlightEnd );
			double			dWidth		= Math.Max( 1.0, dXEnd - dX );
			DoSelection( dX, dWidth );
		}

		// ─────────────────────────────────────────────────
		//  Coordinate Conversion
		// ─────────────────────────────────────────────────

		public double GetPixelFromPosition( double dX )
		{
			double			dWidthRuler		= Bounds.Width - PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - PnlMapLane.N_LANE_MARGIN;
			double			dWidthPosition	= ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom;

			double			dPixel			= ( dX - m_nPosition ) / dWidthPosition * dWidthRuler + N_RULER_LEFT;

			return dPixel;
		}

		public double GetPositionFromPixel( double dX )
		{
			dX				= dX - N_RULER_LEFT;

			double			dWidthRuler		= Bounds.Width - PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - PnlMapLane.N_LANE_MARGIN;
			double			dWidthPosition	= ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom;

			double			dPosition		= dWidthPosition * dX / dWidthRuler + m_nPosition;

			return dPosition;
		}

		public double GetHeightMinimum()
		{
			double			dHeightMin		= N_RULER_TOP + GetLaneHeightSum()
											+ PnlMapLane.N_LANE_HEIGHTMINIMUM * GetCountLaneWOHeight()
											+ ( GetCountLane() + 1 ) * N_LANE_VERTICALGAP;

			return dHeightMin;
		}

		// ─────────────────────────────────────────────────
		//  Update Size (called from parent ScrollViewer)
		// ─────────────────────────────────────────────────

		private ScrollViewer GetParentScrollViewer()
		{
			Avalonia.Visual	p				= Parent as Avalonia.Visual;
			while( p != null )
			{
				if( p is ScrollViewer s ) return s;
				p = p.GetVisualParent();
			}
			return null;
		}

		public void DoUpdateSize()
		{
			ScrollViewer	sv				= GetParentScrollViewer();

			double			dMinimum		= GetHeightMinimum();
			double			dViewerH		= sv != null ? sv.Bounds.Height : 600;
			double			dViewerW		= sv != null ? sv.Bounds.Width  : 800;

			if( dViewerH <= 0 )		dViewerH = 600;
			if( dViewerW <= 0 )		dViewerW = 800;
			if( dMinimum <= 0 )		dMinimum = 400;

			Height							= Math.Max( dMinimum, dViewerH );
			Width							= dViewerW;
		}

		// ─────────────────────────────────────────────────
		//  DoUpdateView — master refresh
		// ─────────────────────────────────────────────────

		public void DoUpdateView()
		{
			if( m_dZoom == 0.0f )
				return;

			if( m_docMap == null )
				return;

			ManagerData		mgr				= ManagerData.GetManager();
			if( mgr == null )
				return;

			string			strSequenceId	= GetDocMapSequenceId();

			m_nPositionMin	= mgr.GetPositionMin( strSequenceId );
			m_nPositionMax	= mgr.GetPositionMax( strSequenceId );

			// Get fresh size from ScrollViewer viewport (not our own Bounds)
			DoUpdateSize();

			double			dWidthActual	= Bounds.Width;
			double			dHeightActual	= Bounds.Height;

			if( dWidthActual == 0.0f || dHeightActual == 0.0f )
				return;

			DoUpdateRuler();
			DoUpdateLane();
			DoUpdateScrollbar();

			OnStatusBarUpdateRequested?.Invoke();
			OnWorkspaceSaveDebounceRequested?.Invoke();
		}

		public void DoUpdateRuler()
		{
			m_pnlRuler.Width				= Bounds.Width;
			m_pnlRuler.Height				= N_RULER_TOP;
			m_pnlRuler.InvalidateVisual();
		}

		public void DoUpdateScrollbar()
		{
			if( m_docMap == null )
				return;

			OnScrollSetRequested?.Invoke( m_docMap );
		}

		public void DoUpdateLane()
		{
			if( m_dZoom == 0.0f )
			{
				return;
			}

			Debug.Assert( m_dZoom >= 0.0f );

			DoUpdateLaneBack();
			DoUpdateLaneDrag();
			DoUpdateLaneHeight();
			DoUpdateLaneLayout();
		}

		private void DoUpdateLaneDrag()
		{
			// Drag box height tracks panel height (rendered in Render override)
			m_dDragBoxHeight				= Bounds.Height;
		}

		private void DoUpdateLaneLayout()
		{
			double			dWidthActual	= Bounds.Width;
			double			dHeightActual	= Bounds.Height;
			double			dLaneTop		= N_RULER_TOP + N_LANE_VERTICALGAP;
			double			dWidthLane		= dWidthActual - N_LANE_VERTICALGAP * 2;
			double			dHeightLane		= dHeightActual - dLaneTop - N_LANE_VERTICALGAP;

			int				nCountLane		= GetCountLane();
			int				nCountLaneWOH	= GetCountLaneWOHeight();
			double			dHeightSum		= GetLaneHeightSum();
			double			dHeightEach		= ( dHeightLane - dHeightSum ) / nCountLaneWOH;

			double			dTopEach		= dLaneTop;
			double			dLeftEach		= N_LANE_VERTICALGAP;

			for( int i = 0; i < nCountLane; i++ )
			{
				PnlMapLane		pnlLane			= GetLane( i );

				pnlLane.PositionDisplayMin		= m_nPosition;
				pnlLane.PositionDisplayMax		= m_nPosition + UtilityMath.DoRound( ( m_nPositionMax - m_nPositionMin ) / m_dZoom );

				pnlLane.LaneWidth				= dWidthLane;
				pnlLane.Width					= dWidthLane;
				pnlLane.Height					= pnlLane.LaneHeightActual;

				// Store position for ArrangeOverride to use
				pnlLane.LaneLeftOffset			= dLeftEach;
				pnlLane.LaneTopOffset			= dTopEach;

				pnlLane.DoLayoutUpdate();

				dTopEach		+= pnlLane.LaneHeightActual + N_LANE_VERTICALGAP;
			}

			DoFeatureHighlightUpdate();

			// Trigger re-layout so ArrangeOverride positions children
			InvalidateMeasure();
			InvalidateArrange();
		}

		private void DoUpdateLaneBack()
		{
			double			dWidthActual	= Bounds.Width;
			double			dHeightActual	= Bounds.Height;
			double			dLaneTop		= N_RULER_TOP + N_LANE_VERTICALGAP;
			double			dWidthLane		= dWidthActual - N_LANE_VERTICALGAP * 2;
			double			dHeightLane		= dHeightActual - dLaneTop - N_LANE_VERTICALGAP;

			m_dLaneBackWidth				= dWidthLane;
			m_dLaneBackHeight				= dHeightLane;
		}

		private void DoUpdateLaneHeight()
		{
			double			dWidthActual	= Bounds.Width;
			double			dHeightActual	= Bounds.Height;
			double			dLaneTop		= N_RULER_TOP + N_LANE_VERTICALGAP;
			double			dWidthLane		= dWidthActual - N_LANE_VERTICALGAP * 2;
			double			dHeightLane		= dHeightActual - dLaneTop - N_LANE_VERTICALGAP;

			int				nCountLane		= GetCountLane();
			int				nCountLaneWOH	= GetCountLaneWOHeight();
			double			dHeightSum		= GetLaneHeightSum();
			double			dHeightEach		= ( dHeightLane - dHeightSum - ( nCountLane - 1 ) * N_LANE_VERTICALGAP ) / nCountLaneWOH;

			SetLaneHeight( dHeightEach );
		}

		// ─────────────────────────────────────────────────
		//  Overlay — draws selection highlight + drag box at PnlMap level
		//  CRITICAL: The blue selection overlay must span ALL lanes (gotcha #3)
		//
		//  Avalonia's Panel.Render() is sealed, so we use a transparent overlay
		//  Control child that sits on top of all lanes and handles drawing.
		// ─────────────────────────────────────────────────

		private		PnlMapOverlay					m_overlay						= null;

		private void EnsureOverlay()
		{
			if( m_overlay == null )
			{
				m_overlay					= new PnlMapOverlay( this );
				m_overlay.IsHitTestVisible	= false;		// let pointer events pass through
				m_overlay.ZIndex			= 1000;			// always on top of lanes
				Children.Add( m_overlay );
			}
		}

		/// <summary>
		/// Called by the overlay control during its Render pass.
		/// </summary>
		internal void RenderOverlay( DrawingContext dc )
		{
			// Ensure lanes that haven't been laid out get updated
			foreach( PnlMapLane pnlLane in m_lstLane )
			{
				if( pnlLane.GetIsLayoutUpdated() == false )
				{
					pnlLane.DoLayoutUpdate();
				}
			}

			// Draw selection overlay spanning all lanes (gotcha #3)
			if( m_bSelectionVisible && m_dSelectionWidth > 0 )
			{
				Rect			rtSelection		= new Rect( m_dSelectionX, 0, m_dSelectionWidth, Bounds.Height );
				dc.DrawRectangle( m_bshSelection, null, rtSelection );
			}

			// Draw drag box
			if( m_bDragBoxVisible && m_bDragBox )
			{
				IPen			penDragBox		= new Pen( Brushes.Black, 1.0 );
				Rect			rtDragBox		= new Rect( m_dDragBoxX, m_dDragBoxY, m_dDragBoxWidth, m_dDragBoxHeight );
				dc.DrawRectangle( null, penDragBox, rtDragBox );
			}

			// Draw placer indicator
			if( m_bPlacerVisible && m_bDragLane )
			{
				IBrush			bshPlacer		= Brushes.Gray;
				IPen			penPlacer		= new Pen( Brushes.Gray, 1.0 );
				double			dPlacerX		= 5.0;
				double			dPlacerSize		= 8.0;
				Rect			rtPlacer		= new Rect( dPlacerX, m_dPlacerY - dPlacerSize / 2, dPlacerSize, dPlacerSize );
				dc.DrawRectangle( bshPlacer, penPlacer, rtPlacer, dPlacerSize / 4, dPlacerSize / 4 );
			}
		}

		/// <summary>
		/// Requests a redraw of the overlay (selection, drag box, placer).
		/// Replaces direct InvalidateVisual calls that targeted the old Render override.
		/// </summary>
		private void InvalidateOverlay()
		{
			EnsureOverlay();
			m_overlay.InvalidateVisual();
		}

		// ─────────────────────────────────────────────────
		//  MeasureOverride / ArrangeOverride
		// ─────────────────────────────────────────────────

		protected override Size MeasureOverride( Size szAvailable )
		{
			Size			szResult		= new Size( 0, 0 );

			foreach( var child in Children )
			{
				child.Measure( szAvailable );
				szResult = new Size(
					Math.Max( szResult.Width, child.DesiredSize.Width ),
					Math.Max( szResult.Height, child.DesiredSize.Height ) );
			}

			double			dWidth			= double.IsPositiveInfinity( szAvailable.Width ) ? szResult.Width : szAvailable.Width;
			double			dHeight			= double.IsPositiveInfinity( szAvailable.Height ) ? szResult.Height : szAvailable.Height;

			szResult = new Size( dWidth, dHeight );

			double			dHeightMin		= GetHeightMinimum();

			szResult = new Size( szResult.Width, Math.Max( szResult.Height, dHeightMin ) );

			return szResult;
		}

		protected override Size ArrangeOverride( Size szFinal )
		{
			// Arrange the ruler at the top
			if( m_pnlRuler != null )
			{
				m_pnlRuler.Arrange( new Rect( 0, 0, szFinal.Width, N_RULER_TOP ) );
			}

			// Arrange lane children at their stored positions
			foreach( PnlMapLane pnlLane in m_lstLane )
			{
				double		dLeft			= pnlLane.LaneLeftOffset;
				double		dTop			= pnlLane.LaneTopOffset;
				double		dWidth			= pnlLane.LaneWidth;
				double		dHeight			= pnlLane.LaneHeightActual;

				if( dWidth > 0 && dHeight > 0 )
				{
					pnlLane.Arrange( new Rect( dLeft, dTop, dWidth, dHeight ) );
				}
				else
				{
					pnlLane.Arrange( new Rect( 0, 0, 0, 0 ) );
				}
			}

			// Arrange the overlay on top of everything (full panel size)
			if( m_overlay != null )
			{
				m_overlay.Arrange( new Rect( 0, 0, szFinal.Width, szFinal.Height ) );
			}

			// Arrange any other children
			foreach( var child in Children )
			{
				if( child != m_pnlRuler && child != m_overlay && !m_lstLane.Contains( child as PnlMapLane ) )
				{
					child.Arrange( new Rect( 0, 0, child.DesiredSize.Width, child.DesiredSize.Height ) );
				}
			}

			// Trigger initial DoUpdateView once we have real bounds
			if( m_bNeedsInitialUpdate && szFinal.Width > 0 && szFinal.Height > 0 && GetCountLane() > 0 )
			{
				m_bNeedsInitialUpdate = false;
				Avalonia.Threading.Dispatcher.UIThread.Post( () => DoUpdateView() );
			}

			return szFinal;
		}

		// ─────────────────────────────────────────────────
		//  Pointer Events (mouse → pointer)
		// ─────────────────────────────────────────────────

		private void OnPointerWheelChanged( object obj, PointerWheelEventArgs ea )
		{
			var				props			= ea.KeyModifiers;

			if( props.HasFlag( KeyModifiers.Meta ) )
			{
				var doc = m_docMap as MetaScope.Views.DocMap;
				if( doc != null )
				{
					if( ea.Delta.Y > 0 )
						doc.DoPanelZoomIn();
					else
						doc.DoPanelZoomOut();
				}

				ea.Handled						= true;
			}
			else if( props.HasFlag( KeyModifiers.Shift ) )
			{
				var doc = m_docMap as MetaScope.Views.DocMap;
				if( doc != null )
				{
					if( ea.Delta.Y > 0 )
						doc.DoPanelScrollLeft();
					else
						doc.DoPanelScrollRight();
				}

				ea.Handled						= true;
			}
		}

		private void OnPointerMoved( object obj, PointerEventArgs ea )
		{
			if( ea.GetCurrentPoint( this ).Properties.IsLeftButtonPressed )
			{
				if( m_bDragBox == true )
				{
					m_ptDragEnd		= ea.GetPosition( this );

					m_dDragBoxWidth		= Math.Abs( m_ptDragEnd.X - m_ptDragStart.X );
					m_dDragBoxHeight	= Math.Abs( m_ptDragEnd.Y - m_ptDragStart.Y );

					m_dDragBoxX			= Math.Min( m_ptDragStart.X, m_ptDragEnd.X );
					m_dDragBoxY			= Math.Min( m_ptDragStart.Y, m_ptDragEnd.Y );

					m_bDragBoxVisible	= true;
					InvalidateOverlay();
				}
				else if( m_bDragLane == true )
				{
					m_ptDragEnd		= ea.GetPosition( this );

					int				nIndex			= GetLanePlacerPosition();
					if( nIndex >= 0 && nIndex < GetCountLane() )
					{
						PnlMapLane		pnl				= GetLane( nIndex );
						double			dLaneY			= pnl.LaneTopOffset;

						m_bPlacerVisible				= true;
						m_dPlacerY						= dLaneY - N_LANE_VERTICALGAP / 2;
						InvalidateOverlay();
					}
					else if( nIndex == GetCountLane() )
					{
						PnlMapLane		pnl				= GetLane( nIndex - 1 );
						double			dLaneY			= pnl.LaneTopOffset;
						double			dLaneH			= pnl.Bounds.Height;

						m_bPlacerVisible				= true;
						m_dPlacerY						= dLaneY + dLaneH + N_LANE_VERTICALGAP / 2;
						InvalidateOverlay();
					}
				}
			}
		}

		private void OnPointerReleased( object obj, PointerReleasedEventArgs ea )
		{
			if( m_bDragBox == true )
			{
				m_bDragBoxVisible				= false;
				InvalidateOverlay();

				if( m_dDragBoxWidth <= 1.0f )
				{
					m_bDragBox					= false;
					return;
				}

				if( ListLaneEditable.Count == 0 )
				{
					ErrorMessage.ShowErrorSelectLaneFirst();
				}
				else
				{
					double			dTop			= m_dDragBoxY;
					double			dBottom			= m_dDragBoxY + m_dDragBoxHeight;

					int				nPosStart		= UtilityMath.DoRound( GetPositionFromPixel( m_dDragBoxX ) );
					int				nPosEnd			= UtilityMath.DoRound( GetPositionFromPixel( m_dDragBoxX + m_dDragBoxWidth ) );

					bool			bCareScore		= true;
					if( IsSelectByPositionFunc != null )
						bCareScore					= !IsSelectByPositionFunc();

					foreach( PnlMapLane pnl in m_lstLaneEditable )
					{
						double		dLaneY			= pnl.LaneTopOffset;
						double		dLaneW			= pnl.Bounds.Width;
						double		dLaneH			= pnl.Bounds.Height;

						Rect		rtBox			= new Rect( m_dDragBoxX, m_dDragBoxY, m_dDragBoxWidth, m_dDragBoxHeight );
						Rect		rtLane			= new Rect( pnl.LaneLeftOffset, dLaneY, dLaneW, dLaneH );

						if( rtBox.Intersects( rtLane ) )
						{
							if( bCareScore == true )
							{
								double		dScoreTop		= pnl.GetScoreFromYOffset( dLaneY - dTop );
								double		dScoreBottom	= pnl.GetScoreFromYOffset( dBottom - dLaneY );

								pnl.DoFeatureSelect( nPosStart, nPosEnd, dScoreTop, dScoreBottom );
							}
							else
							{
								pnl.DoFeatureSelect( nPosStart, nPosEnd );
							}
						}
					}
				}

				m_bDragBox						= false;
			}
			else if( m_bDragLane == true )
			{
				m_bPlacerVisible				= false;
				InvalidateOverlay();

				m_bDragLane						= false;

				int				nIndex			= GetLanePlacerPosition();
				if( nIndex != -1 )
					DoLaneMoveSelectedAfter( nIndex );
			}
		}

		private void OnPointerPressed( object obj, PointerPressedEventArgs ea )
		{
			if( !ea.GetCurrentPoint( this ).Properties.IsLeftButtonPressed )
				return;

			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT, Bounds.Height );
			Point			pt				= ea.GetPosition( this );

			if( rtHead.Contains( pt ) == true )
			{
				m_bDragBox		= false;
				m_bDragLane		= true;

				m_ptDragStart	= ea.GetPosition( this );
				m_ptDragEnd		= ea.GetPosition( this );
			}
			else
			{
				m_bDragBox		= true;
				m_bDragLane		= false;

				m_ptDragStart	= ea.GetPosition( this );
				m_ptDragEnd		= ea.GetPosition( this );

				m_dDragBoxWidth		= 0.0f;
				m_dDragBoxHeight	= 0.0f;
				m_bDragBoxVisible	= false;
			}
		}

		// ─────────────────────────────────────────────────
		//  Lane mouse enter/leave (hover effect)
		// ─────────────────────────────────────────────────

		protected void OnLaneMouseLeave( object obj, PointerEventArgs ea )
		{
			// Lane back opacity effect — in Avalonia this is handled
			// via lane hover state; no separate Rectangle child needed
		}

		protected void OnLaneMouseEnter( object obj, PointerEventArgs ea )
		{
			// Lane back opacity effect — in Avalonia this is handled
			// via lane hover state; no separate Rectangle child needed
		}
	}

	/// <summary>
	/// Transparent overlay control that renders on top of all PnlMap children.
	/// Draws the selection highlight (gotcha #3), drag box, and placer indicator.
	/// This exists because Avalonia's Panel.Render() is sealed.
	/// </summary>
	internal class PnlMapOverlay : Control
	{
		private		PnlMap							m_pnlMap;

		public PnlMapOverlay( PnlMap pnlMap )
		{
			m_pnlMap					= pnlMap;
			IsHitTestVisible			= false;
		}

		public override void Render( DrawingContext dc )
		{
			base.Render( dc );
			m_pnlMap.RenderOverlay( dc );
		}
	}
}
