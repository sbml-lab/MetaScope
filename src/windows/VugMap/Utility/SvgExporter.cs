using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace VugMap.Utility
{
	public static class SvgExporter
	{
		private		static int						s_nClipId						= 0;

		public static void DoExport( Visual visual, double dWidth, double dHeight, string strPath )
		{
			s_nClipId						= 0;

			XmlWriterSettings	settings	= new XmlWriterSettings();
			settings.Indent					= true;
			settings.IndentChars			= "\t";

			using( XmlWriter xw = XmlWriter.Create( strPath, settings ) )
			{
				xw.WriteStartDocument();
				xw.WriteStartElement( "svg", "http://www.w3.org/2000/svg" );
				xw.WriteAttributeString( "width", DoFormat( dWidth ) );
				xw.WriteAttributeString( "height", DoFormat( dHeight ) );
				xw.WriteAttributeString( "viewBox", string.Format( "0 0 {0} {1}", DoFormat( dWidth ), DoFormat( dHeight ) ) );

				xw.WriteStartElement( "rect" );
				xw.WriteAttributeString( "width", "100%" );
				xw.WriteAttributeString( "height", "100%" );
				xw.WriteAttributeString( "fill", "white" );
				xw.WriteEndElement();

				DoVisualWrite( xw, visual );

				xw.WriteEndElement();
				xw.WriteEndDocument();
			}
		}

		private static void DoVisualWrite( XmlWriter xw, Visual visual )
		{
			Drawing			drawing			= VisualTreeHelper.GetDrawing( visual );
			Transform		transform		= VisualTreeHelper.GetTransform( visual );
			Geometry		clip			= VisualTreeHelper.GetClip( visual );
			double			dOpacity		= VisualTreeHelper.GetOpacity( visual );

			bool			bGroup			= transform != null || clip != null || dOpacity < 1.0;

			if( bGroup )
			{
				xw.WriteStartElement( "g" );

				if( transform != null && transform != Transform.Identity )
					xw.WriteAttributeString( "transform", DoTransformFormat( transform ) );

				if( dOpacity < 1.0 )
					xw.WriteAttributeString( "opacity", DoFormat( dOpacity ) );

				if( clip != null )
					DoClipWrite( xw, clip );
			}

			if( drawing != null )
				DoDrawingWrite( xw, drawing );

			int				nCount			= VisualTreeHelper.GetChildrenCount( visual );
			for( int i = 0; i < nCount; i++ )
			{
				Visual		child			= VisualTreeHelper.GetChild( visual, i ) as Visual;
				if( child != null )
					DoVisualWrite( xw, child );
			}

			if( bGroup )
				xw.WriteEndElement();
		}

		private static void DoDrawingWrite( XmlWriter xw, Drawing drawing )
		{
			if( drawing is DrawingGroup )
			{
				DrawingGroup	dg			= ( DrawingGroup ) drawing;

				bool		bGroup			= dg.ClipGeometry != null || dg.Opacity < 1.0 || dg.Transform != null;

				if( bGroup )
				{
					xw.WriteStartElement( "g" );

					if( dg.Transform != null && dg.Transform != Transform.Identity )
						xw.WriteAttributeString( "transform", DoTransformFormat( dg.Transform ) );

					if( dg.Opacity < 1.0 )
						xw.WriteAttributeString( "opacity", DoFormat( dg.Opacity ) );

					if( dg.ClipGeometry != null )
						DoClipWrite( xw, dg.ClipGeometry );
				}

				foreach( Drawing child in dg.Children )
				{
					DoDrawingWrite( xw, child );
				}

				if( bGroup )
					xw.WriteEndElement();
			}
			else if( drawing is GeometryDrawing )
			{
				DoGeometryDrawingWrite( xw, ( GeometryDrawing ) drawing );
			}
			else if( drawing is GlyphRunDrawing )
			{
				DoGlyphRunDrawingWrite( xw, ( GlyphRunDrawing ) drawing );
			}
		}

		private static void DoGeometryDrawingWrite( XmlWriter xw, GeometryDrawing gd )
		{
			Geometry		geom			= gd.Geometry;

			if( geom is RectangleGeometry )
			{
				RectangleGeometry	rg		= ( RectangleGeometry ) geom;
				Rect			rt			= rg.Rect;

				xw.WriteStartElement( "rect" );
				xw.WriteAttributeString( "x", DoFormat( rt.X ) );
				xw.WriteAttributeString( "y", DoFormat( rt.Y ) );
				xw.WriteAttributeString( "width", DoFormat( rt.Width ) );
				xw.WriteAttributeString( "height", DoFormat( rt.Height ) );

				DoBrushWrite( xw, "fill", gd.Brush );
				DoPenWrite( xw, gd.Pen );

				xw.WriteEndElement();
			}
			else if( geom is LineGeometry )
			{
				LineGeometry	lg			= ( LineGeometry ) geom;

				xw.WriteStartElement( "line" );
				xw.WriteAttributeString( "x1", DoFormat( lg.StartPoint.X ) );
				xw.WriteAttributeString( "y1", DoFormat( lg.StartPoint.Y ) );
				xw.WriteAttributeString( "x2", DoFormat( lg.EndPoint.X ) );
				xw.WriteAttributeString( "y2", DoFormat( lg.EndPoint.Y ) );

				if( gd.Pen != null )
				{
					DoBrushWrite( xw, "stroke", gd.Pen.Brush );
					xw.WriteAttributeString( "stroke-width", DoFormat( gd.Pen.Thickness ) );
				}

				xw.WriteEndElement();
			}
			else if( geom is PathGeometry || geom is StreamGeometry )
			{
				string		strPath			= geom.ToString( CultureInfo.InvariantCulture );

				xw.WriteStartElement( "path" );
				xw.WriteAttributeString( "d", strPath );

				DoBrushWrite( xw, "fill", gd.Brush );
				DoPenWrite( xw, gd.Pen );

				xw.WriteEndElement();
			}
		}

		private static void DoGlyphRunDrawingWrite( XmlWriter xw, GlyphRunDrawing grd )
		{
			GlyphRun		gr				= grd.GlyphRun;
			if( gr == null )
				return;

			Point			ptBase			= gr.BaselineOrigin;
			double			dFontSize		= gr.FontRenderingEmSize;
			string			strFontFamily	= "sans-serif";
			IDictionary< CultureInfo, string >	dicNames	= gr.GlyphTypeface.FamilyNames;
			if( dicNames != null && dicNames.Count > 0 )
			{
				foreach( string strName in dicNames.Values )
				{
					strFontFamily				= strName;
					break;
				}
			}

			string			strText			= "";
			if( gr.Characters != null )
			{
				char[]		chars			= new char[ gr.Characters.Count ];
				gr.Characters.CopyTo( chars, 0 );
				strText						= new string( chars );
			}

			xw.WriteStartElement( "text" );
			xw.WriteAttributeString( "x", DoFormat( ptBase.X ) );
			xw.WriteAttributeString( "y", DoFormat( ptBase.Y ) );
			xw.WriteAttributeString( "font-family", strFontFamily );
			xw.WriteAttributeString( "font-size", DoFormat( dFontSize ) );

			DoBrushWrite( xw, "fill", grd.ForegroundBrush );

			xw.WriteString( strText );
			xw.WriteEndElement();
		}

		private static void DoBrushWrite( XmlWriter xw, string strAttr, Brush bsh )
		{
			if( bsh == null )
			{
				xw.WriteAttributeString( strAttr, "none" );
				return;
			}

			if( bsh is SolidColorBrush )
			{
				SolidColorBrush	scb			= ( SolidColorBrush ) bsh;
				Color			clr			= scb.Color;

				xw.WriteAttributeString( strAttr, string.Format( "#{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B ) );

				if( clr.A < 255 )
					xw.WriteAttributeString( strAttr + "-opacity", DoFormat( clr.A / 255.0 ) );
			}
			else
			{
				xw.WriteAttributeString( strAttr, "#000000" );
			}
		}

		private static void DoPenWrite( XmlWriter xw, Pen pen )
		{
			if( pen == null )
			{
				xw.WriteAttributeString( "stroke", "none" );
				return;
			}

			DoBrushWrite( xw, "stroke", pen.Brush );
			xw.WriteAttributeString( "stroke-width", DoFormat( pen.Thickness ) );
		}

		private static void DoClipWrite( XmlWriter xw, Geometry clip )
		{
			string			strId			= string.Format( "clip{0}", s_nClipId++ );

			xw.WriteAttributeString( "clip-path", string.Format( "url(#{0})", strId ) );

			xw.WriteStartElement( "defs" );
			xw.WriteStartElement( "clipPath" );
			xw.WriteAttributeString( "id", strId );

			if( clip is RectangleGeometry )
			{
				RectangleGeometry	rg		= ( RectangleGeometry ) clip;
				Rect			rt			= rg.Rect;

				xw.WriteStartElement( "rect" );
				xw.WriteAttributeString( "x", DoFormat( rt.X ) );
				xw.WriteAttributeString( "y", DoFormat( rt.Y ) );
				xw.WriteAttributeString( "width", DoFormat( rt.Width ) );
				xw.WriteAttributeString( "height", DoFormat( rt.Height ) );
				xw.WriteEndElement();
			}

			xw.WriteEndElement();
			xw.WriteEndElement();
		}

		private static string DoTransformFormat( Transform transform )
		{
			if( transform is TranslateTransform )
			{
				TranslateTransform	tt		= ( TranslateTransform ) transform;
				return string.Format( "translate({0},{1})", DoFormat( tt.X ), DoFormat( tt.Y ) );
			}
			else if( transform is RotateTransform )
			{
				RotateTransform		rt		= ( RotateTransform ) transform;
				return string.Format( "rotate({0},{1},{2})", DoFormat( rt.Angle ), DoFormat( rt.CenterX ), DoFormat( rt.CenterY ) );
			}
			else if( transform is ScaleTransform )
			{
				ScaleTransform		st		= ( ScaleTransform ) transform;
				return string.Format( "scale({0},{1})", DoFormat( st.ScaleX ), DoFormat( st.ScaleY ) );
			}
			else if( transform is MatrixTransform )
			{
				MatrixTransform		mt		= ( MatrixTransform ) transform;
				Matrix				mx		= mt.Matrix;
				return string.Format( "matrix({0},{1},{2},{3},{4},{5})",
					DoFormat( mx.M11 ), DoFormat( mx.M12 ), DoFormat( mx.M21 ), DoFormat( mx.M22 ),
					DoFormat( mx.OffsetX ), DoFormat( mx.OffsetY ) );
			}
			else if( transform is TransformGroup )
			{
				TransformGroup		tg		= ( TransformGroup ) transform;
				Matrix				mx		= tg.Value;
				return string.Format( "matrix({0},{1},{2},{3},{4},{5})",
					DoFormat( mx.M11 ), DoFormat( mx.M12 ), DoFormat( mx.M21 ), DoFormat( mx.M22 ),
					DoFormat( mx.OffsetX ), DoFormat( mx.OffsetY ) );
			}

			return "";
		}

		private static string DoFormat( double d )
		{
			return d.ToString( "G6", CultureInfo.InvariantCulture );
		}
	}
}
