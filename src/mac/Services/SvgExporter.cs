using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using Avalonia.Media;

using MetaScope.Controls;
using MetaScope.Models;

namespace MetaScope.Services
{
	/// <summary>
	/// SVG export for MetaScope map panels.
	/// Strategy 3: Generate SVG directly from the data model using the same
	/// layout math as PnlMapLane, bypassing the Avalonia rendering pipeline.
	/// </summary>
	public static class SvgExporter
	{
		public static void DoExport( object visual, double dWidth, double dHeight, string strPath )
		{
			var pnlMap = visual as PnlMap;
			if( pnlMap == null || dWidth <= 0 || dHeight <= 0 )
				return;

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";

			using( XmlWriter xw = XmlWriter.Create( strPath, settings ) )
			{
				xw.WriteStartDocument();
				xw.WriteStartElement( "svg", "http://www.w3.org/2000/svg" );
				xw.WriteAttributeString( "width", DoFormat( dWidth ) );
				xw.WriteAttributeString( "height", DoFormat( dHeight ) );
				xw.WriteAttributeString( "viewBox", $"0 0 {DoFormat( dWidth )} {DoFormat( dHeight )}" );
				xw.WriteAttributeString( "font-family", "-apple-system, Helvetica Neue, sans-serif" );
				xw.WriteAttributeString( "shape-rendering", "geometricPrecision" );
				xw.WriteAttributeString( "style", "stroke-width:0" );

				xw.WriteStartElement( "style" );
				xw.WriteString( "text { font-weight: 300; } line { shape-rendering: crispEdges; }" );
				xw.WriteEndElement();

				// Background
				DoWriteRect( xw, 0, 0, dWidth, dHeight, "white", null );

				// Ruler
				DoWriteRuler( xw, pnlMap, dWidth );

				// Lanes
				DoWriteLanes( xw, pnlMap, dWidth );

				xw.WriteEndElement();
				xw.WriteEndDocument();
			}
		}

		// ─────────────────────────────────────────────────
		//  Ruler
		// ─────────────────────────────────────────────────
		private static void DoWriteRuler( XmlWriter xw, PnlMap pnlMap, double dWidth )
		{
			double dRulerTop = PnlMap.N_RULER_TOP;
			double dRulerLeft = PnlMap.N_RULER_LEFT;
			double dVGap = PnlMap.N_LANE_VERTICALGAP;
			double dLaneMargin = PnlMapLane.N_LANE_MARGIN;
			int nNotchHeight = PnlMap.N_RULER_NOTCHHEIGHT;

			int nPosition = pnlMap.Position;
			int nPosMin = pnlMap.PositionMin;
			int nPosMax = pnlMap.PositionMax;
			double dZoom = pnlMap.Zoom;

			// Match PnlMapRuler.RenderRuler exactly
			double dWidthRuler = dWidth - dRulerLeft - dVGap - dLaneMargin;
			double dWidthPosition = ( double )( nPosMax - nPosMin ) / dZoom;
			double dWidthPerPix = dWidthPosition / dWidthRuler;
			int nRulerUnit = GetRulerUnit( dWidthPerPix );
			if( nRulerUnit <= 0 ) nRulerUnit = 1;
			int nPosDispMax = nPosition + (int)Math.Round( dWidthPosition );

			// Baseline
			DoWriteLine( xw, 0, dRulerTop, dWidth, dRulerTop, "#000000", 0.5 );

			// First notch (at Position)
			DoWriteLine( xw, dRulerLeft, dRulerTop, dRulerLeft, dRulerTop - nNotchHeight, "#000000", 0.5 );
			DoWriteRulerText( xw, dRulerLeft, dRulerTop - nNotchHeight, nPosition );

			// Last notch
			double dLastX = dRulerLeft + dWidthRuler;
			DoWriteLine( xw, dLastX, dRulerTop, dLastX, dRulerTop - nNotchHeight, "#000000", 0.5 );
			DoWriteRulerText( xw, dLastX, dRulerTop - nNotchHeight, nPosDispMax );

			// Intermediate notches
			int nNotchCount = (int)Math.Round( dWidthPosition / nRulerUnit ) + 1;
			int nNotchStart = ( nPosition / nRulerUnit + 1 ) * nRulerUnit;

			for( int i = 0; i < nNotchCount; i++ )
			{
				int nOffset = nNotchStart + nRulerUnit * i;
				double dPixel = ( nOffset - nPosition ) / dWidthPosition * dWidthRuler + dRulerLeft;

				if( dPixel - dRulerLeft < 15 ) continue;
				if( dRulerLeft + dWidthRuler - dPixel < 15 ) continue;

				DoWriteLine( xw, dPixel, dRulerTop, dPixel, dRulerTop - nNotchHeight, "#000000", 0.5 );
				DoWriteRulerText( xw, dPixel, dRulerTop - nNotchHeight, nOffset );
			}
		}

		private static void DoWriteRulerText( XmlWriter xw, double x, double y, int nValue )
		{
			string strText = string.Format( "{0:N0}", nValue );

			// Rotated -45° text matching PnlMapRuler.RenderRulerText
			xw.WriteStartElement( "text" );
			xw.WriteAttributeString( "x", "0" );
			xw.WriteAttributeString( "y", "0" );
			xw.WriteAttributeString( "font-size", DoFormat( PnlMap.N_RULERTEXT_FONTSIZE ) );
			xw.WriteAttributeString( "fill", "#000000" );
			xw.WriteAttributeString( "transform",
				$"translate({DoFormat( x - PnlMap.N_RULERTEXT_WIDTH )},{DoFormat( y - PnlMap.N_RULERTEXT_HEIGHT )}) rotate(-45)" );
			xw.WriteString( strText );
			xw.WriteEndElement();
		}

		private static readonly int[] S_NUNITA = { 1, 10, 50, 100, 500, 1000, 5000, 10000, 50000, 100000, 500000 };

		private static int GetRulerUnit( double dWidthPerPix )
		{
			double dSpan = dWidthPerPix * PnlMap.N_RULER_TIPSPAN;

			foreach( int nUnit in S_NUNITA )
			{
				if( dSpan <= nUnit )
					return nUnit;
			}
			return 1000000;
		}

		// ─────────────────────────────────────────────────
		//  Lanes
		// ─────────────────────────────────────────────────
		private static void DoWriteLanes( XmlWriter xw, PnlMap pnlMap, double dWidth )
		{
			double dRulerTop = PnlMap.N_RULER_TOP;
			double dRulerLeft = PnlMap.N_RULER_LEFT;
			double dVGap = PnlMap.N_LANE_VERTICALGAP;
			double dLaneMargin = PnlMapLane.N_LANE_MARGIN;

			int nPosition = pnlMap.Position;
			int nPosMin = pnlMap.PositionMin;
			int nPosMax = pnlMap.PositionMax;
			double dZoom = pnlMap.Zoom;

			double dLeft = dRulerLeft - dVGap;
			double dDispWidth = dWidth - dLaneMargin - dLeft;
			double dWidthPosition = ( double )( nPosMax - nPosMin ) / dZoom;
			int nPosDispMin = nPosition;
			int nPosDispMax = nPosition + (int)Math.Round( dWidthPosition );

			double dY = dRulerTop + dVGap;

			for( int iLane = 0; iLane < pnlMap.GetCountLane(); iLane++ )
			{
				PnlMapLane pnlLane = pnlMap.GetLane( iLane );
				double dLaneHeight = pnlLane.LaneHeightActual;
				if( dLaneHeight <= 0 ) dLaneHeight = 60;

				DataType dt = pnlLane.DataTypeSelected;
				if( dt == null ) { dY += dLaneHeight + dVGap; continue; }

				// Lane background
				DoWriteRect( xw, dVGap, dY, dWidth - dVGap * 2, dLaneHeight, "#FAFAFA", "#E0E0E0" );

				// Lane label
				xw.WriteStartElement( "text" );
				xw.WriteAttributeString( "x", DoFormat( dVGap + 4 ) );
				xw.WriteAttributeString( "y", DoFormat( dY + 12 ) );
				xw.WriteAttributeString( "font-size", "10" );
				xw.WriteAttributeString( "fill", "#666666" );
				xw.WriteString( dt.Type );
				xw.WriteEndElement();

				// Get color
				string strColor = "#" + dt.GetColorString();

				// Get scale range — match PnlMapLane.OnRenderFeature exactly
				double dScaleMax = dt.ScaleMax;
				double dScaleMin = dt.ScaleMin;
				if( !dt.Scale )
				{
					dScaleMax = dt.ScoreMax;
					dScaleMin = dt.ScoreMin;
					dScaleMin = Math.Min( dScaleMin, 0 );
					dScaleMax = Math.Max( dScaleMax, 0 );
				}
				if( dScaleMax == dScaleMin ) { dScaleMax = dScaleMin + 1; }

				double dScaleBase = Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );
				double dHeightBase = ( dScaleMax - dScaleBase ) * ( dLaneHeight - 2 * dLaneMargin )
									/ ( dScaleMax - dScaleMin ) + dLaneMargin;
				double dHeightPos = dHeightBase - dLaneMargin;
				double dHeightNeg = dLaneHeight - 2 * dLaneMargin - dHeightPos;

				// Clip group
				xw.WriteStartElement( "g" );
				xw.WriteAttributeString( "clip-path", $"url(#clip-lane-{iLane})" );

				// Clippath definition
				xw.WriteStartElement( "defs" );
				xw.WriteStartElement( "clipPath" );
				xw.WriteAttributeString( "id", $"clip-lane-{iLane}" );
				DoWriteRect( xw, dLeft, dY + dLaneMargin, dDispWidth, dLaneHeight - 2 * dLaneMargin, null, null );
				xw.WriteEndElement(); // clipPath
				xw.WriteEndElement(); // defs

				// Features
				EDataTypeDisplay eDisplay = dt.Display;
				var lnk = dt.GetFeatureLinkFirst();
				double dPrevX = double.NaN;
				double dPrevY = double.NaN;
				double dOffXLast = -999;

				while( lnk != null )
				{
					DataFeature df = lnk.Value;

					int nPositionWidth = nPosDispMax - nPosDispMin;
					if( nPositionWidth <= 0 ) nPositionWidth = 1;

					if( df.End >= nPosDispMin && df.Start <= nPosDispMax )
					{
						double dOffX = dDispWidth * ( df.Start - nPosDispMin ) / nPositionWidth + dLeft;
						double dFeatW = ( df.End - df.Start + 1.0 ) * dDispWidth / nPositionWidth;
						if( dFeatW < 0.5 ) dFeatW = 0.5;

						// Skip features too close together (matches PnlMapLane rendering)
						if( eDisplay != EDataTypeDisplay.LINE && dOffX - dOffXLast < 0.5 )
						{
							lnk = lnk.Next;
							continue;
						}
						dOffXLast = dOffX;

						double dScore = Math.Min( Math.Max( df.Score, dScaleMin ), dScaleMax );
						double dFeatH, dOffY;

						if( dScore >= dScaleBase )
						{
							dFeatH = ( dScaleMax - dScaleBase ) > 0
								? dHeightPos / ( dScaleMax - dScaleBase ) * ( dScore - dScaleBase ) : 0;
							dOffY = dY + dHeightBase - dFeatH;
						}
						else
						{
							dFeatH = ( dScaleBase - dScaleMin ) > 0
								? dHeightNeg / ( dScaleBase - dScaleMin ) * ( dScaleBase - dScore ) : 0;
							dOffY = dY + dHeightBase;
						}

						string strFeatColor = strColor;
						if( df.ColorBrush is SolidColorBrush scb )
						{
							strFeatColor = $"#{scb.Color.R:X2}{scb.Color.G:X2}{scb.Color.B:X2}";
						}

						// Simulate anti-aliased sub-pixel rendering: bars < 1px get proportional opacity
						double dAlpha = Math.Min( 1.0, dFeatW );

						switch( eDisplay )
						{
							case EDataTypeDisplay.BAR:
								if( dFeatH > 0.5 )
									DoWriteRect( xw, dOffX, dOffY, dFeatW, dFeatH, strFeatColor, null, dAlpha );
								break;

							case EDataTypeDisplay.POINT:
								double dPtY = dScore >= dScaleBase ? dOffY : dOffY + dFeatH - 2;
								DoWriteRect( xw, dOffX, dPtY, dFeatW, 2, strFeatColor, null, dAlpha );
								break;

							case EDataTypeDisplay.LINE:
								double dLineY = dScore >= dScaleBase ? dOffY : dOffY + dFeatH;
								double dMidX = dOffX + dFeatW / 2;
								if( !double.IsNaN( dPrevX ) )
									DoWriteLine( xw, dPrevX, dPrevY, dMidX, dLineY, strFeatColor, 0.5 );
								dPrevX = dMidX;
								dPrevY = dLineY;
								break;

							case EDataTypeDisplay.STACK:
								DoWriteRect( xw, dOffX, dOffY, dFeatW, Math.Max( dFeatH, 0.5 ), strFeatColor, null, dAlpha );
								break;
						}
					}

					lnk = lnk.Next;
				}

				xw.WriteEndElement(); // g (clip group)

				// Lane chrome — vertical line, gridlines, baseline, score labels
				double dGap = 2;
				DoWriteLine( xw, dLeft, dY + dGap, dLeft, dY + dLaneHeight - dGap, "#333333", 0.5 );
				DoWriteLine( xw, dLeft, dY + dLaneMargin, dLeft + dDispWidth, dY + dLaneMargin, "#DDDDDD", 0.5 );
				DoWriteLine( xw, dLeft, dY + dLaneHeight - dLaneMargin, dLeft + dDispWidth, dY + dLaneHeight - dLaneMargin, "#DDDDDD", 0.5 );

				// Baseline
				DoWriteLine( xw, dLeft, dY + dHeightBase, dLeft + dDispWidth, dY + dHeightBase, "#333333", 0.5 );

				// Score labels
				double dLabelMax = dt.Scale ? dt.ScaleMax : Math.Max( 0, dt.ScoreMax );
				double dLabelMin = dt.Scale ? dt.ScaleMin : dt.ScoreMin;

				DoWriteScoreLabel( xw, dLeft - 2, dY + dLaneMargin + 3, dLabelMax );
				DoWriteScoreLabel( xw, dLeft - 2, dY + dLaneHeight - dLaneMargin + 3, dLabelMin );
				if( dScaleMin < 0 && dScaleMax > 0 )
					DoWriteScoreLabel( xw, dLeft - 2, dY + dHeightBase + 3, 0 );

				dY += dLaneHeight + dVGap;
			}
		}

		// ─────────────────────────────────────────────────
		//  SVG primitives
		// ─────────────────────────────────────────────────
		private static void DoWriteRect( XmlWriter xw, double x, double y, double w, double h, string fill, string stroke, double dOpacity = 1.0 )
		{
			xw.WriteStartElement( "rect" );
			xw.WriteAttributeString( "x", DoFormat( x ) );
			xw.WriteAttributeString( "y", DoFormat( y ) );
			xw.WriteAttributeString( "width", DoFormat( w ) );
			xw.WriteAttributeString( "height", DoFormat( h ) );
			if( fill != null )		xw.WriteAttributeString( "fill", fill );
			else					xw.WriteAttributeString( "fill", "none" );
			if( stroke != null )	xw.WriteAttributeString( "stroke", stroke );
			else					xw.WriteAttributeString( "stroke", "none" );
			if( dOpacity < 1.0 )	xw.WriteAttributeString( "opacity", DoFormat( dOpacity ) );
			xw.WriteEndElement();
		}

		private static void DoWriteLine( XmlWriter xw, double x1, double y1, double x2, double y2, string stroke, double strokeWidth )
		{
			xw.WriteStartElement( "line" );
			xw.WriteAttributeString( "x1", DoFormat( x1 ) );
			xw.WriteAttributeString( "y1", DoFormat( y1 ) );
			xw.WriteAttributeString( "x2", DoFormat( x2 ) );
			xw.WriteAttributeString( "y2", DoFormat( y2 ) );
			xw.WriteAttributeString( "stroke", stroke );
			xw.WriteAttributeString( "stroke-width", DoFormat( strokeWidth ) );
			xw.WriteEndElement();
		}

		private static void DoWriteScoreLabel( XmlWriter xw, double x, double y, double dScore )
		{
			string strScore = dScore.ToString( "G4", CultureInfo.InvariantCulture );

			xw.WriteStartElement( "text" );
			xw.WriteAttributeString( "x", DoFormat( x ) );
			xw.WriteAttributeString( "y", DoFormat( y ) );
			xw.WriteAttributeString( "font-size", "9" );
			xw.WriteAttributeString( "fill", "#666666" );
			xw.WriteAttributeString( "text-anchor", "end" );
			xw.WriteString( strScore );
			xw.WriteEndElement();
		}

		public static string DoFormat( double d )
		{
			return d.ToString( "G6", CultureInfo.InvariantCulture );
		}
	}
}
