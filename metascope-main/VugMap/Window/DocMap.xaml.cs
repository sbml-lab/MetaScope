using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using AvalonDock;
using VugMap.Utility;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;

namespace VugMap.Window
{	
	public partial class DocMap : DocumentContent
	{
		//			.								.								.		
		private		string							m_strSequenceId					= null;
		private		bool							m_bSplit						= false;
		private		PnlMap							m_pnlFocus						= null;

		public DocMap()
		{
			InitializeComponent();

			m_pnlMap.DocMap					= this;
			m_pnlMapAlt.DocMap				= this;
			m_pnlFocus						= m_pnlMap;
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
					PanelMapAlt.Visibility				= Visibility.Visible;
					PanelMapAlt.DoLaneLayoutCopy( PanelMap );

					double				dHeight			= ( GridDoc.ActualHeight - GridSplitterDoc.ActualHeight ) / 2;

					GridDoc.RowDefinitions[ 0 ].Height	= new GridLength( dHeight );					
					//GridDoc.RowDefinitions[ 0 ].Height	= GridLength.Auto;
					GridDoc.RowDefinitions[ 1 ].Height	= new GridLength( 5 );
					GridDoc.RowDefinitions[ 2 ].Height	= new GridLength( dHeight );
					//GridDoc.RowDefinitions[ 2 ].Height	= GridLength.Auto;					
					
					m_gsDoc.Visibility					= Visibility.Visible;
				}
				else
				{
					double				dHeight			= m_grdMap.ActualHeight - m_scbScroll.ActualHeight;

					GridDoc.RowDefinitions[ 0 ].Height	= new GridLength( dHeight );
					GridDoc.RowDefinitions[ 1 ].Height	= new GridLength( 0 );
					GridDoc.RowDefinitions[ 2 ].Height	= new GridLength( 0 );

					PanelMapAlt.Visibility				= Visibility.Collapsed;
					m_gsDoc.Visibility					= Visibility.Collapsed;
				}								
			}
		}

		private void OnGridSplitterMouseMove( object obj, MouseEventArgs ea )
		{
			if( m_gsDoc.IsMouseCaptureWithin )
			{
				Point			pt0				= ea.GetPosition( m_grdDoc );
				Point			pt1				= ea.GetPosition( m_gsDoc );

				double			dHeight			= m_grdMap.ActualHeight - m_scbScroll.ActualHeight;
				double			dHeight0		= pt0.Y - pt1.Y;
				double			dHeight1		= dHeight - dHeight0 - m_gsDoc.ActualHeight;

				GridDoc.RowDefinitions[ 0 ].Height	= new GridLength( dHeight0 );
				GridDoc.RowDefinitions[ 2 ].Height	= new GridLength( dHeight1 );
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

		public void DoPanelScaleDown()
		{
			DoPanelScaleDown( m_pnlMap );
			if( m_bSplit == true )
				DoPanelScaleDown( m_pnlMapAlt );
		}

		private void DoPanelScaleDown( PnlMap pnl )
		{
			ScaleTransform		stDoc			= pnl.LayoutTransform as ScaleTransform;
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
			ScaleTransform		stDoc			= pnl.LayoutTransform as ScaleTransform;
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
			MainWindow.GetMainWindow().DoExplorerUpdate();
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

				double			dScrollRange	= m_scbScroll.Maximum - m_scbScroll.Minimum;
				int				nPosRange		= UtilityMath.DoRound( 
													( m_pnlMap.PositionMax - m_pnlMap.PositionRange / m_pnlMap.Zoom ) - m_pnlMap.PositionMin );

				if( nPosRange == 0 )
				{
					// 처음
					double			dScrollValue	= 0;
					m_scbScroll.Value				= dScrollValue;	
			
					Logger.PrintLine( "# DocMap:DoScrollSet - {0}", dScrollValue );			
				}
				else
				{
					// PosRange - PosRnage / Zoom : ScrollRange = (Pos - Min ) : Scroll.Value - Scroll.Min
					// Scroll.Value = ScrollRange * ( Pos - Min ) / ( PosRange - PosRange / Zoom ) + Scrol.Min
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
			string[]		strFileA		= ( string[] ) ea.Data.GetData( DataFormats.FileDrop, false );

			DoDrop( strFileA );

			ea.Handled						= true;
		}

		public void DoDrop( string[] strFileA )
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			mw.DoDrop( strFileA );
		}

		protected void OnLoaded( object obj, RoutedEventArgs ea )
		{			
		}

		private void OnScroll( object obj, System.Windows.Controls.Primitives.ScrollEventArgs ea )
		{
			double			dValue			= ea.NewValue;

			Logger.PrintLine( "# DocMap:OnScroll - {0}", dValue );

			DoScrollPosition( dValue );
		}

		private void OnMapSizeChanged( object obj, SizeChangedEventArgs ea )
		{
			m_pnlMap.Height					= Math.Max( m_pnlMap.GetHeightMinimum(), ea.NewSize.Height );
		}		

		private void OnMapAltSizeChanged( object obj, SizeChangedEventArgs ea )
		{
			m_pnlMapAlt.Height				= Math.Max( m_pnlMapAlt.GetHeightMinimum(), ea.NewSize.Height );
		}		
	}
}
