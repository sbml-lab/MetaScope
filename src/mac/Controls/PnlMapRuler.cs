using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using MetaScope.Services;
using MetaScope.Models;

namespace MetaScope.Controls
{
	public class PnlMapRuler : Control
	{
		// Ruler
		private		IPen							m_penRuler						= null;
		private		IBrush							m_brsRulerText					= null;
		private		Typeface						m_tfRulerText					= Typeface.Default;
		private		Point							m_ptRulerTextOrigin				= new Point( 0, 0 );
		private		double							m_dRulerTextRotation			= -45.0;

		private		PnlMap							m_pnlMap						= null;

		public PnlMapRuler()
		{
			BuildElementRuler();
		}

		/// <summary>
		/// Sets or gets the parent PnlMap reference.
		/// </summary>
		public PnlMap MapParent
		{
			get { return m_pnlMap; }
			set { m_pnlMap = value; }
		}

		private void RenderRulerDrawLine( DrawingContext dc, IPen pen, Point pt0, Point pt1 )
		{
			// Avalonia does not have GuidelineSet for pixel-snapping.
			// In Avalonia, lines drawn at integer + 0.5 coordinates with pen width 1.0 are already sharp.
			dc.DrawLine( pen, pt0, pt1 );
		}

		public override void Render( DrawingContext dc )
		{
			base.Render( dc );

			PnlMap		pnlMap			= m_pnlMap;
			if( pnlMap == null )
				return;

			Debug.Assert( pnlMap.Zoom >= 0.0f );
			if( pnlMap.LaneList.Count != 0 && pnlMap.Zoom != 0.0f )
			{
				RenderRuler( dc );
			}
		}

		private void RenderRuler( DrawingContext dc )
		{
			PnlMap		pnlMap			= m_pnlMap;
			if( pnlMap == null )
				return;

			// Avalonia 11: PushClip returns a disposable (replaces WPF dc.Pop())
			using( dc.PushClip( new Rect( 0.0, 0.0, Bounds.Width, Bounds.Height ) ) )
			{
				double			dWidth			= Bounds.Width;
				Point			ptBaseStart		= new Point( 0, PnlMap.N_RULER_TOP );
				Point			ptBaseEnd		= new Point( dWidth, PnlMap.N_RULER_TOP );

				RenderRulerDrawLine( dc, m_penRuler, ptBaseStart, ptBaseEnd );

				double			dWidthRuler		= dWidth - PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - PnlMapLane.N_LANE_MARGIN;
				double			dWidthPosition	= ( ( double ) ( pnlMap.PositionMax - pnlMap.PositionMin ) ) / pnlMap.Zoom;
				double			dWidthPerPix	= dWidthPosition / dWidthRuler;
				int				nRulerUnit		= GetRulerUnit( dWidthPerPix );
				double			dRulerUnit		= pnlMap.GetPixelFromPosition( (double)nRulerUnit ) - PnlMap.N_RULER_LEFT;
				int				nPosDispMax		= pnlMap.Position + UtilityMath.DoRound( pnlMap.PositionRange / pnlMap.Zoom );

				// First notch at m_nPosition
				Point			ptNotch0Start	= new Point( PnlMap.N_RULER_LEFT, PnlMap.N_RULER_TOP );
				Point			ptNotch0End		= new Point( PnlMap.N_RULER_LEFT, PnlMap.N_RULER_TOP - PnlMap.N_RULER_NOTCHHEIGHT );
				RenderRulerDrawLine( dc, m_penRuler, ptNotch0Start, ptNotch0End );
				RenderRulerText( dc, ptNotch0End, pnlMap.Position );

				// Last notch
				Point			ptNotch1Start	= new Point( PnlMap.N_RULER_LEFT + dWidthRuler, PnlMap.N_RULER_TOP );
				Point			ptNotch1End		= new Point( PnlMap.N_RULER_LEFT + dWidthRuler, PnlMap.N_RULER_TOP - PnlMap.N_RULER_NOTCHHEIGHT );
				RenderRulerDrawLine( dc, m_penRuler, ptNotch1Start, ptNotch1End );
				RenderRulerText( dc, ptNotch1End, nPosDispMax );

				int				nNotchCount		= UtilityMath.DoRound( dWidthPosition / nRulerUnit ) + 1;
				int				nNotchStart		= ( pnlMap.Position / nRulerUnit + 1 ) * nRulerUnit;
				for( int i = 0; i < nNotchCount; i++ )
				{
					int				nOffset			= nNotchStart + nRulerUnit * i;
					double			dOffset			= pnlMap.GetPixelFromPosition( (double)nOffset );

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

					RenderRulerDrawLine( dc, m_penRuler, ptNochStart, ptNotchEnd );
					RenderRulerText( dc, ptNotchEnd, nOffset );
				}
			}
		}

		private void RenderRulerText( DrawingContext dc, Point ptOrigin, int nText )
		{
			string			strText			= string.Format( "{0:N0}", nText );

			var				ftNotch			= new FormattedText( strText,
															 CultureInfo.GetCultureInfo( "en-us" ),
															 FlowDirection.LeftToRight,
															 m_tfRulerText,
															 PnlMap.N_RULERTEXT_FONTSIZE,
															 m_brsRulerText );

			double			dTransX			= ptOrigin.X - PnlMap.N_RULERTEXT_WIDTH;
			double			dTransY			= ptOrigin.Y - PnlMap.N_RULERTEXT_HEIGHT;

			using( dc.PushTransform( Matrix.CreateTranslation( dTransX, dTransY ) ) )
			using( dc.PushTransform( Matrix.CreateRotation( m_dRulerTextRotation * Math.PI / 180.0 ) ) )
			{
				dc.DrawText( ftNotch, m_ptRulerTextOrigin );
			}
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
			m_penRuler		= new Pen( Brushes.Black, 1.0 );
			m_brsRulerText	= Brushes.Black;
			m_tfRulerText	= new Typeface( "Calibri" );
			m_dRulerTextRotation = -45.0;
			// Avalonia: Pen and IBrush are lightweight; no Freeze() needed.
		}
	}
}
