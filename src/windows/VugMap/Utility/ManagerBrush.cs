using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using AvalonDock;
using VugMap.Utility.Logger;

namespace VugMap.Utility
{
	using		DicBrush					= Dictionary< string, Brush >;

	class ManagerBrush
	{
		//			.								.								.
		private		static ManagerBrush				S_MANAGER						= null;
		private		static Brush[]					OBJ_BRUSH						=
			{
				Brushes.AliceBlue,
				Brushes.Aqua,
				Brushes.Aquamarine,
				Brushes.Azure,
				Brushes.Beige,
				Brushes.Bisque,
				Brushes.BlanchedAlmond,
				Brushes.Blue,
				Brushes.BlueViolet,
				Brushes.Brown,
				Brushes.BurlyWood,
				Brushes.CadetBlue,
				Brushes.Chartreuse,
				Brushes.Chocolate,
				Brushes.Coral,
				Brushes.CornflowerBlue,
				Brushes.Cornsilk,
				Brushes.Crimson,
				Brushes.Cyan,
				Brushes.DarkBlue,
				Brushes.DarkCyan,
				Brushes.DarkGoldenrod,
				Brushes.DarkGray,
				Brushes.DarkGreen,
				Brushes.DarkKhaki,
				Brushes.DarkMagenta,
				Brushes.DarkOliveGreen,
				Brushes.DarkOrange,
				Brushes.DarkOrchid,
				Brushes.DarkRed,
				Brushes.DarkSalmon,
				Brushes.DarkSeaGreen,
				Brushes.DarkSlateBlue,
				Brushes.DarkSlateGray,
				Brushes.DarkTurquoise,
				Brushes.DarkViolet,
				Brushes.DeepPink,
				Brushes.DeepSkyBlue,
				Brushes.DimGray,
				Brushes.DodgerBlue,
				Brushes.Firebrick,
				Brushes.FloralWhite,
				Brushes.ForestGreen,
				Brushes.Fuchsia,
				Brushes.Gainsboro,
				Brushes.GhostWhite,
				Brushes.Gold,
				Brushes.Goldenrod,				
				Brushes.Green,
				Brushes.GreenYellow,
				Brushes.Honeydew,
				Brushes.HotPink,
				Brushes.IndianRed,
				Brushes.Indigo,
				Brushes.Ivory,
				Brushes.Khaki,
				Brushes.Lavender,
				Brushes.LavenderBlush,
				Brushes.LawnGreen,
				Brushes.LemonChiffon,
				Brushes.LightBlue,
				Brushes.LightCoral,
				Brushes.LightCyan,
				Brushes.LightGoldenrodYellow,
				Brushes.LightGreen,
				Brushes.LightPink,
				Brushes.LightSalmon,
				Brushes.LightSeaGreen,
				Brushes.LightSkyBlue,
				Brushes.LightSlateGray,
				Brushes.LightSteelBlue,
				Brushes.LightYellow,
				Brushes.Lime,
				Brushes.LimeGreen,
				Brushes.Linen,
				Brushes.Magenta,
				Brushes.Maroon,
				Brushes.MediumAquamarine,
				Brushes.MediumBlue,
				Brushes.MediumOrchid,
				Brushes.MediumPurple,
				Brushes.MediumSeaGreen,
				Brushes.MediumSlateBlue,
				Brushes.MediumSpringGreen,
				Brushes.MediumTurquoise,
				Brushes.MediumVioletRed,
				Brushes.MidnightBlue,
				Brushes.MintCream,
				Brushes.MistyRose,
				Brushes.Moccasin,
				Brushes.NavajoWhite,
				Brushes.Navy,
				Brushes.OldLace,
				Brushes.Olive,
				Brushes.OliveDrab,
				Brushes.Orange,
				Brushes.OrangeRed,
				Brushes.Orchid,
				Brushes.PaleGoldenrod,
				Brushes.PaleGreen,
				Brushes.PaleTurquoise,
				Brushes.PaleVioletRed,
				Brushes.PapayaWhip,
				Brushes.PeachPuff,
				Brushes.Peru,
				Brushes.Pink,
				Brushes.Plum,
				Brushes.PowderBlue,
				Brushes.Purple,
				Brushes.Red,
				Brushes.RosyBrown,
				Brushes.RoyalBlue,
				Brushes.SaddleBrown,
				Brushes.Salmon,
				Brushes.SandyBrown,
				Brushes.SeaGreen,
				Brushes.SeaShell,
				Brushes.Sienna,
				Brushes.Silver,
				Brushes.SkyBlue,
				Brushes.SlateBlue,
				Brushes.SlateGray,				
				Brushes.SpringGreen,
				Brushes.SteelBlue,
				Brushes.Tan,
				Brushes.Teal,
				Brushes.Thistle,
				Brushes.Tomato,
				Brushes.Turquoise,
				Brushes.Violet,
				Brushes.Wheat,
				Brushes.Yellow,
				Brushes.YellowGreen				
			};

		static ManagerBrush()
		{
			S_MANAGER			= new ManagerBrush();
		}

		private		static string					STR_OPACITY_DEFAULT				= "EE";

		public static ManagerBrush GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER			= new ManagerBrush();
			}

			return S_MANAGER;
		}

		public static string DoOpacityGet()
		{
			return STR_OPACITY_DEFAULT;
		}

		public static void DoOpacitySet( string strOpacity )
		{
			STR_OPACITY_DEFAULT				= strOpacity;
		}

		public string Opacity
		{
			get {	return DoOpacityGet(); }
			set {	DoOpacitySet( value ); }
		}

		private		DicBrush						m_dicBrush						= null;
		private		Random							m_rnd							= null;

		public ManagerBrush()
		{
			m_dicBrush			= new DicBrush();
			m_rnd				= new Random( ( int ) DateTime.Now.Ticks );
		}

		public Brush GetBrushAlpha( string strAlpha, Brush bsh )
		{
			SolidColorBrush	scb				= bsh as SolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );

			Brush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public Brush GetBrushDark( SolidColorBrush scb )
		{
			byte			bDiff			= 200;
			byte			bRed			= ( byte ) Math.Max( 0, ( int ) scb.Color.R - bDiff );
			byte			bGreen			= ( byte ) Math.Max( 0, ( int ) scb.Color.G - bDiff );
			byte			bBlue			= ( byte ) Math.Max( 0, ( int ) scb.Color.B - bDiff );
			
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", bRed, bGreen, bBlue );

			Brush			bsh				= GetBrush( "FF", strColor );

			return bsh;
		}
		
		public Brush GetBrushSolid( Color clr )
		{
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B );			

			Brush			bshReturn		= GetBrush( "FF", strColor );

			return bshReturn;
		}

		public Brush GetBrush( Color clr, byte bAlpha )
		{
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B );
			string			strAlpha		= string.Format( "{0:X2}", bAlpha );

			Brush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public Brush GetBrush( Color clr )
		{
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B );
			string			strAlpha		= STR_OPACITY_DEFAULT;

			Brush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}
	
		public Brush GetBrush( Brush bsh )
		{
			SolidColorBrush	scb				= bsh as SolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );
			string			strAlpha		= STR_OPACITY_DEFAULT;

			Brush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public Brush GetBrush( string strAlpha, string strColor )
		{
			Brush			bsh				= null;
			bool			bReturn			= m_dicBrush.TryGetValue( strAlpha + strColor, out bsh );

			if( bReturn == false )
			{				
				string			strRed			= strColor.Substring( 0, 2 );
				string			strGreen		= strColor.Substring( 2, 2 );
				string			strBlue			= strColor.Substring( 4, 2 );
			
				byte			bAlpha			= byte.Parse( strAlpha, NumberStyles.HexNumber );
				byte			bRed			= byte.Parse( strRed, NumberStyles.HexNumber );
				byte			bGreen			= byte.Parse( strGreen, NumberStyles.HexNumber );
				byte			bBlue			= byte.Parse( strBlue, NumberStyles.HexNumber );

				Color			clr				= Color.FromArgb( bAlpha, bRed, bGreen, bBlue );

				bsh				= new SolidColorBrush( clr );
				bsh.Freeze();

				m_dicBrush.Add( strAlpha + strColor, bsh );
			}

			return bsh;			
		}

		public Brush GetBrush( string strColor )
		{
			Brush			bsh				= GetBrush( STR_OPACITY_DEFAULT, strColor );

			return bsh;
		}

		public Brush GetBrushRandom()
		{			
			int				nIndex			= m_rnd.Next( OBJ_BRUSH.Count() );
			SolidColorBrush	scb				= OBJ_BRUSH[ nIndex ] as SolidColorBrush;
			
			byte			bR				= scb.Color.R > 200 ? ( byte ) ( scb.Color.R - 125 ) : scb.Color.R;
			byte			bG				= scb.Color.G > 200 ? ( byte ) ( scb.Color.G - 125 ) : scb.Color.G;
			byte			bB				= scb.Color.B > 200 ? ( byte ) ( scb.Color.B - 125 ) : scb.Color.B;

			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", bR, bG, bB );			

			Brush			bshReturn		= GetBrush( strColor );

			return bshReturn;
		}
	}
}
