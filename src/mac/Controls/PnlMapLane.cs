using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;

using MetaScope.Models;
using MetaScope.Services;

namespace MetaScope.Controls
{
	using			ListRect						= List< Rect >;
	using			ListFeature						= List< DataFeature >;
	using			ListListFeature					= List< List< DataFeature > >;
	using			DicRectFeature					= Dictionary< Rect, DataFeature >;
	using			ListDataType					= List< DataType >;
	using			ListDataFeatureNode				= LinkedListNode< DataFeature >;

	public class PnlMapLane : Control
	{
		//			.								.								.
		public		static double					N_SCOREVERTICAL_GAP				= 10.0f;
		public		static double					N_LANE_MARGIN					= 10.0f;
		public		static int						N_FEATURE_COUNT					= 1024 * 2;
		public		static double					N_FEATURE_MINIMALGAP			= 1.0f;
		public		static double					N_FEATURE_MINIMALWIDTH			= 0.5f;
		public		static double					N_FEATURE_MINIMALHEIGHT			= 0.5f;
		public		static double					N_LANE_WIDTHMINIMUM				= 200.0f;
		public		static double					N_LANE_HEIGHTMINIMUM			= 50.0f;

		public		static Color					CLR_BACK						= Color.FromRgb( 255, 255, 255 );
		public		static Color					CLR_BACKSELECTED				= Color.FromRgb( 64, 64, 64 );
		public		static Color					CLR_SELECTED					= Color.FromRgb( 0, 10, 23 );
		public		static Color					CLR_HEADSELECTED				= Color.FromRgb( 139, 178, 255 );
		public		static Color					CLR_TYPEBACK					= Color.FromArgb( 255, 237, 247, 255 );
		public		static Color					CLR_TYPEBACKSELECTED			= Color.FromArgb( 225, 209, 235, 255 );

		private		int								m_nPositionMax					= 0;
		private		int								m_nPositionMin					= 0;
		private		int								m_nPosDispMax					= 0;
		private		int								m_nPosDispMin					= 0;

		private		double							m_dLaneWidth					= 0.0f;
		private		double							m_dLaneHeight					= 0.0f;
		private		double							m_dLaneHeightActual				= 0.0f;
		private		ListDataType					m_lstDataType					= null;
		private		DataType						m_dtSelected					= null;
		private		bool							m_bEditable						= false;
		private		bool							m_bSelected						= false;

		private		Rect							m_rtClip						= default;
		private		IBrush							m_bshBack						= null;
		private		IPen							m_penBack						= null;
		private		IBrush							m_bshSelect						= null;
		private		IBrush							m_bshHeadSelected				= null;
		private		IPen							m_penLine						= null;
		private		IPen							m_penScoreSub					= null;
		private		Typeface						m_tfScore						= Typeface.Default;

		private		IBrush							m_brsTypeBack					= null;
		private		IBrush							m_brsTypeBackSelected			= null;
		private		Point							m_ptRrightClick;

		private		ListFeature						m_lstFeatSelected				= null;
		private		DicRectFeature					m_dicRectFeature				= null;
		private		DataFeature						m_dfToolTip						= null;

		// --- Context menu fields ---
		private		ContextMenu						m_cmHead						= null;
		private		ContextMenu						m_cmBack						= null;
		private		ContextMenu						m_cmFeature						= null;

		public PnlMapLane()
		{
			m_lstDataType					= new ListDataType();

			BuildElementBack();
			BuildElementLine();
			BuildElementType();
			BuildElementSelect();
			BuildElementFeature( N_FEATURE_COUNT );

			PointerMoved	+= OnPointerMoved;
			PointerPressed	+= OnPointerPressed;
			PointerReleased	+= OnPointerReleased;

			BuildContextMenus();
		}

		private void BuildContextMenus()
		{
			// Head context menu (right-click on lane header)
			m_cmHead = new ContextMenu();
			m_cmHead.Items.Add( new MenuItem { Header = "Select to Edit", Command = new MetaScope.Services.RelayCommand( DoHeadSelectToEditClick ) } );
			m_cmHead.Items.Add( new MenuItem { Header = "Select All Features", Command = new MetaScope.Services.RelayCommand( DoHeadSelectAllClick ) } );
			m_cmHead.Items.Add( new Separator() );
			m_cmHead.Items.Add( new MenuItem { Header = "Set Color", Command = new MetaScope.Services.RelayCommand( DoHeadSetColorClick ) } );
			m_cmHead.Items.Add( new MenuItem { Header = "Set Height", Command = new MetaScope.Services.RelayCommand( DoHeadSetHeightClick ) } );
			m_cmHead.Items.Add( new Separator() );

			var mniDisplay = new MenuItem { Header = "Display" };
			mniDisplay.Items.Add( new MenuItem { Header = "Bar", Command = new MetaScope.Services.RelayCommand( DoHeadDisplayBox ) } );
			mniDisplay.Items.Add( new MenuItem { Header = "Point", Command = new MetaScope.Services.RelayCommand( DoHeadDisplayPoint ) } );
			mniDisplay.Items.Add( new MenuItem { Header = "Line", Command = new MetaScope.Services.RelayCommand( DoHeadDisplayLine ) } );
			mniDisplay.Items.Add( new MenuItem { Header = "Stack", Command = new MetaScope.Services.RelayCommand( DoHeadDisplayStack ) } );
			m_cmHead.Items.Add( mniDisplay );

			m_cmHead.Items.Add( new MenuItem { Header = "Manual Scale", Command = new MetaScope.Services.RelayCommand( DoHeadManualScaleClick ) } );
			m_cmHead.Items.Add( new MenuItem { Header = "Change Type", Command = new MetaScope.Services.RelayCommand( DoHeadChangeTypeClick ) } );
			m_cmHead.Items.Add( new Separator() );
			m_cmHead.Items.Add( new MenuItem { Header = "Hide Lane", Command = new MetaScope.Services.RelayCommand( DoHeadHideClick ) } );
			m_cmHead.Items.Add( new MenuItem { Header = "Close File", Command = new MetaScope.Services.RelayCommand( DoHeadCloseClick ) } );
			m_cmHead.Items.Add( new Separator() );

			var mniTrackOp = new MenuItem { Header = "Track Operations" };
			mniTrackOp.Items.Add( new MenuItem { Header = "Average", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionAverageClick ) } );
			mniTrackOp.Items.Add( new MenuItem { Header = "Difference", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionDiffClick ) } );
			mniTrackOp.Items.Add( new MenuItem { Header = "Summation", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionSumClick ) } );
			mniTrackOp.Items.Add( new MenuItem { Header = "Merge", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionMergeClick ) } );
			mniTrackOp.Items.Add( new MenuItem { Header = "Filter", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionFilterClick ) } );
			mniTrackOp.Items.Add( new MenuItem { Header = "Adjust", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionAdjustClick ) } );
			mniTrackOp.Items.Add( new MenuItem { Header = "Assign ID", Command = new MetaScope.Services.RelayCommand( DoHeadOpeartionAssignIdClick ) } );
			m_cmHead.Items.Add( mniTrackOp );

			var mniFeatOp = new MenuItem { Header = "Feature Operations" };
			mniFeatOp.Items.Add( new MenuItem { Header = "Copy", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpCopyClick ) } );
			mniFeatOp.Items.Add( new MenuItem { Header = "Move", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpMoveClick ) } );
			mniFeatOp.Items.Add( new MenuItem { Header = "Merge", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpMergeClick ) } );
			mniFeatOp.Items.Add( new MenuItem { Header = "Filter", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpFilterClick ) } );
			m_cmHead.Items.Add( mniFeatOp );

			var mniIntegration = new MenuItem { Header = "Integration" };
			mniIntegration.Items.Add( new MenuItem { Header = "pORF", Command = new MetaScope.Services.RelayCommand( DoHeadIntegrationPorfClick ) } );
			mniIntegration.Items.Add( new MenuItem { Header = "RTS", Command = new MetaScope.Services.RelayCommand( DoHeadIntegrationRtsClick ) } );
			mniIntegration.Items.Add( new MenuItem { Header = "TU", Command = new MetaScope.Services.RelayCommand( DoHeadIntegrationTuClick ) } );
			mniIntegration.Items.Add( new MenuItem { Header = "TRN", Command = new MetaScope.Services.RelayCommand( DoHeadIntegrationTrnClick ) } );
			m_cmHead.Items.Add( mniIntegration );

			// Back context menu (right-click on empty lane area)
			m_cmBack = new ContextMenu();
			var miBackAdd = new MenuItem { Header = "Add", Command = new MetaScope.Services.RelayCommand( DoFeatureAddClick ) };
			var miBackBookmark = new MenuItem { Header = "Bookmark", Command = new MetaScope.Services.RelayCommand( DoBookmarkAddClick ) };
			m_cmBack.Items.Add( miBackAdd );
			m_cmBack.Items.Add( new Separator() );
			m_cmBack.Items.Add( miBackBookmark );
			m_cmBack.Opening += ( s, e ) =>
			{
				miBackAdd.IsEnabled = m_bEditable;
			};

			// Feature context menu (right-click on a feature)
			m_cmFeature = new ContextMenu();
			var miFeatBookmark = new MenuItem { Header = "Bookmark", Command = new MetaScope.Services.RelayCommand( DoBookmarkAddClick ) };
			var miFeatUnite = new MenuItem { Header = "Unite", Command = new MetaScope.Services.RelayCommand( () => { var p = Parent as PnlMap; p?.DoLaneFeatureUniteSelected(); } ) };
			var miFeatMerge = new MenuItem { Header = "Merge", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpMergeClick ) };
			var miFeatFilter = new MenuItem { Header = "Filter", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpFilterClick ) };
			var miFeatMove = new MenuItem { Header = "Move", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpMoveClick ) };
			var miFeatCopy = new MenuItem { Header = "Copy", Command = new MetaScope.Services.RelayCommand( DoHeadFeatOpCopyClick ) };
			var miFeatEdit = new MenuItem { Header = "Edit", Command = new MetaScope.Services.RelayCommand( DoFeatureEditClick ) };
			var miFeatDelete = new MenuItem { Header = "Delete", Command = new MetaScope.Services.RelayCommand( DoFeatureDeleteClick ) };
			m_cmFeature.Items.Add( miFeatBookmark );
			m_cmFeature.Items.Add( new Separator() );
			m_cmFeature.Items.Add( miFeatUnite );
			m_cmFeature.Items.Add( miFeatMerge );
			m_cmFeature.Items.Add( miFeatFilter );
			m_cmFeature.Items.Add( miFeatMove );
			m_cmFeature.Items.Add( miFeatCopy );
			m_cmFeature.Items.Add( new Separator() );
			m_cmFeature.Items.Add( miFeatEdit );
			m_cmFeature.Items.Add( miFeatDelete );
			m_cmFeature.Opening += ( s, e ) =>
			{
				if( m_bEditable )
				{
					int nSel = ListFeatureSelected.Count;
					miFeatBookmark.IsEnabled	= true;
					miFeatEdit.IsEnabled		= nSel == 1;
					miFeatUnite.IsEnabled		= nSel >= 2;
					miFeatMerge.IsEnabled		= nSel >= 1;
					miFeatFilter.IsEnabled		= nSel >= 2;
					miFeatMove.IsEnabled		= nSel >= 1;
					miFeatCopy.IsEnabled		= nSel >= 1;
					miFeatDelete.IsEnabled		= nSel >= 1;
				}
				else
				{
					miFeatBookmark.IsEnabled	= true;
					miFeatEdit.IsEnabled		= false;
					miFeatUnite.IsEnabled		= false;
					miFeatMerge.IsEnabled		= false;
					miFeatFilter.IsEnabled		= false;
					miFeatMove.IsEnabled		= false;
					miFeatCopy.IsEnabled		= false;
					miFeatDelete.IsEnabled		= false;
				}
			};
		}

		public PnlMapLane( DataType dtLane )
			: this()
		{
			m_lstDataType.Add( dtLane );

			m_dtSelected	= dtLane;
		}

		public ListFeature ListFeatureSelected
		{
			get {	return m_lstFeatSelected; }
		}

		public bool IsContainingDataType( DataType dt )
		{
			bool			b				= m_lstDataType.Contains( dt );

			return b;
		}

		public DataType DoDataTypeGet( int nIndex )
		{
			DataType		dt				= m_lstDataType[ nIndex ];

			return dt;
		}

		public int GetCountDataType()
		{
			int				nCount			= m_lstDataType.Count;

			return nCount;
		}

		public void DoDataTypeRemove( DataType dt )
		{
			m_lstDataType.Remove( dt );

			if( dt == m_dtSelected )
			{
				if( m_lstDataType.Count == 0 )
					m_dtSelected	= null;
				else
					m_dtSelected	= m_lstDataType.Last();
			}
		}

		public void DoDataTypeAdd( DataType dt )
		{
			m_lstDataType.Add( dt );

			m_dtSelected	= dt;
		}

		public void DoDataTypeSelect( string strType )
		{
			foreach( DataType dt in m_lstDataType )
			{
				if( dt.Type == strType )
				{
					DoDataTypeSelect( dt );
				}
			}
		}

		public void DoDataTypeSelect( DataType dt )
		{
			m_dtSelected	= dt;
		}

		public void SetLaneHeightActual( double dHeight )
		{
			dHeight			= Math.Max( dHeight, N_LANE_HEIGHTMINIMUM );

			LaneHeightActual				= dHeight;
		}

		public bool IsSelected
		{
			get {	return m_bSelected; }
			set {	m_bSelected = value; }
		}

		public bool GetIsLayoutUpdated()
		{
			// In immediate-mode rendering, we are always ready to render.
			return true;
		}

		public void DoClose()
		{
			m_lstDataType.Clear();
			m_lstFeatSelected.Clear();
			m_dicRectFeature.Clear();
		}

		public void DoFeatureSelect( DataFeature df )
		{
			if( df != null && m_dtSelected != null && m_dtSelected.IsReadOnly )
				return;

			m_lstFeatSelected.Clear();

			if( df == null )
			{
			}
			else
			{
				m_lstFeatSelected.Add( df );
			}

			MainWindow.GetMainWindow( this )?.DoStatusBarUpdate();
			// Notify via event/delegate once MainWindow is ported.
			OnFeatureSelected?.Invoke( this, df );

			DoLayoutUpdate();
		}

		public int GetCountFeatureSelected()
		{
			int				nCount			= m_lstFeatSelected.Count;

			return nCount;
		}

		public void DoFeatureSelect( int nPosStart, int nPosEnd )
		{
			DoFeatureSelect( nPosStart, nPosEnd, double.MaxValue, double.MinValue );
		}

		public void DoFeatureSelect( int nPosStart, int nPosEnd, double dScoreMax, double dScoreMin )
		{
			if( m_dtSelected != null && m_dtSelected.IsReadOnly )
				return;

			nPosStart		= Math.Max( nPosStart, m_nPosDispMin );
			nPosEnd			= Math.Min( nPosEnd, m_nPosDispMax );

			m_lstFeatSelected.Clear();

			if( m_dtSelected.IsReadOnly )
			{
				DataFeature[]	arr				= m_dtSelected.GetFeatureArray();
				if( arr != null )
				{
					int				iStart			= m_dtSelected.GetFeatureIndexByEnd( nPosStart );
					if( iStart < 0 )		iStart	= 0;

					for( int i = iStart; i < arr.Length; i++ )
					{
						DataFeature		df			= arr[ i ];
						if( df.Start > nPosEnd )	break;
						if( df.Score >= dScoreMin && df.Score <= dScoreMax )
						{
							m_lstFeatSelected.Add( df );
						}
					}
				}
			}
			else
			{
				LinkedListNode< DataFeature >
								node			= m_dtSelected.GetFeatureLinkByEnd( nPosStart );

				while( node != null && node.Value.Start <= nPosEnd )
				{
					if( node.Value.Score >= dScoreMin && node.Value.Score <= dScoreMax )
					{
						m_lstFeatSelected.Add( node.Value );
					}

					node				= node.Next;
				}
			}

			MainWindow.GetMainWindow( this )?.DoStatusBarUpdate();
			OnFeatureListSelected?.Invoke( this, m_lstFeatSelected );

			DoLayoutUpdate();
		}

		private void BuildElementSelect()
		{
			m_bshSelect		= new ImmutableSolidColorBrush( CLR_SELECTED );
		}

		public void DoScale( double dScaleMax, double dScaleMin )
		{
			DataTypeSelected.DoScale( dScaleMax, dScaleMin );

			InvalidateVisual();
		}

		public void DoScaleAuto()
		{
			DataTypeSelected.DoScaleAuto();

			DoLayoutUpdate();
		}

		// =====================================================================
		//  Pointer events (Avalonia equivalents of WPF Mouse events)
		// =====================================================================

		private void OnPointerPressed( object sender, PointerPressedEventArgs ea )
		{
			var				props			= ea.GetCurrentPoint( this ).Properties;

			if( props.IsLeftButtonPressed )
			{
				OnMouseDownLeft( ea );
			}
		}

		private void OnMouseDownLeft( PointerPressedEventArgs ea )
		{
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height );
			Point			pt				= ea.GetPosition( this );

			if( ea.ClickCount == 1 )
			{
				bool			bShift			= ( ea.KeyModifiers & KeyModifiers.Shift ) != 0;
				bool			bCmd			= ( ea.KeyModifiers & ( OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control ) ) != 0;

				if( rtHead.Contains( pt ) )
				{
					// Head area click — handled elsewhere
				}
				else if( bShift )
				{
					// Shift+click: range selection
					var			pnlMap			= Parent as PnlMap;
					if( pnlMap == null ) return;

					Nullable<Rect>	rt				= GetRectangleHit( pt );
					if( rt == null )
						pnlMap.DoSelection();
					else
						pnlMap.DoSelection( rt.Value.X + PnlMap.N_LANE_VERTICALGAP, rt.Value.Width );
				}
				else if( bCmd )
				{
					// Cmd+click (Mac) / Ctrl+click: toggle individual feature in selection
					var			pnlMap			= Parent as PnlMap;
					if( pnlMap == null ) return;

					int				nPos			= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( pt.X + PnlMap.N_LANE_VERTICALGAP ) );
					double			dScoreToggle	= GetScoreFromYOffset( pt.Y );

					DataFeature		df				= null;
					var rtToggle = GetRectangleHit( pt );
					if( rtToggle != null && m_dicRectFeature.ContainsKey( rtToggle.Value ) )
						df = m_dicRectFeature[ rtToggle.Value ];
					if( df == null && m_dtSelected != null )
					{
						df = m_dtSelected.GetFeatureContaining( nPos, dScoreToggle );
					}

					if( df != null )
					{
						if( m_lstFeatSelected.Contains( df ) )
							m_lstFeatSelected.Remove( df );
						else
							m_lstFeatSelected.Add( df );

						OnFeatureListSelected?.Invoke( this, m_lstFeatSelected );
						DoLayoutUpdate();
					}
				}
				else
				{
					// Normal click: single feature selection
					var			pnlMap			= Parent as PnlMap;
					if( pnlMap == null ) return;

					int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( pt.X + PnlMap.N_LANE_VERTICALGAP ) );
					double			dScore			= GetScoreFromYOffset( pt.Y );

					DataFeature		df				= null;

					// First try rect hit test (most reliable — uses rendered positions)
					var rtHit = GetRectangleHit( pt );
					if( rtHit != null && m_dicRectFeature.ContainsKey( rtHit.Value ) )
					{
						df = m_dicRectFeature[ rtHit.Value ];
					}

					// Fallback: score-aware position lookup
					if( df == null && m_dtSelected != null )
					{
						df = m_dtSelected.GetFeatureContaining( nPosition, dScore );
					}

					DoFeatureSelect( df );
					if( df != null )
						pnlMap.DoFeatureHighlightSet( df.Start, df.End );
					else
						pnlMap.DoFeatureHighlightClear();
				}
			}
			else if( ea.ClickCount == 2 )
			{
				if( m_dtSelected != null && m_dtSelected.IsReadOnly )
					return;

				if( rtHead.Contains( pt ) == true )
				{
					dynamic		pnlMap				= Parent;
					if( pnlMap == null ) return;

					pnlMap.DoLaneSetEditable( this );
				}
				else
				{
					dynamic		pnlMap				= Parent;
					if( pnlMap == null ) return;

					pnlMap.DoLaneSetEditable( this );
				}
			}
		}

		private void OnPointerReleased( object sender, PointerReleasedEventArgs ea )
		{
			// In Avalonia, check InitialPressMouseButton for which button was released
			if( ea.InitialPressMouseButton == MouseButton.Left )
			{
				OnMouseUpLeft( ea );
			}
			else if( ea.InitialPressMouseButton == MouseButton.Right )
			{
				OnMouseUpRight( ea );
			}
		}

		private void OnMouseUpRight( PointerReleasedEventArgs ea )
		{
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height );
			Rect			rtBody			= new Rect( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, 0.0f,
												Bounds.Width - (PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP ), Bounds.Height );

			Point			pt				= ea.GetPosition( this );
			m_ptRrightClick					= pt;

			if( rtHead.Contains( pt ) == true )
			{
				dynamic		pnlMap				= Parent;
				if( pnlMap == null ) return;

				pnlMap.DoLaneSelected( this );
				m_cmHead.Open( this );
			}
			else if( rtBody.Contains( pt ) == true )
			{
				Nullable<Rect>	rt				= GetRectangleHit( pt );
				if( rt == null )
				{
					m_cmBack.Open( this );
				}
				else
				{
					m_cmFeature.Open( this );
				}
			}
		}

		private void OnMouseUpLeft( PointerReleasedEventArgs ea )
		{
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height );
			Point			pt				= ea.GetPosition( this );

			bool			bShift			= ( ea.KeyModifiers & KeyModifiers.Shift ) != 0;

			if( bShift )
			{
				if( rtHead.Contains( pt ) == true )
				{
					dynamic		pnlMap				= Parent;
					if( pnlMap == null ) return;

					pnlMap.DoLaneSelectedAdd( this );
				}
				else
				{
				}
			}
			else
			{
				if( rtHead.Contains( pt ) == true )
				{
					dynamic		pnlMap				= Parent;
					if( pnlMap == null ) return;

					pnlMap.DoLaneSelected( this );
				}
				else
				{
				}
			}
		}

		private void OnPointerMoved( object sender, PointerEventArgs ea )
		{
			Point			pt				= ea.GetPosition( this );

			dynamic		pnlMap			= Parent;
			if( pnlMap == null ) return;

			int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( pt.X + PnlMap.N_LANE_VERTICALGAP ) );
			double			dScore			= GetScoreFromYOffset( pt.Y );

			Nullable<Rect>	rt				= GetRectangleHit( pt );
			if( rt == null )
			{
				MainWindow.GetMainWindow( this )?.DoStatusBarUpdate();
				OnFeatureHover?.Invoke( this, null );

				if( m_dfToolTip != null )
				{
					m_dfToolTip					= null;
					ToolTip.SetTip( this, null );
				}
			}
			else
			{
				DataFeature		df				= m_dtSelected.GetFeatureContaining( nPosition, dScore );

				MainWindow.GetMainWindow( this )?.DoStatusBarUpdate();
				OnFeatureHover?.Invoke( this, df );

				if( df != null && df != m_dfToolTip )
				{
					m_dfToolTip					= df;
					ToolTip.SetTip( this, DoToolTipMake( df ) );
					ToolTip.SetShowDelay( this, 200 );
				}
			}
		}

		private Nullable< Rect > GetRectangleHit( Point pt )
		{
			foreach( KeyValuePair<Rect, DataFeature> pv in m_dicRectFeature )
			{
				bool			bContain		= pv.Key.Contains( pt );

				if( bContain == true )
				{
					return pv.Key;
				}
			}

			return null;
		}

		private string DoToolTipMake( DataFeature df )
		{
			StringBuilder	sb				= new StringBuilder();
			sb.AppendFormat( "{0:N0} .. {1:N0}  ({2:N0} bp)", df.Start, df.End, df.End - df.Start + 1 );
			sb.AppendLine();
			sb.AppendFormat( "Score: {0}  Strand: {1}", df.ScoreString, df.Strand );

			string			strAttr			= df.Attribute;
			if( strAttr != null )
			{
				string		strName			= df.DoAttributeGet( "Name" );
				if( strName == null )	strName	= df.DoAttributeGet( "ID" );
				if( strName == null )	strName	= df.DoAttributeGet( "locus_tag" );
				if( strName != null )
				{
					sb.AppendLine();
					sb.Append( strName );
				}
			}

			return sb.ToString();
		}

		public void SetEditable( bool bEditable )
		{
			if( m_bEditable != bEditable )
			{
				m_bEditable		= bEditable;

				DoLayoutUpdate();
			}
		}

		public string GetTypeText()
		{
			StringBuilder	sb				= new StringBuilder();

			foreach( DataType dt in m_lstDataType )
			{
				string			str				= null;

				if( dt == m_dtSelected )
				{
					str				= string.Format( "{0}* ", dt.Type );
				}
				else
				{
					str				= string.Format( "{0} ", dt.Type );
				}

				if( dt == m_lstDataType.Last() )
				{
					sb.Append( string.Format( "{0}", str ) );
				}
				else
				{
					sb.Append( string.Format( "{0}, ", str ) );
				}
			}

			string			strType			= sb.ToString();

			return strType;
		}

		public int GetPositionMax()
		{
			int				nMax			= int.MinValue;

			foreach( DataType dt in m_lstDataType )
			{
				nMax			= Math.Max( nMax, dt.PositionMax );
			}

			return nMax;
		}

		public int GetPositionMin()
		{
			int				nMin			= int.MaxValue;

			foreach( DataType dt in m_lstDataType )
			{
				nMin			= Math.Min( nMin, dt.PositionMin );
			}

			return nMin;
		}

		public double GetScoreMax()
		{
			double			dMax			= double.MinValue;

			foreach( DataType dt in m_lstDataType )
			{
				dMax			= Math.Max( dMax, dt.ScoreMax );
			}

			return dMax;
		}

		public double GetScoreMin()
		{
			double			dMin			= double.MaxValue;

			foreach( DataType dt in m_lstDataType )
			{
				dMin			= Math.Min( dMin, dt.ScoreMin );
			}

			return dMin;
		}

		// =====================================================================
		//  Render override (immediate-mode, replaces WPF OnRender)
		// =====================================================================

		public override void Render( DrawingContext dc )
		{
			m_nPositionMax					= GetPositionMax();
			m_nPositionMin					= GetPositionMin();

			base.Render( dc );

			if( DataTypeSelected == null )
				return;

			if( DataTypeSelected.Display == EDataTypeDisplay.BAR || DataTypeSelected.Display == EDataTypeDisplay.LINE ||
				DataTypeSelected.Display == EDataTypeDisplay.POINT )
			{
				OnRenderLane( dc );
			}
			else if( DataTypeSelected.Display == EDataTypeDisplay.STACK )
			{
				if( DataTypeSelected.StackLayer == 0 )
				{
					DataTypeSelected.BuildStack();
				}

				OnRenderLaneStack( dc );
			}

			foreach( DataType dt in m_lstDataType )
			{
				if( dt == m_lstDataType.First() )
				{
					OnRenderFeature( dc, dt );
				}
				else
				{
					using( dc.PushOpacity( 0.8 ) )
					{
						OnRenderFeature( dc, dt );
					}
				}
			}
		}

		private void OnRenderLaneDrawRectDark( DrawingContext dc, IBrush bsh, IPen pen, Rect rt )
		{
			IBrush			bshDark			= ManagerBrush.GetManager().GetBrushDark( bsh as ISolidColorBrush );

			dc.DrawRectangle( bshDark, pen, rt );
		}

		private void OnRenderLaneDrawRectSolid( DrawingContext dc, IBrush bsh, IPen pen, Rect rt )
		{
			// WPF used GuidelineSet for pixel-snapping. In Avalonia, we draw directly.
			// Lines/rects drawn at integer + 0.5 coordinates with pen width 1.0 are sharp.
			dc.DrawRectangle( bsh, pen, rt );
		}

		private void OnRenderLaneDrawRect( DrawingContext dc, IBrush bsh, IPen pen, Rect rt )
		{
			dc.DrawRectangle( bsh, pen, rt );
		}

		private void OnRenderFeature( DrawingContext dc, DataType dt )
		{
			dynamic		pnlMap			= Parent;
			if( pnlMap == null ) return;

			double			dLeft			= PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP;
			double			dDispWidth		= Bounds.Width - N_LANE_MARGIN - dLeft;
			double			dDispHeight		= Bounds.Height - 2 * N_LANE_MARGIN;
			int				nDispWidth		= UtilityMath.DoRound( dDispWidth );
			int				nPositionWidth	= m_nPosDispMax - m_nPosDispMin;
			double			dPosPerPix		= nPositionWidth / dDispWidth;
			double			dFeatHeight		= m_dLaneHeightActual - 2 * N_LANE_MARGIN;

			double			dScaleMax		= dt.ScaleMax;
			double			dScaleMin		= dt.ScaleMin;
			if( dt.Scale == false )
			{
				dScaleMax		= dt.ScoreMax;
				dScaleMin		= dt.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );

			double			dHeightBase		= GetYOffsetScoreBase();
			double			dHeightPos		= GetYOffsetScoreBase() - N_LANE_MARGIN;
			double			dHeightNeg		= m_dLaneHeightActual - 2 * N_LANE_MARGIN - dHeightPos;
			Rect			rtPrevPos		= new Rect( double.MinValue, double.MinValue, 0.0f, 0.0f );
			Rect			rtPrevNeg		= new Rect( double.MinValue, double.MinValue, 0.0f, 0.0f );
			Nullable< Point >	ptPrevious	= null;

			if( m_dtSelected == dt )
			{
				m_dicRectFeature.Clear();
			}

			m_rtClip						= new Rect( dLeft, N_LANE_MARGIN, dDispWidth, dDispHeight );
			using( dc.PushClip( m_rtClip ) )
			{

			if( dt.Display == EDataTypeDisplay.STACK )
			{
				ListListFeature		lstList		= new ListListFeature();
				ListFeature			lstLeft		= new ListFeature();

				if( dt.IsReadOnly )
				{
					DataFeature[]	arr				= dt.GetFeatureArray();
					if( arr != null )
					{
						int				iStart			= dt.GetFeatureIndexByEnd( m_nPosDispMin );
						if( iStart < 0 )	iStart		= 0;

						for( int i = iStart; i < arr.Length; i++ )
						{
							DataFeature		df			= arr[ i ];
							if( df.Start > m_nPosDispMax )	break;
							lstLeft.Add( df );
						}
					}
				}
				else
				{
					ListDataFeatureNode	lnkFirst	= dt.GetFeatureLinkByEnd( m_nPosDispMin );
					ListDataFeatureNode	lnkEnd		= dt.GetFeatureLinkByStart( m_nPosDispMax );
					ListDataFeatureNode	lnk			= lnkFirst;

					while( lnk != null && lnk != lnkEnd )
					{
						lstLeft.Add( lnk.Value );
						lnk				= lnk.Next;
					}
				}

				while( lstLeft.Count > 0 )
				{
					ListFeature		lstCurr			= new ListFeature();
					ListFeature		lstLeft0		= new ListFeature();
					DataFeature		dfCurr			= null;

					foreach( DataFeature df in lstLeft )
					{
						if( dfCurr != null && dfCurr.DoCheckOverlap( df ) == true )
						{
							lstLeft0.Add( df );
						}
						else
						{
							lstCurr.Add( df );

							dfCurr			= df;
						}
					}

					lstList.Add( lstCurr );
					lstLeft			= lstLeft0;
				}

				int			nLayerCount		= dt.Scale == true ? ( int ) ( dt.ScaleMax - dt.ScaleMin + 1 ) : dt.StackLayer;
				int			nLayer			= 0;
				double		dHeightStack	= dDispHeight / nLayerCount;

				foreach( ListFeature lst in lstList )
				{
					if( dt.Scale == true && ( ( nLayer + 1 ) < dScaleMin || ( nLayer + 1 ) > dScaleMax ) )
					{
						continue;
					}

					Nullable< double >	dOffXLast	= null;
					bool				bPrevSelected	= false;

					foreach( DataFeature df in lst )
					{
						double			dOffX			= dDispWidth * ( df.Start - m_nPosDispMin ) / nPositionWidth + dLeft;
						bool			bCurrSelected	= m_lstFeatSelected.Contains( df );

						// CRITICAL: Sub-pixel skip optimization with selection bypass (gotcha #2)
						if( dOffXLast == null )
						{
							dOffXLast		= dOffX;
							bPrevSelected	= bCurrSelected;
						}
						else if( dOffX - dOffXLast < 0.5f && !bCurrSelected && !bPrevSelected )
						{
							continue;
						}
						else
						{
							dOffXLast		= dOffX;
							bPrevSelected	= bCurrSelected;
						}

						if( dOffX > dDispWidth + dLeft )
						{
							continue;
						}

						double			dWidth			= ( df.End - df.Start + 1 ) * dDispWidth / nPositionWidth;
						double			dHeight			= dHeightStack - 1.0f;
						double			dOffY			= dHeightStack * ( nLayerCount - nLayer - 1 ) + N_LANE_MARGIN;

						if( dOffX + dWidth > dDispWidth + dLeft )
						{
							dWidth			= dDispWidth + dLeft - dOffX;
						}

						dWidth			= Math.Max( N_FEATURE_MINIMALWIDTH, dWidth );
						dHeight			= Math.Max( N_FEATURE_MINIMALHEIGHT, dHeight );

						Rect			rt				= new Rect( dOffX, dOffY, dWidth, dHeight );

						if( m_dtSelected == dt && m_dicRectFeature.ContainsKey( rt ) == false )
						{
							m_dicRectFeature.Add( rt, df );
						}

						double			dXPrev			= rtPrevPos.X + rtPrevPos.Width;
						if( dOffX - dXPrev < N_FEATURE_MINIMALGAP && (double)pnlMap.Zoom >= (double)pnlMap.ZoomThreshould )
						{
							rt = new Rect( rt.X + N_FEATURE_MINIMALGAP, rt.Y, rt.Width, rt.Height );
						}

						rtPrevPos		= rt;

						IBrush		bshFeature		= df.ColorBrush;

						if( bshFeature == null )
							bshFeature					= dt.DoBrushGet();

						Rect		rtDraw			= rt;

						if( m_lstFeatSelected.Contains( df ) == true )
						{
							// Selected
							OnRenderLaneDrawRectDark( dc, bshFeature, null, rtDraw );
						}
						else
						{
							OnRenderLaneDrawRect( dc, bshFeature, null, rtDraw );
						}
					}

					nLayer++;
				}
			}
			else if( dt.Display == EDataTypeDisplay.POINT || dt.Display == EDataTypeDisplay.BAR ||
					 dt.Display == EDataTypeDisplay.LINE )
			{
				// Collect features in display range
				ListFeature			lstDisp		= new ListFeature();

				if( dt.IsReadOnly )
				{
					DataFeature[]	arr				= dt.GetFeatureArray();
					if( arr != null )
					{
						int				iStart			= dt.GetFeatureIndexByEnd( m_nPosDispMin );
						if( iStart < 0 )	iStart		= 0;

						for( int i = iStart; i < arr.Length; i++ )
						{
							if( arr[ i ].Start > m_nPosDispMax )	break;
							lstDisp.Add( arr[ i ] );
						}
					}
				}
				else
				{
					ListDataFeatureNode	lnkFirst	= dt.GetFeatureLinkByEnd( m_nPosDispMin );
					ListDataFeatureNode	lnkEnd2		= dt.GetFeatureLinkByStart( m_nPosDispMax );
					ListDataFeatureNode	lnk			= lnkFirst;

					while( lnk != null && lnk != lnkEnd2 )
					{
						lstDisp.Add( lnk.Value );
						lnk							= lnk.Next;
					}
				}

				Nullable< double >	dOffXLast	= null;
				bool				bPrevSelected	= false;

				for( int fi = 0; fi < lstDisp.Count; fi++ )
				{
					DataFeature		df				= lstDisp[ fi ];

					if( df == null )				continue;

					double			dOffX			= dDispWidth * ( df.Start - m_nPosDispMin ) / nPositionWidth + dLeft;
					bool			bCurrSelected	= m_lstFeatSelected.Contains( df );

					// CRITICAL: Sub-pixel skip optimization with selection bypass (gotcha #2)
					if( dOffXLast == null )
					{
						dOffXLast		= dOffX;
						bPrevSelected	= bCurrSelected;
					}
					else if( dOffX - dOffXLast < 0.5f && !bCurrSelected && !bPrevSelected )
					{
						continue;
					}
					else
					{
						dOffXLast		= dOffX;
						bPrevSelected	= bCurrSelected;
					}

					if( dOffX > dDispWidth + dLeft )
					{
						continue;
					}

					double			dScore			= Math.Min( Math.Max( df.Score, dScaleMin ), dScaleMax );

					double			dWidth			= ( df.End - df.Start + 1 ) * dDispWidth / nPositionWidth;
					double			dHeight			= ( dScore >= dScaleBase ) ?
														dHeightPos / ( dScaleMax - dScaleBase ) * ( dScore - dScaleBase ) :
														dHeightNeg / ( dScaleBase - dScaleMin ) * ( dScaleBase - dScore );
					double			dOffY			= ( dScore >= dScaleBase ) ?
														dHeightBase - dHeight : dHeightBase;

					if( dOffX + dWidth > dDispWidth + dLeft )
					{
						dWidth			= dDispWidth + dLeft - dOffX;
					}

					dWidth			= Math.Max( N_FEATURE_MINIMALWIDTH, dWidth );
					dHeight			= Math.Max( N_FEATURE_MINIMALHEIGHT, dHeight );

					Rect			rt			= new Rect( dOffX, dOffY, dWidth, dHeight );

					if( m_dtSelected == dt && m_dicRectFeature.ContainsKey( rt ) == false )
					{
						m_dicRectFeature.Add( rt, df );
					}

					if( dScore >= dScaleBase )
					{
						double			dXPrev			= rtPrevPos.X + rtPrevPos.Width;
						if( dOffX - dXPrev < N_FEATURE_MINIMALGAP &&
							(double)pnlMap.Zoom >= (double)pnlMap.ZoomThreshould )
						{
							rt = new Rect( rt.X + N_FEATURE_MINIMALGAP, rt.Y, rt.Width, rt.Height );
						}

						rtPrevPos		= rt;
					}
					else
					{
						double			dXPrev			= rtPrevNeg.X + rtPrevNeg.Width;
						if( dOffX - dXPrev < N_FEATURE_MINIMALGAP &&
							(double)pnlMap.Zoom >= (double)pnlMap.ZoomThreshould )
						{
							rt = new Rect( rt.X + N_FEATURE_MINIMALGAP, rt.Y, rt.Width, rt.Height );
						}

						rtPrevNeg		= rt;
					}

					IBrush		bshFeature		= df.ColorBrush;

					if( bshFeature == null )
						bshFeature					= dt.DoBrushGet();

					Rect		rtDraw			= rt;

					if( dt.Display == EDataTypeDisplay.POINT || dt.Display == EDataTypeDisplay.LINE )
					{
						if( dScore >= dScaleBase )
						{
							rtDraw		= new Rect( rt.X, rt.Y - 1.0f, rt.Width, 2.0f );
						}
						else
						{
							rtDraw		= new Rect( rt.X, rt.Y + rt.Height - 1.0f, rt.Width, 2.0f );
						}
					}

					if( dt.Display == EDataTypeDisplay.LINE )
					{
						if( ptPrevious == null )
						{
							if( dScore >= dScaleBase )
							{
								ptPrevious		= new Point( rt.X + rt.Width / 2, rt.Y );
							}
							else
							{
								ptPrevious		= new Point( rt.X + rt.Width / 2, rt.Y + rt.Height );
							}
						}
						else
						{
							Nullable< Point >	ptCurr		= null;

							if( dScore >= dScaleBase )
							{
								ptCurr			= new Point( rt.X + rt.Width / 2, rt.Y );
							}
							else
							{
								ptCurr			= new Point( rt.X + rt.Width / 2, rt.Y + rt.Height );
							}

							if( m_lstFeatSelected.Contains( df ) == true )
							{
								IPen			penDark			= ManagerPen.GetManager().GetPen(
																ManagerBrush.GetManager().GetBrushDark( bshFeature as ISolidColorBrush ) );

								OnRenderLaneDrawLine( dc, penDark, ptPrevious.Value, ptCurr.Value );
							}
							else
							{
								IPen			pen				= ManagerPen.GetManager().GetPen( bshFeature );

								OnRenderLaneDrawLine( dc, pen, ptPrevious.Value, ptCurr.Value );
							}

							ptPrevious		= ptCurr;
						}
					}

					if( m_lstFeatSelected.Contains( df ) == true )
					{
						// Selected
						OnRenderLaneDrawRectDark( dc, bshFeature, null, rtDraw );
					}
					else
					{
						OnRenderLaneDrawRect( dc, bshFeature, null, rtDraw );
					}
				}
			}

			}  // end using dc.PushClip
		}

		private void OnRenderLaneDrawLineSolid( DrawingContext dc, IPen pen, Point pt0, Point pt1 )
		{
			// WPF used GuidelineSet for pixel-snapping. In Avalonia, draw directly.
			// Half-pixel offsets are already baked into coordinates for sharpness.
			dc.DrawLine( pen, pt0, pt1 );
		}

		private void OnRenderLaneDrawLine( DrawingContext dc, IPen pen, Point pt0, Point pt1 )
		{
			dc.DrawLine( pen, pt0, pt1 );
		}

		private void OnRenderLaneStack( DrawingContext dc )
		{
			dynamic		pnlMap			= Parent;

			Rect			rtBack			= new Rect( 0.0f, 0.0f, Bounds.Width, Bounds.Height );
			Rect			rtHead			= new Rect( 1.0f, PnlMap.N_LANE_VERTICALGAP,
												PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - 1, Bounds.Height - 2 * PnlMap.N_LANE_VERTICALGAP );

			if( m_bEditable == true )
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, m_penBack, rtBack );
			}
			else
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, null, rtBack );
			}

			Point			ptUpper0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			Point			ptUpper1		= new Point( Bounds.Width - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptUpper0, ptUpper1 );

			Point			ptLower0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_LANE_MARGIN );
			Point			ptLower1		= new Point( Bounds.Width - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptLower0, ptLower1 );

			Point			ptVertical0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_SCOREVERTICAL_GAP );
			Point			ptVertical1		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_SCOREVERTICAL_GAP );
			OnRenderLaneDrawLineSolid( dc, m_penLine, ptVertical0, ptVertical1 );

			double			dMax			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMax : DataTypeSelected.StackLayer;
			Point			ptUpper			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneText( dc, ptUpper, dMax );

			double			dMin			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMin : 1.0f;
			Point			ptLower			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_LANE_MARGIN );
			OnRenderLaneText( dc, ptLower, dMin );

			if( m_bSelected == true )
			{
				dc.DrawRectangle( m_bshHeadSelected, null, rtHead );
			}

			// Draw type labels directly via immediate-mode text rendering
			DoTypeFillRender( dc );
		}

		private void OnRenderLane( DrawingContext dc )
		{
			dynamic		pnlMap			= Parent;

			Rect			rtBack			= new Rect( 0.0f, 0.0f, Bounds.Width, Bounds.Height );
			Rect			rtHead			= new Rect( 1.0f, PnlMap.N_LANE_VERTICALGAP,
												PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - 1, Bounds.Height - 2 * PnlMap.N_LANE_VERTICALGAP );

			if( m_bEditable == true )
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, m_penBack, rtBack );
			}
			else
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, null, rtBack );
			}

			Point			ptVertical0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_SCOREVERTICAL_GAP );
			Point			ptVertical1		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_SCOREVERTICAL_GAP );
			OnRenderLaneDrawLineSolid( dc, m_penLine, ptVertical0, ptVertical1 );

			Point			ptUpper0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			Point			ptUpper1		= new Point( Bounds.Width - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptUpper0, ptUpper1 );

			Point			ptLower0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_LANE_MARGIN );
			Point			ptLower1		= new Point( Bounds.Width - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptLower0, ptLower1 );

			double			dYOffsetScore0	= GetYOffsetScoreBase();
			Point			ptBase0			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, dYOffsetScore0 );
			Point			ptBase1			= new Point( Bounds.Width - PnlMap.N_LANE_VERTICALGAP, dYOffsetScore0 );
			OnRenderLaneDrawLineSolid( dc, m_penLine, ptBase0, ptBase1 );

			double			dMax			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMax : Math.Max( 0, DataTypeSelected.ScoreMax );
			Point			ptUpper			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneText( dc, ptUpper, dMax );

			double			dMin			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMin : DataTypeSelected.ScoreMin;
			Point			ptLower			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, Bounds.Height - N_LANE_MARGIN );
			OnRenderLaneText( dc, ptLower, dMin );

			Point			ptBase			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, dYOffsetScore0 );
			if( ptBase.Y >= ptUpper.Y + 8.0f && ptBase.Y <= ptLower.Y - 8.0f )
				OnRenderLaneText( dc, ptBase, 0.0f );

			if( m_bSelected == true )
			{
				dc.DrawRectangle( m_bshHeadSelected, null, rtHead );
			}

			// Draw type labels directly via immediate-mode text rendering
			DoTypeFillRender( dc );
		}

		private void OnRenderLaneText( DrawingContext dc, Point ptOrigin, double dScore )
		{
			string			strText			= string.Format( "{0:F1}", dScore );

			var ft = new FormattedText( strText,
										CultureInfo.GetCultureInfo( "en-us" ),
										FlowDirection.LeftToRight,
										m_tfScore,
										PnlMap.N_RULERTEXT_FONTSIZE,
										Brushes.Black );

			dc.DrawText( ft, new Point( ptOrigin.X - ft.Width - 4.0f, ptOrigin.Y - ft.Height / 2 ) );
		}

		/// <summary>
		/// Immediate-mode rendering of the type labels that were previously WPF Label children
		/// in a StackPanel. Now drawn directly as text in the Render pass.
		/// </summary>
		private void DoTypeFillRender( DrawingContext dc )
		{
			double		dOffX			= PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP;
			double		dOffY			= Height - N_SCOREVERTICAL_GAP + 2.0f;

			if( double.IsNaN( dOffY ) || double.IsInfinity( dOffY ) )
				dOffY = Bounds.Height - N_SCOREVERTICAL_GAP + 2.0f;

			double		dCurrX			= dOffX;

			foreach( DataType dt in m_lstDataType )
			{
				IBrush		bshBack		= ( dt == m_dtSelected ) ? m_brsTypeBackSelected : m_brsTypeBack;
				string		strLabel	= dt.Type;

				var ft = new FormattedText( strLabel,
											CultureInfo.GetCultureInfo( "en-us" ),
											FlowDirection.LeftToRight,
											m_tfScore,
											PnlMap.N_RULERTEXT_FONTSIZE,
											Brushes.Black );

				double		dLabelWidth		= ft.Width + 4.0;
				double		dLabelHeight	= ft.Height;

				// Draw background rect
				Rect		rtLabel			= new Rect( dCurrX, dOffY, dLabelWidth, dLabelHeight );
				dc.DrawRectangle( bshBack, null, rtLabel );

				// Draw left border with type color
				IBrush		bshBorder		= dt.DoBrushGet();
				Rect		rtBorder		= new Rect( dCurrX, dOffY, 2.0, dLabelHeight );
				dc.DrawRectangle( bshBorder, null, rtBorder );

				// Draw label text
				dc.DrawText( ft, new Point( dCurrX + 2.0, dOffY ) );

				dCurrX += dLabelWidth + 4.0;  // gap between labels
			}
		}

		public void DoLayoutUpdate()
		{
			InvalidateVisual();
		}

		public static double LaneMargin
		{
			get
			{
				return N_LANE_MARGIN;
			}
		}

		public int PositionMax
		{
			get
			{
				return m_nPositionMax;
			}

			set
			{
				m_nPositionMax	= value;
			}
		}

		public int PositionMin
		{
			get
			{
				return m_nPositionMin;
			}

			set
			{
				m_nPositionMin	= value;
			}
		}

		public int PositionDisplayMax
		{
			get
			{
				return m_nPosDispMax;
			}

			set
			{
				m_nPosDispMax	= value;
			}
		}

		public int PositionDisplayMin
		{
			get
			{
				return m_nPosDispMin;
			}

			set
			{
				m_nPosDispMin	= value;
			}
		}

		// =====================================================================
		//  Element initialization
		// =====================================================================

		public void BuildElementBack()
		{
			m_rtClip						= default;

			m_bshBack						= new ImmutableSolidColorBrush( CLR_BACK );

			m_penBack						= new Pen( new ImmutableSolidColorBrush( CLR_BACKSELECTED ), 1.0f );

			m_bshHeadSelected				= ManagerBrush.GetManager().GetBrush( CLR_HEADSELECTED, 125 );
		}

		public void BuildElementLine()
		{
			m_penLine			= new Pen( Brushes.Black, 1.0f );

			m_penScoreSub		= new Pen( Brushes.DarkGray, 1.0f );

			m_tfScore			= new Typeface( "Calibri" );
		}

		public void BuildElementType()
		{
			m_brsTypeBack					= new ImmutableSolidColorBrush( CLR_TYPEBACK );
			m_brsTypeBackSelected			= new ImmutableSolidColorBrush( CLR_TYPEBACKSELECTED );

			// In immediate-mode, we no longer create StackPanel/Label children.
			// Type labels are drawn directly in DoTypeFillRender().
		}

		public void BuildElementFeature( int nCount )
		{
			m_dicRectFeature				= new DicRectFeature();
			m_lstFeatSelected				= new ListFeature( nCount );
		}

		// =====================================================================
		//  Score / Y offset calculations
		// =====================================================================

		public double GetYOffsetScoreBase()
		{
			double			dScaleMax		= DataTypeSelected.ScaleMax;
			double			dScaleMin		= DataTypeSelected.ScaleMin;
			if( DataTypeSelected.Scale == false )
			{
				dScaleMax		= DataTypeSelected.ScoreMax;
				dScaleMin		= DataTypeSelected.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );

			double			dYOffset		= ( dScaleMax - dScaleBase ) * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) / ( dScaleMax - dScaleMin )
										+ N_LANE_MARGIN;
			return dYOffset;
		}

		public double GetScoreFromYOffset( double dYOffset )
		{
			double			dScaleMax		= DataTypeSelected.ScaleMax;
			double			dScaleMin		= DataTypeSelected.ScaleMin;
			if( DataTypeSelected.Scale == false )
			{
				dScaleMax		= DataTypeSelected.ScoreMax;
				dScaleMin		= DataTypeSelected.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );
			double			dOffsetBase		= GetYOffsetScoreBase();

			double			dScore			= ( dScaleMax - dScaleMin ) * ( dOffsetBase - dYOffset ) / ( m_dLaneHeightActual - 2 * N_LANE_MARGIN )
											+ dScaleBase;

			return dScore;
		}

		public double GetYOffsetFromScore( double dScore )
		{
			double			dScaleMax		= DataTypeSelected.ScaleMax;
			double			dScaleMin		= DataTypeSelected.ScaleMin;
			if( DataTypeSelected.Scale == false )
			{
				dScaleMax		= DataTypeSelected.ScoreMax;
				dScaleMin		= DataTypeSelected.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );

			double			dYOffset		= -1 * ( dScore - dScaleBase ) * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN )
											/ ( dScaleMax - dScaleMin ) + N_LANE_MARGIN;

			return dYOffset;
		}

		// =====================================================================
		//  Properties
		// =====================================================================

		public ListDataType DataTypeList
		{
			get {	return m_lstDataType; }
		}

		public DataType DataTypeSelected
		{
			get {	return m_dtSelected; }
		}

		public double LaneWidth
		{
			get {	return m_dLaneWidth; }
			set {	m_dLaneWidth	= value; }
		}

		public double LaneHeight
		{
			get {	return m_dLaneHeight; }
			set	{	m_dLaneHeight	= value; }
		}

		public double LaneHeightActual
		{
			get {	return m_dLaneHeightActual; }
			set {	m_dLaneHeightActual	= value; }
		}

		public bool Editable
		{
			get {	return m_bEditable; }
		}

		/// <summary>
		/// Left offset for positioning this lane within PnlMap.
		/// Replaces WPF TranslateTransform.X used in PnlMap layout.
		/// </summary>
		public double LaneLeftOffset { get; set; }

		/// <summary>
		/// Top offset for positioning this lane within PnlMap.
		/// Replaces WPF TranslateTransform.Y used in PnlMap layout.
		/// </summary>
		public double LaneTopOffset { get; set; }

		// =====================================================================
		//  Feature editing operations
		//  These methods manipulate the data model. Dialog interactions are
		//  stubbed with TODO markers until the dialog classes are ported.
		// =====================================================================

		public void DoFeatureUniteSelected( object /* CommandReplace */ cmd )
		{
			if( m_lstFeatSelected.Count == 0 )
			{
				return;
			}

			DataFeature		dfNew			= DataFeature.MakeFeatureByMerge( m_lstFeatSelected );

			ListFeature		lstOld			= new ListFeature();
			lstOld.AddRange( m_lstFeatSelected );

			ListFeature		lstNew			= new ListFeature();
			lstNew.Add( dfNew );

			((dynamic)cmd).DoFeatureAdd( this, lstOld, lstNew );

			m_dtSelected.DoFeatureRemove( m_lstFeatSelected );
			m_dtSelected.DoFeatureAdd( dfNew );

			m_lstFeatSelected.Clear();
			m_lstFeatSelected.Add( dfNew );
		}

		public void DoFeatureDeleteSelected( object /* CommandDelete */ cmd )
		{
			((dynamic)cmd).DoFeatureAdd( this, m_lstFeatSelected, null );

			DoFeatureDelete( m_lstFeatSelected );

			m_lstFeatSelected.Clear();
		}

		public void DoFeatureAdd( DataFeature df )
		{
			MainWindow.GetMainWindow( this )?.DoEditUpdate();
			MainWindow.GetMainWindow( this )?.DoExplorerUpdate();
			m_dtSelected.DoFeatureAdd( df );
			DoLayoutUpdate();
		}

		private void DoFeatureDelete( ListFeature lst )
		{
			m_dtSelected.DoFeatureRemove( lst );

			DoLayoutUpdate();
		}

		public void DoFeatureDeleteMouseOver()
		{
			dynamic		pnlMap			= Parent;
			if( pnlMap == null ) return;

			int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( m_ptRrightClick.X + PnlMap.N_LANE_VERTICALGAP ) );
			double			dScore			= GetScoreFromYOffset( m_ptRrightClick.Y );
			DataFeature		df				= m_dtSelected.GetFeatureContaining( nPosition, dScore );

			ListFeature		lst				= new ListFeature();
			lst.Add( df );

			MainWindow.GetMainWindow( this )?.DoEditUpdate();
			MainWindow.GetMainWindow( this )?.DoExplorerUpdate();
			DoFeatureDelete( lst );
		}

		private async void DoFeatureAddClick()
		{
			if( m_dtSelected == null || m_dtSelected.IsReadOnly )	return;
			var pnlMap = Parent as PnlMap;
			if( pnlMap == null ) return;

			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogFeatureAdd( this );

			// Pre-fill from click position (matching WPF reference DoBackAddClick)
			DataFeature dfFirst = DataTypeSelected.GetFeatureByStart( 0 );
			string strSource = dfFirst == null ? null : dfFirst.Source;
			int nPosition = UtilityMath.DoRound( pnlMap.GetPositionFromPixel( m_ptRrightClick.X + PnlMap.N_LANE_VERTICALGAP ) );
			double dScore = GetScoreFromYOffset( m_ptRrightClick.Y );

			dlg.Source = strSource;
			dlg.Start  = nPosition;
			dlg.End    = nPosition;
			dlg.Score  = dScore;

			// The dialog's OnOkClick handles DoFeatureAdd + DoAutoSaveImmediate + Close.
			await dlg.ShowDialog<bool?>( mw );
		}

		private async void DoBookmarkAddClick()
		{
			var pnlMap = Parent as PnlMap;
			if( pnlMap == null ) return;
			var mw = MainWindow.GetMainWindow( this );

			var dlg = new MetaScope.Views.DialogBookmarkAdd();

			string strTitle = string.Format( "{0}_{1}", "Bookmark", pnlMap.Position );

			dlg.SequenceId		= DataTypeSelected.SequenceId;
			dlg.BookmarkTitle	= strTitle;
			dlg.Position		= pnlMap.Position;
			dlg.Zoom			= pnlMap.Zoom;

			var b = await dlg.ShowDialog<bool?>( mw );
			if( b == true )
			{
				var db = dlg.MakeBookmark();
				var mb = ManagerBookmark.GetManager();
				mb.DoBookmarkAdd( db );
				mw?.DoBookmarkUpdate();
			}
		}

		private async void DoFeatureEditClick()
		{
			if( m_dtSelected == null || m_dtSelected.IsReadOnly )	return;
			var pnlMap = Parent as PnlMap;
			if( pnlMap == null ) return;
			var mw = MainWindow.GetMainWindow( this );

			int nPosition = UtilityMath.DoRound( pnlMap.GetPositionFromPixel( m_ptRrightClick.X + PnlMap.N_LANE_VERTICALGAP ) );
			double dScore = GetScoreFromYOffset( m_ptRrightClick.Y );
			DataFeature df = m_dtSelected.GetFeatureContaining( nPosition, dScore );
			if( df == null ) return;

			var dlg = new MetaScope.Views.DialogFeatureEdit( this );
			dlg.SetFeature( df );
			var b = await dlg.ShowDialog<bool?>( mw );
			if( b == true )
			{
				DataFeature dfEdited = dlg.MakeFeatureEdited();
				if( dfEdited.ColorBrush == null )
					dfEdited.ColorBrush = df.ColorBrush;

				var me = MetaScope.Services.ManagerEdit.GetManager();
				var cmd = me.MakeCommandEdit();
				cmd.DoFeatureAdd( this, df, dfEdited );

				if( mw != null ) mw.Cursor = new Avalonia.Input.Cursor( Avalonia.Input.StandardCursorType.Wait );

				m_dtSelected.DoFeatureRemove( df );
				m_dtSelected.DoFeatureAdd( dfEdited );

				if( mw != null ) mw.Cursor = Avalonia.Input.Cursor.Default;

				mw?.DoEditUpdate();
				DoLayoutUpdate();
				mw?.DoAutoSaveImmediate();
			}
		}

		private void DoFeatureDeleteClick()
		{
			if( m_dtSelected == null || m_dtSelected.IsReadOnly )	return;
			DoFeatureDeleteMouseOver();
			MainWindow.GetMainWindow( this )?.DoAutoSaveImmediate();
		}

		// =====================================================================
		//  Scale / display mode operations
		// =====================================================================

		public void SetScale( double dScaleMax, double dScaleMin )
		{
			DataTypeSelected.ScaleMax		= dScaleMax;
			DataTypeSelected.ScaleMin		= dScaleMin;
			DataTypeSelected.Scale			= true;

			DoLayoutUpdate();
		}

		public void DoFeatureSetColor( Color clr )
		{
			m_dtSelected.DoColorSet( clr );
		}

		public void DoHeadDisplayBox()
		{
			if( DataTypeSelected.Display != EDataTypeDisplay.BAR )
			{
				DataTypeSelected.Display	= EDataTypeDisplay.BAR;
				DoLayoutUpdate();
			}
		}

		public void DoHeadDisplayPoint()
		{
			if( DataTypeSelected.Display != EDataTypeDisplay.POINT )
			{
				DataTypeSelected.Display	= EDataTypeDisplay.POINT;
				DoLayoutUpdate();
			}
		}

		public void DoHeadDisplayLine()
		{
			if( DataTypeSelected.Display != EDataTypeDisplay.LINE )
			{
				DataTypeSelected.Display	= EDataTypeDisplay.LINE;
				DoLayoutUpdate();
			}
		}

		public void DoHeadDisplayStack()
		{
			if( DataTypeSelected.Display != EDataTypeDisplay.STACK )
			{
				DataTypeSelected.Display	= EDataTypeDisplay.STACK;
				DoLayoutUpdate();
			}
		}

		public void DoTypeChange( string strType )
		{
			m_dtSelected.Type				= strType;
			m_dtSelected.IsEdited			= true;

			DoLayoutUpdate();
		}

		public void DoDataTypeRemove()
		{
			ListDataType	lst				= new ListDataType();
			ManagerData		md				= ManagerData.GetManager();

			foreach( DataType dt in m_lstDataType )
			{
				DataType		dtExist			= md.GetDataType( dt.SequenceId, dt.Type );

				if( dtExist == null )
				{
					lst.Add( dt );
				}
			}

			foreach( DataType dt in lst )
			{
				DoDataTypeRemove( dt );
			}
		}

		public void DoDataTypeCloseAll()
		{
			foreach( DataType dt in m_lstDataType )
			{
				dt.DoClose();
			}
		}

		public void DoHeadSelectAllClick()
		{
			SetEditable( true );
			DoFeatureSelect( m_nPositionMin, m_nPositionMax );
		}

		public void DoHeadHideClick()
		{
			dynamic		pnlMap			= Parent;
			if( pnlMap == null ) return;

			pnlMap.DoLaneRemoveSelected();
			pnlMap.DoUpdateView();
		}

		public void DoHeadCloseClick()
		{
			dynamic		pnlMap			= Parent;
			if( pnlMap == null ) return;

			MainWindow.GetMainWindow( this )?.DoExplorerUpdate();
			DoDataTypeCloseAll();
		}

		public void DoHeadSelectToEditClick()
		{
			dynamic		pnlMap			= Parent;
			if( pnlMap == null ) return;

			pnlMap.DoLaneSetEditable( this );
		}

		// =====================================================================
		//  Dialog-dependent operations (stubbed)
		//  These open dialogs that are not yet ported. Each method preserves
		//  the original signature so PnlMap and MainWindow can call them.
		// =====================================================================

		public async void DoHeadSetColorClick()
		{
			var pnlMap = Parent as PnlMap;
			var mw = MainWindow.GetMainWindow( this );

			var dlg = new Avalonia.Controls.Window
			{
				Title = "Choose Color",
				Width = 420, Height = 500,
				WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
			};

			var colorView = new Avalonia.Controls.ColorView
			{
				Color = Avalonia.Media.Colors.Red,
				IsAlphaVisible = false
			};

			var btnOk = new Avalonia.Controls.Button
			{
				Content = "OK",
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
				Padding = new Avalonia.Thickness( 20, 6 )
			};
			bool bOk = false;
			btnOk.Click += ( s, e ) => { bOk = true; dlg.Close(); };

			dlg.Content = new Avalonia.Controls.StackPanel
			{
				Spacing = 10, Margin = new Avalonia.Thickness( 10 ),
				Children = { colorView, btnOk }
			};

			await dlg.ShowDialog( mw );
			if( bOk )
			{
				var hsvClr = colorView.Color;
				var avClr = Avalonia.Media.Color.FromArgb( 255, hsvClr.R, hsvClr.G, hsvClr.B );
				foreach( PnlMapLane pnl in pnlMap.LaneSelected )
				{
					pnl.DoFeatureSetColor( avClr );
					pnl.DoLayoutUpdate();
				}
			}
		}

		public async void DoHeadSetHeightClick()
		{
			var pnlMap = Parent as PnlMap;
			var mw = MainWindow.GetMainWindow( this );

			var dlg = new MetaScope.Views.DialogSetHeight( this );
			dlg.SetElementValue();
			var b = await dlg.ShowDialog<bool?>( mw );
			if( b == true )
			{
				double dHeight = 0.0;
				if( dlg.IsAutomatic == true )
					dHeight = 0.0;
				else
					dHeight = double.Parse( dlg.LaneHeight );

				foreach( PnlMapLane pnl in pnlMap.LaneSelected )
					pnl.LaneHeight = dHeight;

				pnlMap.DoUpdateSize();
			}
		}

		public async void DoHeadManualScaleClick()
		{
			var pnlMap = Parent as PnlMap;
			var mw = MainWindow.GetMainWindow( this );

			var dlg = new MetaScope.Views.DialogSetScale( this );
			dlg.SetNone( DataTypeSelected.ScoreMax, DataTypeSelected.ScoreMin );
			if( DataTypeSelected.Scale )
				dlg.SetManual( DataTypeSelected.ScaleMax, DataTypeSelected.ScaleMin );
			else
				dlg.SetManual( DataTypeSelected.ScoreMax, 0 );

			var b = await dlg.ShowDialog<bool?>( mw );
			if( b == true )
			{
				string strMax, strMin;
				if( dlg.IsNone == true )
				{
					strMax = dlg.NoneMax;
					strMin = dlg.NoneMin;
				}
				else
				{
					strMax = dlg.ManualMax;
					strMin = dlg.ManualMin;
				}

				if( double.TryParse( strMax, out double dScaleMax ) &&
					double.TryParse( strMin, out double dScaleMin ) )
				{
					foreach( PnlMapLane pnl in pnlMap.LaneSelected )
						pnl.SetScale( dScaleMax, dScaleMin );
				}
				else
				{
					MetaScope.Services.Error.ErrorMessage.ShowError( "Scale values must be valid numbers." );
				}
			}
		}

		public async void DoHeadChangeTypeClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogChangeType( this );
			dlg.SetElementValue();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionDiffClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillDifference();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionAverageClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillAverage();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionFilterClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillFilter();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionAdjustClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillAdjust();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionAssignIdClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillAssignId();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionSumClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillSummation();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadOpeartionMergeClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogLaneOperation( this );
			dlg.DoFillMerge();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadFeatOpCopyClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogFeatureOperation( this );
			dlg.DoFillCopy();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadFeatOpMoveClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogFeatureOperation( this );
			dlg.DoFillMove();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadFeatOpMergeClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogFeatureOperation( this );
			dlg.DoFillMerge();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadFeatOpFilterClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogFeatureOperation( this );
			dlg.DoFillFilter();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadIntegrationPorfClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogIntegrationOperation( this );
			dlg.DoFillPorf();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadIntegrationRtsClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogIntegrationOperation( this );
			dlg.DoFillRts();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadIntegrationTuClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogIntegrationOperation( this );
			dlg.DoFillTu();
			await dlg.ShowDialog<bool?>( mw );
		}

		public async void DoHeadIntegrationTrnClick()
		{
			var mw = MainWindow.GetMainWindow( this );
			var dlg = new MetaScope.Views.DialogIntegrationOperation( this );
			dlg.DoFillTrn();
			await dlg.ShowDialog<bool?>( mw );
		}

		// =====================================================================
		//  Layout measurement (Avalonia equivalents of WPF MeasureOverride/ArrangeOverride)
		// =====================================================================

		protected override Size MeasureOverride( Size szAvailable )
		{
			Size			szResult		= new Size( 0, 0 );

			double			dWidth			= double.IsPositiveInfinity( szAvailable.Width ) ? 0 : szAvailable.Width;
			double			dHeight			= double.IsPositiveInfinity( szAvailable.Height ) ? 0 : szAvailable.Height;

			dWidth	= Math.Max( dWidth, N_LANE_WIDTHMINIMUM );
			dHeight	= Math.Max( dHeight, N_LANE_HEIGHTMINIMUM );

			szResult = new Size( dWidth, dHeight );

			return szResult;
		}

		protected override Size ArrangeOverride( Size szFinal )
		{
			return szFinal;
		}

		// =====================================================================
		//  Events / delegates for decoupled communication
		//  These replace direct MainWindow.GetMainWindow( this ) calls.
		// =====================================================================

		/// <summary>
		/// Fired when a single feature is selected (or null for deselection).
		/// </summary>
		public event EventHandler< DataFeature > OnFeatureSelected;

		/// <summary>
		/// Fired when multiple features are selected (range selection).
		/// </summary>
		public event EventHandler< ListFeature > OnFeatureListSelected;

		/// <summary>
		/// Fired when the mouse hovers over a feature (or null when leaving).
		/// </summary>
		public event EventHandler< DataFeature > OnFeatureHover;
	}
}
