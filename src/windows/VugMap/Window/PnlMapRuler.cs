using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using VugMap.Utility.Data;
using VugMap.Utility.Logger;

namespace VugMap.Window
{
	class PnlMapRuler : Panel
	{
		// Ruler		
		private		Pen								m_penRuler						= null;
		private		Brush							m_brsRulerText					= null;
		private		Typeface						m_tfRulerText					= null;	
		private		Point							m_ptRulerTextOrigin				= new Point( 0, 0 );
		private		RotateTransform					m_rtRulerText					= null;	
		private		TranslateTransform				m_ttRulerText					= null;

		public PnlMapRuler()
		{
			BuildElementRuler();
		}

		private void OnRenderRulerDrawLine( DrawingContext dc, Pen pen, Point pt0, Point pt1 )
		{
			GuidelineSet	gs				= new GuidelineSet();
			gs.GuidelinesX.Add( pt0.X + 0.5 );
			gs.GuidelinesX.Add( pt1.X + 0.5 );

			gs.GuidelinesY.Add( pt0.Y + 0.5 );
			gs.GuidelinesY.Add( pt1.Y + 0.5 );
						
			dc.PushGuidelineSet( gs );
			dc.DrawLine( pen, pt0, pt1 );
			dc.Pop();
		}

		protected override void OnRender( DrawingContext dc )
		{	
			base.OnRender( dc );	

			PnlMap			pnlMap			= Parent as PnlMap;
			Debug.Assert( pnlMap != null );

			Debug.Assert( pnlMap.Zoom >= 0.0f );
			if( pnlMap.LaneList.Count != 0 && pnlMap.Zoom != 0.0f )
			{				
				OnRenderRuler( dc );			
			}			 
		}
				
		private void OnRenderRuler( DrawingContext dc )
		{			
			PnlMap			pnlMap			= Parent as PnlMap;
			Debug.Assert( pnlMap != null );

			dc.PushClip( new RectangleGeometry( new Rect( 0.0f, 0.0f, ActualWidth, ActualHeight ) ) );

			double			dWidth			= ActualWidth;
			Point			ptBaseStart		= new Point( 0, PnlMap.N_RULER_TOP );
			Point			ptBaseEnd		= new Point( dWidth, PnlMap.N_RULER_TOP );
			
			OnRenderRulerDrawLine( dc, m_penRuler, ptBaseStart, ptBaseEnd );			

			double			dWidthRuler		= dWidth - PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - PnlMapLane.N_LANE_MARGIN;
			double			dWidthPosition	= ( ( double ) ( pnlMap.PositionMax - pnlMap.PositionMin ) ) / pnlMap.Zoom;
			double			dWidthPerPix	= dWidthPosition / dWidthRuler;
			int				nRulerUnit		= GetRulerUnit( dWidthPerPix );
			double			dRulerUnit		= pnlMap.GetPixelFromPosition( nRulerUnit ) - PnlMap.N_RULER_LEFT;
			int				nPosDispMax		= pnlMap.Position + UtilityMath.DoRound( pnlMap.PositionRange / pnlMap.Zoom );

			// 첫번째 눈금, m_nPosition
			Point			ptNotch0Start	= new Point( PnlMap.N_RULER_LEFT, PnlMap.N_RULER_TOP );
			Point			ptNotch0End		= new Point( PnlMap.N_RULER_LEFT, PnlMap.N_RULER_TOP - PnlMap.N_RULER_NOTCHHEIGHT );
			OnRenderRulerDrawLine( dc, m_penRuler, ptNotch0Start, ptNotch0End );			
			OnRenderRulterText( dc, ptNotch0End, pnlMap.Position );

			// 마지막
			Point			ptNotch1Start	= new Point( PnlMap.N_RULER_LEFT + dWidthRuler, PnlMap.N_RULER_TOP );
			Point			ptNotch1End		= new Point( PnlMap.N_RULER_LEFT + dWidthRuler, PnlMap.N_RULER_TOP - PnlMap.N_RULER_NOTCHHEIGHT );
			OnRenderRulerDrawLine( dc, m_penRuler, ptNotch1Start, ptNotch1End );			
			OnRenderRulterText( dc, ptNotch1End, nPosDispMax );
			
			int				nNotchCount		= UtilityMath.DoRound( dWidthPosition / nRulerUnit ) + 1;
			int				nNotchStart		= ( pnlMap.Position / nRulerUnit + 1 ) * nRulerUnit;			
			for( int i = 0; i < nNotchCount; i++ )
			{
				int				nOffset			= nNotchStart + nRulerUnit * i;
				double			dOffset			= pnlMap.GetPixelFromPosition( nOffset );

				Point			ptNochStart		= new Point( dOffset, PnlMap.N_RULER_TOP );
				Point			ptNotchEnd		= new Point( dOffset, PnlMap.N_RULER_TOP - PnlMap.N_RULER_NOTCHHEIGHT );

				if( ( dOffset - PnlMap.N_RULER_LEFT ) < 15.0f )
				{
					continue;
				}
				else if( PnlMap.N_RULER_LEFT + dWidthRuler - dOffset < 15.0f )
				{
					continue;
				}

				OnRenderRulerDrawLine( dc, m_penRuler, ptNochStart, ptNotchEnd );			
				OnRenderRulterText( dc, ptNotchEnd, nOffset );			
			}

			dc.Pop();
		}

		private void OnRenderRulterText( DrawingContext dc, Point ptOrigin, int nText )
		{
			string			strText			= string.Format( "{0:N0}", nText );

			FormattedText	ftNotch			= new FormattedText( strText,
																 CultureInfo.GetCultureInfo( "en-us" ), FlowDirection.LeftToRight,
																 m_tfRulerText, PnlMap.N_RULERTEXT_FONTSIZE, m_brsRulerText,
																 VisualTreeHelper.GetDpi( this ).PixelsPerDip );
			
			m_ttRulerText.X					= ptOrigin.X - PnlMap.N_RULERTEXT_WIDTH;
			m_ttRulerText.Y					= ptOrigin.Y - PnlMap.N_RULERTEXT_HEIGHT;

			dc.PushTransform( new TranslateTransform( ptOrigin.X - PnlMap.N_RULERTEXT_WIDTH, ptOrigin.Y - PnlMap.N_RULERTEXT_HEIGHT ) );
			dc.PushTransform( m_rtRulerText );
			
			dc.DrawText( ftNotch, m_ptRulerTextOrigin );
			
			dc.Pop();
			dc.Pop();
		}

		private		static int[]	S_NUNITA		= { 1, 10, 50, 100, 500, 1000, 5000, 10000, 50000, 100000, 500000 };

		private int GetRulerUnit( double dWidthPerPix )
		{
			double			dSpan			= dWidthPerPix * PnlMap.N_RULER_TIPSPAN;
			
			foreach( int nUnit in S_NUNITA )
			{
				if( dSpan <= nUnit )
				{
					return nUnit;
				}
			}

			return 1000000;
		}

		private void BuildElementRuler()
		{			
			m_penRuler		= new Pen( Brushes.Black, 1.0f );
			m_brsRulerText	= Brushes.Black;
			m_tfRulerText	= new Typeface( "calibri" );
			m_rtRulerText	= new RotateTransform( -45.0f );
			m_ttRulerText	= new TranslateTransform();
			
			m_penRuler.Freeze();
			m_brsRulerText.Freeze();
			m_rtRulerText.Freeze();			
		}
	}
}
