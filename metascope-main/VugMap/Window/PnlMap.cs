using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using VugMap.Utility;
using VugMap.Utility.Command;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;

namespace VugMap.Window
{
	using			DicElement						= Dictionary< string, UIElement >;
	using			ListPnlMapLane					= List< PnlMapLane >;
	using			DicRectFeature					= Dictionary< Rectangle, DataFeature >;	
	
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
				
		private		DocMap							m_docMap						= null;
		private		DicElement						m_dicElement					= null;
				
		private		int								m_nPositionMin					= 0;
		private		int								m_nPositionMax					= 0;
		private		int								m_nPosition						= 0;
		private		double							m_dZoom							= 0.0f;

		private		int								m_nHighlightStart				= -1;
		private		int								m_nHighlightEnd					= -1;

		// Ruler
		private		PnlMapRuler						m_pnlRuler						= null;
		
		// Lane
		private		ListPnlMapLane					m_lstLane						= null;
		//private		PnlMapLane						m_pnlEditable					= null;
		private		ListPnlMapLane					m_lstLaneEditable				= null;
		private		ListPnlMapLane					m_lstLaneSelected				= null;		

		// Drag
		private		Point							m_ptDragStart;
		private		Point							m_ptDragEnd;
		private		TranslateTransform				m_ttDragStart					= null;
		private		bool							m_bDragBox						= false;
		private		bool							m_bDragLane						= false;
		private		TranslateTransform				m_ttPlacer						= null;
		
		public PnlMap()
		{
			m_dicElement					= new DicElement();
			m_lstLane						= new ListPnlMapLane();
			m_lstLaneEditable				= new ListPnlMapLane();
			m_lstLaneSelected				= new ListPnlMapLane();

			BuildElementRuler();			
			BuildElementBasic();
			BuildElementDrag();

			Loaded							+= new RoutedEventHandler( OnLoaded );	
			SizeChanged						+= new SizeChangedEventHandler( OnSizeChanged );
			MouseDown						+= new MouseButtonEventHandler( OnMouseDown );
			MouseUp							+= new MouseButtonEventHandler( OnMouseUp );
			MouseMove						+= new MouseEventHandler( OnMouseMove );
			MouseWheel						+= new MouseWheelEventHandler( OnMouseWheel );
		}

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

				pnlLane.RenderTransform			= new TranslateTransform();

				foreach( DataType dt in pnlEach.DataTypeList )
				{
					pnlLane.DoDataTypeAdd( dt );
				}

				pnlLane.LaneHeight			= pnlEach.LaneHeight;				

				DoLaneAdd( pnlLane );
			}

			DoUpdateView();
		}

		public DocMap DocMap
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

		public void DoClose()
		{
			m_dicElement.Clear();

			foreach( PnlMapLane pnl in m_lstLane )
			{
				pnl.DoClose();
			}

			m_lstLane.Clear();
		}

		public int GetCountLaneSelected()
		{
			int				nCount			= m_lstLaneSelected.Count;

			return nCount;
		}

		public ListPnlMapLane LaneSelected
		{
			get {	return m_lstLaneSelected; }
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

		public void DoLaneFeatureUniteSelected()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandReplace	cmd				= me.MakeCommandReplace();

			foreach( PnlMapLane pnl in m_lstLaneEditable )
			{
				pnl.DoFeatureUniteSelected( cmd );
			}

			cmd.DoLaneUpdate();
			mw.DoExplorerUpdate();
			mw.DoEditUpdate();

			mw.DoAutoSaveImmediate();
		}

		public void DoLaneFeatureDeleteSelected()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandDelete	cmd				= me.MakeCommandDelete();

			foreach( PnlMapLane pnl in m_lstLaneEditable )
			{
				pnl.DoFeatureDeleteSelected( cmd );
			}

			mw.DoExplorerUpdate();
			mw.DoEditUpdate();

			mw.DoAutoSaveImmediate();
		}

		public ListPnlMapLane ListLaneEditable
		{
			get {	return m_lstLaneEditable; }
		}	

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

		public void SetPosition( int nPosition )
		{
			int				nMax				= UtilityMath.DoRound( PositionMax - PositionRange / Zoom );
			nPosition							= Math.Min( nPosition, nMax );
			nPosition							= Math.Max( nPosition, PositionMin );

			m_nPosition							= nPosition;
		}

		public int Position
		{
			get {	return m_nPosition; }			
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

		public void DoExportPng( string strPath, double dDpi )
		{
			double			dScale			= dDpi / 96.0;
			RenderTargetBitmap	rtb			= new RenderTargetBitmap(
				( int )( ActualWidth * dScale ), ( int )( ActualHeight * dScale ),
				dDpi, dDpi, PixelFormats.Pbgra32 );
			rtb.Render( this );

			PngBitmapEncoder	enc			= new PngBitmapEncoder();
			enc.Frames.Add( BitmapFrame.Create( rtb ) );
			using( FileStream fs = new FileStream( strPath, FileMode.Create ) )
				enc.Save( fs );
		}

		public void DoExportSvg( string strPath )
		{
			SvgExporter.DoExport( this, ActualWidth, ActualHeight, strPath );
		}

		private void OnMouseWheel( object obj, MouseWheelEventArgs ea )
		{
			if( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
			{
				if( ea.Delta > 0 )
				{
					// Scroll up
					DocMap.DoPanelZoomIn();
				}
				else
				{
					// Scroll down
					DocMap.DoPanelZoomOut();
				}

				ea.Handled						= true;
			}
			else if( Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift ) )
			{
				if( ea.Delta > 0 )
				{
					// Scroll up
					DocMap.DoPanelScrollLeft();
				}
				else
				{
					// Scroll down
					DocMap.DoPanelScrollRight();
				}

				ea.Handled						= true;
			}		
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

			m_nPosition		= Math.Min( m_nPositionMax - nPage , nPosition );

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

		private void OnMouseMove( object obj, MouseEventArgs ea )
		{
			if( ea.LeftButton == MouseButtonState.Pressed )
			{
				if( m_bDragBox == true )
				{
					Rectangle		rt				= m_dicElement[ STR_ELEMENT_DRAGBOX ] as Rectangle;

					m_ptDragEnd		= ea.GetPosition( this );

					rt.Width		= Math.Abs( m_ptDragEnd.X - m_ptDragStart.X );
					rt.Height		= Math.Abs( m_ptDragEnd.Y - m_ptDragStart.Y );

					m_ttDragStart.X	= Math.Min( m_ptDragStart.X, m_ptDragEnd.X );
					m_ttDragStart.Y	= Math.Min( m_ptDragStart.Y, m_ptDragEnd.Y );

					rt.Visibility	= Visibility.Visible;
				}
				else if( m_bDragLane == true )
				{
					Rectangle		rt				= m_dicElement[ STR_ELEMENT_DRAGPLACER ] as Rectangle;

					int				nIndex			= GetLanePlacerPosition();
					if( nIndex >= 0 && nIndex < GetCountLane() )
					{
						PnlMapLane		pnl				= GetLane( nIndex );
						TranslateTransform	tt			= pnl.RenderTransform as TranslateTransform;

						rt.Visibility					= Visibility.Visible;					
						m_ttPlacer.Y					= tt.Y - N_LANE_VERTICALGAP / 2 - rt.Height / 2;

					}
					else if( nIndex == GetCountLane() )
					{
						PnlMapLane		pnl				= GetLane( nIndex - 1 );
						TranslateTransform	tt			= pnl.RenderTransform as TranslateTransform;

						rt.Visibility					= Visibility.Visible;						
						m_ttPlacer.Y					= tt.Y + pnl.ActualHeight + N_LANE_VERTICALGAP / 2 - rt.Height / 2;
					}
				}
			}
		}
				
		private void OnMouseUp( object obj, MouseButtonEventArgs ea )
		{			
			if( m_bDragBox == true )
			{
				Rectangle		rt				= m_dicElement[ STR_ELEMENT_DRAGBOX ] as Rectangle;
			
				rt.Visibility	= Visibility.Collapsed;
			
				if( rt.Width <= 1.0f )
				{
					return;
				}

				if( ListLaneEditable.Count == 0 )
				{	
					ErrorMessage.ShowErrorSelectLaneFirst();
				}
				else
				{					
					MainWindow		mw				= MainWindow.GetMainWindow();

					TranslateTransform	tt			= rt.RenderTransform as TranslateTransform;
					Rect			rtBox			= new Rect( tt.X, tt.Y, rt.Width, rt.Height );

					double			dTop			= tt.Y;
					double			dBottom			= tt.Y + rt.Height;

					int				nPosStart		= UtilityMath.DoRound( GetPositionFromPixel( tt.X ) );
					int				nPosEnd			= UtilityMath.DoRound( GetPositionFromPixel( tt.X + rt.Width ) );

					bool			bCareScore		= !mw.IsSelectByPosition;

					foreach( PnlMapLane pnl in m_lstLaneEditable )
					{
						TranslateTransform	ttLane		= pnl.RenderTransform as TranslateTransform;
						Rect			rtLane			= new Rect( ttLane.X, ttLane.Y, pnl.ActualWidth, pnl.ActualHeight );
						
						if( rtBox.IntersectsWith( rtLane ) == true )
						{
							if( bCareScore == true )
							{
								double			dScoreTop		= pnl.GetScoreFromYOffset( ttLane.Y - dTop );
								double			dScoreBottom	= pnl.GetScoreFromYOffset( dBottom - ttLane.Y );

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
				Rectangle		rt				= m_dicElement[ STR_ELEMENT_DRAGPLACER ] as Rectangle;

				rt.Visibility					= Visibility.Collapsed;

				m_bDragLane						= false;

				int				nIndex			= GetLanePlacerPosition();
				if( nIndex != -1 )
					DoLaneMoveSelectedAfter( nIndex );
			}
		}

		private void OnMouseDown( object obj, MouseButtonEventArgs ea )
		{			
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT, ActualHeight );					
			Point			pt				= ea.GetPosition( this );

			if( rtHead.Contains( pt ) == true )
			{
				m_bDragBox		= false;
				m_bDragLane		= true;

				m_ptDragStart	= Mouse.GetPosition( this );
				m_ptDragEnd		= Mouse.GetPosition( this );
			}
			else
			{
				m_bDragBox		= true;
				m_bDragLane		= false;

				m_ptDragStart	= Mouse.GetPosition( this );
				m_ptDragEnd		= Mouse.GetPosition( this );

				Rectangle		rt				= m_dicElement[ STR_ELEMENT_DRAGBOX ] as Rectangle;
				rt.Width		= 0.0f;
				rt.Height		= 0.0f;
				rt.Visibility	= Visibility.Collapsed;

				Children.Remove( rt );
				Children.Add( rt );				// Z-index up
			}			
		}

		private void BuildElementDrag()
		{
			m_ttDragStart					= new TranslateTransform();

			Rectangle		rtMouseDrag		= new Rectangle();

			rtMouseDrag.Visibility			= Visibility.Collapsed;			
			rtMouseDrag.Stroke				= Brushes.Black;
			rtMouseDrag.StrokeThickness		= 1.0f;
			//rtMouseDrag.Fill				= new SolidColorBrush( Color.FromArgb( 125, 125, 125, 125 ) );						
			rtMouseDrag.RenderTransform		= m_ttDragStart;
			
			Children.Add( rtMouseDrag );

			m_dicElement.Add( STR_ELEMENT_DRAGBOX, rtMouseDrag );
		
			m_ttPlacer						= new TranslateTransform();
			m_ttPlacer.X					= 5.0f;

			Rectangle		rtPlacer		= new Rectangle();

			rtPlacer.Visibility				= Visibility.Collapsed;			
			rtPlacer.Width					= 8.0f;
			rtPlacer.Height					= 8.0f;
			rtPlacer.RadiusX				= 2.0f;
			rtPlacer.RadiusY				= 2.0f;
			rtPlacer.Fill					= Brushes.Gray;
			rtPlacer.Stroke					= Brushes.Gray;
			rtPlacer.StrokeThickness		= 1.0f;			
			rtPlacer.RenderTransform		= m_ttPlacer;
			
			Children.Add( rtPlacer );

			m_dicElement.Add( STR_ELEMENT_DRAGPLACER, rtPlacer );
		}

		protected override Visual GetVisualChild( int nIndex )
		{
			if( nIndex < 0 || nIndex >= Children.Count )
			{
				throw new Exception( "Invalid child index: " + nIndex );
			}

			int				nZindex			= nIndex;
			Visual			vsChild			= Children[ nZindex ];
			
			return vsChild;
		}

		public void DoLaneMoveUp( PnlMapLane pnlLane )
		{
			Debug.Assert( m_lstLane.Contains( pnlLane ) == true );

			if( m_lstLane[ 0 ] == pnlLane )
				return;

			int				nIndex			= m_lstLane.IndexOf( pnlLane );
			
			PnlMapLane		pnlPrev			= m_lstLane[ nIndex - 1 ];
			m_lstLane[ nIndex - 1 ]			= pnlLane;
			m_lstLane[ nIndex ]				= pnlPrev;

			DoUpdateLane();
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

		public void DoLaneUngroup()
		{
			Debug.Assert( m_lstLaneSelected.Count == 1 );
			Debug.Assert( m_lstLaneSelected[ 0 ].GetCountDataType() >1 );

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

		public void DoLaneMoveDown( PnlMapLane pnlLane )
		{
			Debug.Assert( m_lstLane.Contains( pnlLane ) == true );

			if( m_lstLane.Last() == pnlLane )
				return;

			int				nIndex			= m_lstLane.IndexOf( pnlLane );
			
			PnlMapLane		pnlNext			= m_lstLane[ nIndex + 1 ];
			m_lstLane[ nIndex + 1 ]			= pnlLane;
			m_lstLane[ nIndex ]				= pnlNext;

			DoUpdateLane();
		}

		public int GetLanePlacerPosition()
		{
			Point			pt				= Mouse.GetPosition( this );
			List< double >	lst				= new List< double >();

			for( int i = 0; i < GetCountLane(); i++ )
			{
				PnlMapLane		pnl				= GetLane( i );

				TranslateTransform	tt			= pnl.RenderTransform as TranslateTransform;
				if( pt.Y >= tt.Y - N_LANE_VERTICALGAP * 2 && pt.Y <= tt.Y + N_LANE_VERTICALGAP * 2 )
					return i;
				else if( pt.Y >= tt.Y + pnl.ActualHeight - N_LANE_VERTICALGAP * 2 && pt.Y <= tt.Y + pnl.ActualHeight + N_LANE_VERTICALGAP * 2 )
					return i + 1;
			}

			return -1;
		}

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
				if ( dl.LaneHeight != 0.0f )
				{
					dl.SetLaneHeightActual( dl.LaneHeight );
				}
				else
				{
					dl.SetLaneHeightActual( dHeight );
				}				
			}
		}
		
		protected override void OnRender( DrawingContext dc )
		{
			base.OnRender(dc);
			
			foreach( PnlMapLane pnlLane in m_lstLane )
			{
				if( pnlLane.GetIsLayoutUpdated() == false )
				{
					pnlLane.DoLayoutUpdate();
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

		public void DoLaneShow( string strType )
		{
			ManagerData		md				= ManagerData.GetManager();

			DataType		dt				= md.GetDataType( m_docMap.SequenceId, strType );
			
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
			
			string			strSequenceId	= m_docMap.SequenceId;

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
			
			string			strSequenceId	= m_docMap.SequenceId;

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
			for( int i = 0; i < df.GetCountDataType(); i++ )
			{
				DataType		dt			= df.GetDataType( i );

				if( dt.SequenceId == m_docMap.SequenceId )
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

			// 없으면 추가
			PnlMapLane		pnlLane			= new PnlMapLane( dt );

			pnlLane.RenderTransform			= new TranslateTransform();
			pnlLane.Visibility				= Visibility.Visible;
			
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

			// 없으면 추가
			PnlMapLane		pnlLane			= new PnlMapLane( dt );

			pnlLane.RenderTransform			= new TranslateTransform();
			pnlLane.Visibility				= Visibility.Visible;
			
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

			pnl.Visibility					= Visibility.Visible;

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
				
				MainWindow		mw				= MainWindow.GetMainWindow();
				mw.DoDocumentClose( m_docMap );				
			}
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

		private void BuildElementRuler()
		{
			m_pnlRuler						= new PnlMapRuler();
			m_pnlRuler.RenderTransform		= new TranslateTransform();
			m_pnlRuler.Visibility			= Visibility.Visible;			
		
			m_dicElement.Add( STR_PANEL_RULER, m_pnlRuler );
			Children.Add( m_pnlRuler );
		}

		private void BuildElementBasic()
		{
			double			dWidthActual	= ActualWidth;
			double			dHeightActual	= ActualHeight;
			double			dLaneTop		= N_RULER_TOP + N_LANE_VERTICALGAP;
			
			Rectangle		rtLane			= new Rectangle();
			rtLane.Width	= 0.0f;
			rtLane.Height	= 0.0f;
			rtLane.RadiusX	= 10.0f;
			rtLane.RadiusY	= 10.0f;
			rtLane.Fill		= new SolidColorBrush( CLR_LANE_BACK );
			rtLane.RenderTransform			= new TranslateTransform( N_LANE_VERTICALGAP, dLaneTop );			
			rtLane.MouseEnter				+= new MouseEventHandler( OnLaneMouseEnter );
			rtLane.MouseLeave				+= new MouseEventHandler( OnLaneMouseLeave );
			//Children.Add( rtLane );

			m_dicElement.Add( STR_ELEMENT_LANEBACK, rtLane );

			Rectangle		rtSelect		= new Rectangle();
			rtSelect.Width					= 0.0f;
			rtSelect.Height					= 0.0f;
			rtSelect.Fill					= new SolidColorBrush( CLR_LANE_SELECTION );
			rtSelect.RenderTransform		= new TranslateTransform( 0.0f, 0.0f );

			//rtSelect.MouseDown				+= new MouseButtonEventHandler( OnMouseDown );			
			//rtSelect.MouseUp				+= new MouseButtonEventHandler( OnMouseUp );		
			//rtSelect.MouseMove				+= new MouseButtonEventHandler( OnMouseMove );
			rtSelect.MouseDown				+= delegate( object obj, MouseButtonEventArgs ea )
			{
				ea.Handled		= false;								
			};

			rtSelect.MouseUp				+= delegate( object obj, MouseButtonEventArgs ea )
			{
				ea.Handled		= false;								
			};

			Children.Add( rtSelect );
			m_dicElement.Add( STR_ELEMENT_SELECTION, rtSelect );
		}

		void rtSelect_MouseDown()
		{
			throw new NotImplementedException();
		}

		public void DoSelectUpdate( DicRectFeature dic )
		{
			if( m_docMap == null )
				return;
						
			
		}

		public void DoSelection()
		{
			Rectangle		rt				= m_dicElement[ STR_ELEMENT_SELECTION ] as Rectangle;

			rt.Visibility					= Visibility.Collapsed;
		}

		public void DoSelection( double dX, double dWidth )
		{
			Rectangle		rt				= m_dicElement[ STR_ELEMENT_SELECTION ] as Rectangle;

			rt.Width		= dWidth;
			rt.Height		= ActualHeight;

			TranslateTransform	tt			= rt.RenderTransform as TranslateTransform;
			tt.X			= dX;
			
			Children.Remove( rt );
			Children.Add( rt );

			rt.Visibility	= Visibility.Visible;
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

		protected void OnLaneMouseLeave( object obj, MouseEventArgs ea )
		{
			Rectangle		rect			= ( Rectangle ) obj;

			Brush			brs				= rect.Fill;
			brs.Opacity		= 0.5;			
		}

		protected void OnLaneMouseEnter( object obj, MouseEventArgs ea )
		{
			Rectangle		rect			= ( Rectangle ) obj;

			Brush			brs				= rect.Fill;
			brs.Opacity		= 1.0;
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
		
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Logger.PrintLine( "# PnlMap:OnLoaded()" );

			DoUpdateView();
		}

		public void OnSizeChanged( object obj, SizeChangedEventArgs ea )
		{			
			Logger.PrintLine( "# PnlMap:OnSizeChanged()" );

			DoUpdateView();
		}

		public void DoUpdateSize()
		{
			ScrollViewer	sv				= Parent as ScrollViewer;
			
			double			dMinimum		= GetHeightMinimum();
			double			dViewer			= sv.ActualHeight;

			Height							= Math.Max( dMinimum, dViewer );

			if( dMinimum < dViewer )
			{
				DoUpdateView();
			}			
		}

		public void DoUpdateView()
		{
			if( m_dZoom == 0.0f )
				return;

			if( m_docMap == null )
				return;
						
			ManagerData		mgr				= ManagerData.GetManager();
			if( mgr == null )
				return;

			string			strSequenceId	= m_docMap.SequenceId;

			m_nPositionMin	= mgr.GetPositionMin( strSequenceId );
			m_nPositionMax	= mgr.GetPositionMax( strSequenceId );

			double			dWidthActual	= ActualWidth;
			double			dHeightActual	= ActualHeight;

			if( dWidthActual == 0.0f || dHeightActual == 0.0f )
				return;

			DoUpdateRuler();
			DoUpdateLane();
			DoUpdateScrollbar();

			MainWindow		mw				= MainWindow.GetMainWindow();
			if( mw != null )
			{
				mw.DoStatusBarUpdate();
				mw.DoWorkspaceSaveDebounce();
			}
		}

		public void DoUpdateRuler()
		{
			m_pnlRuler.Width				= ActualWidth;
			m_pnlRuler.Height				= N_RULER_TOP;
			m_pnlRuler.InvalidateVisual();
		}

		public void DoUpdateScrollbar()
		{
			if( m_docMap == null )
				return;

			m_docMap.DoScrollSet();		
		}

		public void DoUpdateLane()
		{
			if( m_dZoom == 0.0f )
			{
				// No data
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
			double			dWidthActual	= ActualWidth;
			double			dHeightActual	= ActualHeight;
						
			Rectangle		rt				= m_dicElement[ STR_ELEMENT_DRAGBOX ] as Rectangle;			
			rt.Height						= dHeightActual;
		}

		private void DoUpdateLaneLayout()
		{
			double			dWidthActual	= ActualWidth;
			double			dHeightActual	= ActualHeight;
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

				TranslateTransform	ttLane		= pnlLane.RenderTransform as TranslateTransform;
				ttLane.X						= dLeftEach;
				ttLane.Y						= dTopEach;

				pnlLane.DoLayoutUpdate();

				dTopEach		+= pnlLane.LaneHeightActual + N_LANE_VERTICALGAP;
			}

			DoFeatureHighlightUpdate();
		}

		private void DoUpdateLaneBack()
		{
			double			dWidthActual	= ActualWidth;
			double			dHeightActual	= ActualHeight;
			double			dLaneTop		= N_RULER_TOP + N_LANE_VERTICALGAP;
			double			dWidthLane		= dWidthActual - N_LANE_VERTICALGAP * 2;
			double			dHeightLane		= dHeightActual - dLaneTop - N_LANE_VERTICALGAP;
			
			Rectangle		rtLane			= m_dicElement[ STR_ELEMENT_LANEBACK ] as Rectangle;
			rtLane.Width	= dWidthLane;
			rtLane.Height	= dHeightLane;
		}
	
		private void DoUpdateLaneHeight()
		{
			double			dWidthActual	= ActualWidth;
			double			dHeightActual	= ActualHeight;
			double			dLaneTop		= N_RULER_TOP + N_LANE_VERTICALGAP;
			double			dWidthLane		= dWidthActual - N_LANE_VERTICALGAP * 2;
			double			dHeightLane		= dHeightActual - dLaneTop - N_LANE_VERTICALGAP;

			int				nCountLane		= GetCountLane();
			int				nCountLaneWOH	= GetCountLaneWOHeight();
			double			dHeightSum		= GetLaneHeightSum();
			double			dHeightEach		= ( dHeightLane - dHeightSum - ( nCountLane - 1 ) * N_LANE_VERTICALGAP ) / nCountLaneWOH;

			SetLaneHeight( dHeightEach );
		}

		public double GetPixelFromPosition( double dX )
		{
			double			dWidthRuler		= ActualWidth - PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - PnlMapLane.N_LANE_MARGIN;
			double			dWidthPosition	= ( ( double ) ( m_nPositionMax - m_nPositionMin ) ) / m_dZoom;

			// dX-N_RULER_LEFT : dWidthRuler = dX - nPositionOffset : dWidthPosition
			// ( dX - nPositionOffset ) / dWidthPositino * dWidthRuler + N_RULER_LEFT
			double			dPixel			= ( dX - m_nPosition ) / dWidthPosition * dWidthRuler + N_RULER_LEFT;

			return dPixel;
		}

		public double GetPositionFromPixel( double dX )
		{
			dX				= dX - N_RULER_LEFT;
						
			double			dWidthRuler		= ActualWidth - PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - PnlMapLane.N_LANE_MARGIN;
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
				
		protected override Size MeasureOverride( Size szAvailable )
		{	
			Size			szResult		= new Size( 0,0 );

			foreach( UIElement ue in Children )
			{
				ue.Measure( szAvailable );
				szResult.Width	= Math.Max( szResult.Width, ue.DesiredSize.Width );
				szResult.Height	= Math.Max( szResult.Height, ue.DesiredSize.Height );
			}
			
			szResult.Width	= double.IsPositiveInfinity( szAvailable.Width ) ? szResult.Width : szAvailable.Width;
			szResult.Height = double.IsPositiveInfinity( szAvailable.Height ) ? szResult.Height : szAvailable.Height;

			double			dHeightMin		= GetHeightMinimum();

			szResult.Height	= Math.Max( szResult.Height, dHeightMin );

			return szResult;
		}		
				
		protected override Size ArrangeOverride( Size szFinal )
		{
			foreach( UIElement ue in Children )
			{
				ue.Arrange( new Rect( 0, 0, ue.DesiredSize.Width, ue.DesiredSize.Height ) );
			}

			return szFinal;
		}		
	}
}
