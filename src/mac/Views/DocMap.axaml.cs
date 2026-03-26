using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

using MetaScope.Controls;
using MetaScope.Models;
using MetaScope.Services;

namespace MetaScope.Views
{
	public partial class DocMap : UserControl
	{
		//			.								.								.
		private		string							m_strSequenceId					= null;
		private		bool							m_bSplit						= false;
		private		double							m_dLastMapWidth					= 0;
		private		double							m_dLastMapAltWidth				= 0;
		private		PnlMap							m_pnlFocus						= null;
		private		string							m_strTitle						= null;

		public DocMap()
		{
			InitializeComponent();

			m_pnlMap.DocMap					= this;
			m_pnlMapAlt.DocMap				= this;
			m_pnlFocus						= m_pnlMap;

			m_scbScroll.ValueChanged		+= OnScroll;
			m_scvMap.PropertyChanged		+= OnMapSizeChanged;
			m_scvMapAlt.PropertyChanged		+= OnMapAltSizeChanged;
			m_gsDoc.PointerMoved			+= OnGridSplitterMouseMove;

			AddHandler( DragDrop.DropEvent, OnDrop );

			// Subscribe to scrollbar update requests from PnlMap
			MetaScope.Controls.PnlMap.OnScrollSetRequested += ( docMapObj ) =>
			{
				if( docMapObj == this )
					DoScrollSet();
			};
		}

		public string Title
		{
			get {	return m_strTitle; }
			set {	m_strTitle = value; }
		}

		public bool IsSplitted
		{
			get {	return m_bSplit; }
		}

		public void DoSplitSet( bool bSplit )
		{
			if( m_bSplit != bSplit )
			{
				m_bSplit		= bSplit;

				if( m_bSplit == true )
				{
					PanelMapAlt.IsVisible				= true;
					PanelMapAlt.DoLaneLayoutCopy( PanelMap );

					double				dHeight			= ( GridDoc.Bounds.Height - GridSplitterDoc.Bounds.Height ) / 2;

					GridDoc.RowDefinitions[ 0 ].Height	= new GridLength( dHeight );
					GridDoc.RowDefinitions[ 1 ].Height	= new GridLength( 5 );
					GridDoc.RowDefinitions[ 2 ].Height	= new GridLength( dHeight );

					m_gsDoc.IsVisible					= true;
				}
				else
				{
					double				dHeight			= m_grdMap.Bounds.Height - m_scbScroll.Bounds.Height;

					GridDoc.RowDefinitions[ 0 ].Height	= new GridLength( dHeight );
					GridDoc.RowDefinitions[ 1 ].Height	= new GridLength( 0 );
					GridDoc.RowDefinitions[ 2 ].Height	= new GridLength( 0 );

					PanelMapAlt.IsVisible				= false;
					m_gsDoc.IsVisible					= false;
				}
			}
		}

		private void OnGridSplitterMouseMove( object obj, PointerEventArgs ea )
		{
			if( m_gsDoc.IsPointerOver && ea.GetCurrentPoint( m_gsDoc ).Properties.IsLeftButtonPressed )
			{
				Point			pt0				= ea.GetPosition( m_grdDoc );
				Point			pt1				= ea.GetPosition( m_gsDoc );

				double			dHeight			= m_grdMap.Bounds.Height - m_scbScroll.Bounds.Height;
				double			dHeight0		= pt0.Y - pt1.Y;
				double			dHeight1		= dHeight - dHeight0 - m_gsDoc.Bounds.Height;

				if( dHeight0 > 0 && dHeight1 > 0 )
				{
					GridDoc.RowDefinitions[ 0 ].Height	= new GridLength( dHeight0 );
					GridDoc.RowDefinitions[ 2 ].Height	= new GridLength( dHeight1 );
				}
			}
		}

		public Grid GridDoc
		{
			get {	return m_grdDoc; }
		}

		public GridSplitter GridSplitterDoc
		{
			get {	return m_gsDoc; }
		}

		public PnlMap PanelMap
		{
			get {	return m_pnlMap; }
		}

		public PnlMap PanelMapAlt
		{
			get {	return m_pnlMapAlt; }
		}

		public PnlMap PanelActive
		{
			get {	return m_pnlFocus; }
		}

		private LayoutTransformControl GetLayoutTransformControl( PnlMap pnl )
		{
			if( pnl == m_pnlMap )
				return m_ltcMap;
			if( pnl == m_pnlMapAlt )
				return m_ltcMapAlt;
			return null;
		}

		public void DoPanelScaleDown()
		{
			DoPanelScaleDown( m_pnlMap );
			if( m_bSplit == true )
				DoPanelScaleDown( m_pnlMapAlt );
		}

		private void DoPanelScaleDown( PnlMap pnl )
		{
			LayoutTransformControl	ltc		= GetLayoutTransformControl( pnl );
			ScaleTransform		stDoc			= ltc?.LayoutTransform as ScaleTransform;
			Debug.Assert( stDoc != null );

			double			dX				= stDoc.ScaleX / 0.9;
			double			dY				= stDoc.ScaleY / 0.9;

			stDoc.ScaleX	= dX;
			stDoc.ScaleY	= dY;
		}

		public void DoPanelScaleUp()
		{
			DoPanelScaleUp( m_pnlMap );
			if( m_bSplit == true )
				DoPanelScaleUp( m_pnlMapAlt );
		}

		private void DoPanelScaleUp( PnlMap pnl )
		{
			LayoutTransformControl	ltc		= GetLayoutTransformControl( pnl );
			ScaleTransform		stDoc			= ltc?.LayoutTransform as ScaleTransform;
			Debug.Assert( stDoc != null );

			double			dX				= stDoc.ScaleX * 0.9;
			double			dY				= stDoc.ScaleY * 0.9;

			stDoc.ScaleX	= dX;
			stDoc.ScaleY	= dY;
		}

		public void DoLaneRemove( DataFile df )
		{
			m_pnlMap.DoLaneRemove( df );
			if( m_bSplit == true )
				m_pnlMapAlt.DoLaneRemove( df );
		}

		public void DoUpdateView()
		{
			m_pnlMap.DoUpdateView();
			if( m_bSplit == true )
				m_pnlMapAlt.DoUpdateView();
		}

		public void DoClose()
		{
			m_strSequenceId					= null;
			PanelMap.DoClose();
		}

		public void SetSequenceId( string strSequenceId )
		{
			m_strSequenceId	= strSequenceId;

			Title			= m_strSequenceId;
		}

		public string GetSequenceId()
		{
			return m_strSequenceId;
		}

		public string SequenceId
		{
			get {	return GetSequenceId(); }
			set {	SetSequenceId( value ); }
		}

		protected void OnFocus( object obj, RoutedEventArgs ea )
		{
		}

		public void DoPanelZoomSet( double dZoom )
		{
			m_pnlMap.Zoom					= dZoom;
			if( m_bSplit == true )
				m_pnlMapAlt.Zoom				= dZoom;
		}

		public void SetPanelPosition( int nPosition )
		{
			m_pnlMap.SetPosition( nPosition );
			if( m_bSplit == true )
				m_pnlMapAlt.SetPosition( nPosition );
		}

		public void DoPanelSelection( double dStart, double dWidth )
		{
			m_pnlMap.DoSelection( dStart, dWidth );
			if( m_bSplit == true )
				m_pnlMapAlt.DoSelection( dStart, dWidth );
		}

		public void DoPanelLaneShow( string strType )
		{
			m_pnlMap.DoLaneShow( strType );
			if( m_bSplit == true )
				m_pnlMapAlt.DoLaneShow( strType );
		}

		public void DoPanelZoomTo( double dZoom )
		{
			m_pnlMap.DoZoomTo( dZoom );
			if( m_bSplit == true )
				m_pnlMapAlt.DoZoomTo( dZoom );
		}

		public void DoPanelZoomIn()
		{
			m_pnlMap.DoZoomIn();
			if( m_bSplit == true )
				m_pnlMapAlt.DoZoomIn();
		}

		public void DoPanelZoomOut()
		{
			m_pnlMap.DoZoomOut();
			if( m_bSplit == true )
				m_pnlMapAlt.DoZoomOut();
		}

		public void DoPanelPositionTo( int nPosition )
		{
			m_pnlMap.SetPosition( nPosition );
			m_pnlMap.DoUpdateView();

			if( m_bSplit == true )
			{
				m_pnlMapAlt.SetPosition( nPosition );
				m_pnlMapAlt.DoUpdateView();
			}
		}

		public void DoPanelScrollLeft()
		{
			m_pnlMap.DoScrollLeft();
			if( m_bSplit == true )
				m_pnlMapAlt.DoScrollLeft();
		}

		public void DoPanelScrollRight()
		{
			m_pnlMap.DoScrollRight();
			if( m_bSplit == true )
				m_pnlMapAlt.DoScrollRight();
		}

		public void DoPanelScrollLeftSmall()
		{
			m_pnlMap.DoScrollLeftSmall();
			if( m_bSplit == true )
				m_pnlMapAlt.DoScrollLeftSmall();
		}

		public void DoPanelScrollRightSmall()
		{
			m_pnlMap.DoScrollRightSmall();
			if( m_bSplit == true )
				m_pnlMapAlt.DoScrollRightSmall();
		}

		public void DoPanelUpdateView()
		{
			m_pnlMap.DoUpdateView();
			if( m_bSplit == true )
				m_pnlMapAlt.DoUpdateView();
		}

		public void DoPanelLaneRemove()
		{
			m_pnlMap.DoLaneRemove();
			if( m_bSplit == true )
				m_pnlMapAlt.DoLaneRemove();
		}

		public void DoPanelLaneAdd( string[] strFileA )
		{
			m_pnlMap.DoLaneAdd( strFileA );
			if( m_bSplit == true )
				m_pnlMapAlt.DoLaneAdd( strFileA );
		}

		public void DoPanelLaneAdd( DataFile df )
		{
			m_pnlMap.DoLaneAdd( df );
			if( m_bSplit == true )
				m_pnlMapAlt.DoLaneAdd( df );
		}

		public void DoPanelLaneRemoveall()
		{
			m_pnlMap.DoLaneRemoveAll();
			if( m_bSplit == true )
				m_pnlMapAlt.DoLaneRemoveAll();
		}

		public void DoFileUpdate()
		{
			MainWindow		mw				= MainWindow.GetMainWindow( this );
			mw.DoExplorerUpdate();
		}

		public void DoScrollSet()
		{
			if( m_pnlMap.LaneList.Count == 0 )
			{
				m_scbScroll.IsEnabled		= false;
			}
			else
			{
				m_scbScroll.IsEnabled		= true;

				// Setting ViewportSize
				double			dTrackLength	= m_scbScroll.Maximum - m_scbScroll.Minimum;
				double			dThumbLength	= dTrackLength / m_pnlMap.Zoom;
				double			dViewportSize	= 0.0f;

				if( dTrackLength < 0 )
				{
					dViewportSize	= 0.0f;
				}
				else if( dThumbLength < dTrackLength )
				{
					dViewportSize	= dTrackLength * dThumbLength / ( dTrackLength - dThumbLength );
				}
				else
				{
					dViewportSize	= double.MaxValue;
				}

				m_scbScroll.ViewportSize		= dViewportSize;

				// Force Track to recalculate thumb size immediately
				var track = m_scbScroll.GetVisualDescendants()
					.OfType<Avalonia.Controls.Primitives.Track>().FirstOrDefault();
				if( track != null )
				{
					track.InvalidateMeasure();
					track.InvalidateArrange();
				}

				double			dScrollRange	= m_scbScroll.Maximum - m_scbScroll.Minimum;
				int				nPosRange		= UtilityMath.DoRound(
													( m_pnlMap.PositionMax - m_pnlMap.PositionRange / m_pnlMap.Zoom ) - m_pnlMap.PositionMin );

				if( nPosRange == 0 )
				{
					// Initial state
					double			dScrollValue	= 0;
					m_scbScroll.Value				= dScrollValue;

					Logger.PrintLine( "# DocMap:DoScrollSet - {0}", dScrollValue );
				}
				else
				{
					// PosRange - PosRange / Zoom : ScrollRange = (Pos - Min ) : Scroll.Value - Scroll.Min
					// Scroll.Value = ScrollRange * ( Pos - Min ) / ( PosRange - PosRange / Zoom ) + Scroll.Min
					double			dScrollValue	= dScrollRange * ( m_pnlMap.Position - m_pnlMap.PositionMin )
														  / nPosRange + m_scbScroll.Minimum;

					double			dChangeSmall	= dScrollRange / ( m_pnlMap.Zoom - 1 ) / 4;
					double			dChangeLarge	= dScrollRange / ( m_pnlMap.Zoom - 1 );

					m_scbScroll.Value				= dScrollValue;
					m_scbScroll.SmallChange			= dChangeSmall;
					m_scbScroll.LargeChange			= dChangeLarge;

					Logger.PrintLine( "# DocMap:DoScrollSet - {0}, {1}, {2}", dScrollValue, dChangeSmall, dChangeLarge );
				}
			}
		}

		public void DoScrollPosition( double dValue )
		{
			double				dValueMax		= m_scbScroll.Maximum;

			if( dValue > dValueMax )
			{
				m_scbScroll.Value			= dValueMax;
			}
			else
			{
				int				nPosition		= UtilityMath.DoRound(
													( dValue - m_scbScroll.Minimum ) / ( m_scbScroll.Maximum - m_scbScroll.Minimum )
													* ( m_pnlMap.PositionRange - m_pnlMap.PositionRange / m_pnlMap.Zoom ) + m_pnlMap.PositionMin );

				SetPanelPosition( nPosition );
				DoPanelUpdateView();
			}

			m_pnlMap.DoSelection();
			if( m_bSplit == true )
				m_pnlMapAlt.DoSelection();
		}

		protected void OnDrop( object obj, DragEventArgs ea )
		{
			var				data			= ea.Data.GetFiles();
			if( data != null )
			{
				var				strFileA	= data.Select( f => f.Path.LocalPath ).ToArray();
				DoDrop( strFileA );
			}

			ea.Handled						= true;
		}

		public void DoDrop( string[] strFileA )
		{
			MainWindow		mw				= MainWindow.GetMainWindow( this );
			mw.DoDrop( strFileA );
		}

		protected void OnLoaded( object obj, RoutedEventArgs ea )
		{
		}

		private void OnScroll( object obj, RangeBaseValueChangedEventArgs ea )
		{
			double			dValue			= ea.NewValue;

			Logger.PrintLine( "# DocMap:OnScroll - {0}", dValue );

			DoScrollPosition( dValue );
		}

		private void OnMapSizeChanged( object obj, AvaloniaPropertyChangedEventArgs ea )
		{
			if( ea.Property == BoundsProperty )
			{
				m_pnlMap.Height				= Math.Max( m_pnlMap.GetHeightMinimum(), m_scvMap.Bounds.Height );

				double dWidth				= m_scvMap.Bounds.Width;
				if( dWidth > 0 && Math.Abs( dWidth - m_dLastMapWidth ) > 0.5 )
				{
					m_dLastMapWidth			= dWidth;
					m_pnlMap.Width			= dWidth;
					m_pnlMap.DoUpdateView();
					DoScrollSet();
				}
			}
		}

		private void OnMapAltSizeChanged( object obj, AvaloniaPropertyChangedEventArgs ea )
		{
			if( ea.Property == BoundsProperty )
			{
				m_pnlMapAlt.Height			= Math.Max( m_pnlMapAlt.GetHeightMinimum(), m_scvMapAlt.Bounds.Height );

				double dWidth				= m_scvMapAlt.Bounds.Width;
				if( m_bSplit && dWidth > 0 && Math.Abs( dWidth - m_dLastMapAltWidth ) > 0.5 )
				{
					m_dLastMapAltWidth		= dWidth;
					m_pnlMapAlt.Width		= dWidth;
					m_pnlMapAlt.DoUpdateView();
				}
			}
		}
	}
}
