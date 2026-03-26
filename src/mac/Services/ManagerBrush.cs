using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace MetaScope.Services
{
	using		DicBrush					= Dictionary< string, IBrush >;

	class ManagerBrush
	{
		//			.								.								.
		private		static ManagerBrush				S_MANAGER						= null;
		private		static IBrush[]					OBJ_BRUSH						=
			{
				new ImmutableSolidColorBrush( Colors.AliceBlue ),
				new ImmutableSolidColorBrush( Colors.Aqua ),
				new ImmutableSolidColorBrush( Colors.Aquamarine ),
				new ImmutableSolidColorBrush( Colors.Azure ),
				new ImmutableSolidColorBrush( Colors.Beige ),
				new ImmutableSolidColorBrush( Colors.Bisque ),
				new ImmutableSolidColorBrush( Colors.BlanchedAlmond ),
				new ImmutableSolidColorBrush( Colors.Blue ),
				new ImmutableSolidColorBrush( Colors.BlueViolet ),
				new ImmutableSolidColorBrush( Colors.Brown ),
				new ImmutableSolidColorBrush( Colors.BurlyWood ),
				new ImmutableSolidColorBrush( Colors.CadetBlue ),
				new ImmutableSolidColorBrush( Colors.Chartreuse ),
				new ImmutableSolidColorBrush( Colors.Chocolate ),
				new ImmutableSolidColorBrush( Colors.Coral ),
				new ImmutableSolidColorBrush( Colors.CornflowerBlue ),
				new ImmutableSolidColorBrush( Colors.Cornsilk ),
				new ImmutableSolidColorBrush( Colors.Crimson ),
				new ImmutableSolidColorBrush( Colors.Cyan ),
				new ImmutableSolidColorBrush( Colors.DarkBlue ),
				new ImmutableSolidColorBrush( Colors.DarkCyan ),
				new ImmutableSolidColorBrush( Colors.DarkGoldenrod ),
				new ImmutableSolidColorBrush( Colors.DarkGray ),
				new ImmutableSolidColorBrush( Colors.DarkGreen ),
				new ImmutableSolidColorBrush( Colors.DarkKhaki ),
				new ImmutableSolidColorBrush( Colors.DarkMagenta ),
				new ImmutableSolidColorBrush( Colors.DarkOliveGreen ),
				new ImmutableSolidColorBrush( Colors.DarkOrange ),
				new ImmutableSolidColorBrush( Colors.DarkOrchid ),
				new ImmutableSolidColorBrush( Colors.DarkRed ),
				new ImmutableSolidColorBrush( Colors.DarkSalmon ),
				new ImmutableSolidColorBrush( Colors.DarkSeaGreen ),
				new ImmutableSolidColorBrush( Colors.DarkSlateBlue ),
				new ImmutableSolidColorBrush( Colors.DarkSlateGray ),
				new ImmutableSolidColorBrush( Colors.DarkTurquoise ),
				new ImmutableSolidColorBrush( Colors.DarkViolet ),
				new ImmutableSolidColorBrush( Colors.DeepPink ),
				new ImmutableSolidColorBrush( Colors.DeepSkyBlue ),
				new ImmutableSolidColorBrush( Colors.DimGray ),
				new ImmutableSolidColorBrush( Colors.DodgerBlue ),
				new ImmutableSolidColorBrush( Colors.Firebrick ),
				new ImmutableSolidColorBrush( Colors.FloralWhite ),
				new ImmutableSolidColorBrush( Colors.ForestGreen ),
				new ImmutableSolidColorBrush( Colors.Fuchsia ),
				new ImmutableSolidColorBrush( Colors.Gainsboro ),
				new ImmutableSolidColorBrush( Colors.GhostWhite ),
				new ImmutableSolidColorBrush( Colors.Gold ),
				new ImmutableSolidColorBrush( Colors.Goldenrod ),
				new ImmutableSolidColorBrush( Colors.Green ),
				new ImmutableSolidColorBrush( Colors.GreenYellow ),
				new ImmutableSolidColorBrush( Colors.Honeydew ),
				new ImmutableSolidColorBrush( Colors.HotPink ),
				new ImmutableSolidColorBrush( Colors.IndianRed ),
				new ImmutableSolidColorBrush( Colors.Indigo ),
				new ImmutableSolidColorBrush( Colors.Ivory ),
				new ImmutableSolidColorBrush( Colors.Khaki ),
				new ImmutableSolidColorBrush( Colors.Lavender ),
				new ImmutableSolidColorBrush( Colors.LavenderBlush ),
				new ImmutableSolidColorBrush( Colors.LawnGreen ),
				new ImmutableSolidColorBrush( Colors.LemonChiffon ),
				new ImmutableSolidColorBrush( Colors.LightBlue ),
				new ImmutableSolidColorBrush( Colors.LightCoral ),
				new ImmutableSolidColorBrush( Colors.LightCyan ),
				new ImmutableSolidColorBrush( Colors.LightGoldenrodYellow ),
				new ImmutableSolidColorBrush( Colors.LightGreen ),
				new ImmutableSolidColorBrush( Colors.LightPink ),
				new ImmutableSolidColorBrush( Colors.LightSalmon ),
				new ImmutableSolidColorBrush( Colors.LightSeaGreen ),
				new ImmutableSolidColorBrush( Colors.LightSkyBlue ),
				new ImmutableSolidColorBrush( Colors.LightSlateGray ),
				new ImmutableSolidColorBrush( Colors.LightSteelBlue ),
				new ImmutableSolidColorBrush( Colors.LightYellow ),
				new ImmutableSolidColorBrush( Colors.Lime ),
				new ImmutableSolidColorBrush( Colors.LimeGreen ),
				new ImmutableSolidColorBrush( Colors.Linen ),
				new ImmutableSolidColorBrush( Colors.Magenta ),
				new ImmutableSolidColorBrush( Colors.Maroon ),
				new ImmutableSolidColorBrush( Colors.MediumAquamarine ),
				new ImmutableSolidColorBrush( Colors.MediumBlue ),
				new ImmutableSolidColorBrush( Colors.MediumOrchid ),
				new ImmutableSolidColorBrush( Colors.MediumPurple ),
				new ImmutableSolidColorBrush( Colors.MediumSeaGreen ),
				new ImmutableSolidColorBrush( Colors.MediumSlateBlue ),
				new ImmutableSolidColorBrush( Colors.MediumSpringGreen ),
				new ImmutableSolidColorBrush( Colors.MediumTurquoise ),
				new ImmutableSolidColorBrush( Colors.MediumVioletRed ),
				new ImmutableSolidColorBrush( Colors.MidnightBlue ),
				new ImmutableSolidColorBrush( Colors.MintCream ),
				new ImmutableSolidColorBrush( Colors.MistyRose ),
				new ImmutableSolidColorBrush( Colors.Moccasin ),
				new ImmutableSolidColorBrush( Colors.NavajoWhite ),
				new ImmutableSolidColorBrush( Colors.Navy ),
				new ImmutableSolidColorBrush( Colors.OldLace ),
				new ImmutableSolidColorBrush( Colors.Olive ),
				new ImmutableSolidColorBrush( Colors.OliveDrab ),
				new ImmutableSolidColorBrush( Colors.Orange ),
				new ImmutableSolidColorBrush( Colors.OrangeRed ),
				new ImmutableSolidColorBrush( Colors.Orchid ),
				new ImmutableSolidColorBrush( Colors.PaleGoldenrod ),
				new ImmutableSolidColorBrush( Colors.PaleGreen ),
				new ImmutableSolidColorBrush( Colors.PaleTurquoise ),
				new ImmutableSolidColorBrush( Colors.PaleVioletRed ),
				new ImmutableSolidColorBrush( Colors.PapayaWhip ),
				new ImmutableSolidColorBrush( Colors.PeachPuff ),
				new ImmutableSolidColorBrush( Colors.Peru ),
				new ImmutableSolidColorBrush( Colors.Pink ),
				new ImmutableSolidColorBrush( Colors.Plum ),
				new ImmutableSolidColorBrush( Colors.PowderBlue ),
				new ImmutableSolidColorBrush( Colors.Purple ),
				new ImmutableSolidColorBrush( Colors.Red ),
				new ImmutableSolidColorBrush( Colors.RosyBrown ),
				new ImmutableSolidColorBrush( Colors.RoyalBlue ),
				new ImmutableSolidColorBrush( Colors.SaddleBrown ),
				new ImmutableSolidColorBrush( Colors.Salmon ),
				new ImmutableSolidColorBrush( Colors.SandyBrown ),
				new ImmutableSolidColorBrush( Colors.SeaGreen ),
				new ImmutableSolidColorBrush( Colors.SeaShell ),
				new ImmutableSolidColorBrush( Colors.Sienna ),
				new ImmutableSolidColorBrush( Colors.Silver ),
				new ImmutableSolidColorBrush( Colors.SkyBlue ),
				new ImmutableSolidColorBrush( Colors.SlateBlue ),
				new ImmutableSolidColorBrush( Colors.SlateGray ),
				new ImmutableSolidColorBrush( Colors.SpringGreen ),
				new ImmutableSolidColorBrush( Colors.SteelBlue ),
				new ImmutableSolidColorBrush( Colors.Tan ),
				new ImmutableSolidColorBrush( Colors.Teal ),
				new ImmutableSolidColorBrush( Colors.Thistle ),
				new ImmutableSolidColorBrush( Colors.Tomato ),
				new ImmutableSolidColorBrush( Colors.Turquoise ),
				new ImmutableSolidColorBrush( Colors.Violet ),
				new ImmutableSolidColorBrush( Colors.Wheat ),
				new ImmutableSolidColorBrush( Colors.Yellow ),
				new ImmutableSolidColorBrush( Colors.YellowGreen )
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

		public IBrush GetBrushAlpha( string strAlpha, IBrush bsh )
		{
			ISolidColorBrush	scb				= bsh as ISolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );

			IBrush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public IBrush GetBrushDark( ISolidColorBrush scb )
		{
			byte			bDiff			= 200;
			byte			bRed			= ( byte ) Math.Max( 0, ( int ) scb.Color.R - bDiff );
			byte			bGreen			= ( byte ) Math.Max( 0, ( int ) scb.Color.G - bDiff );
			byte			bBlue			= ( byte ) Math.Max( 0, ( int ) scb.Color.B - bDiff );

			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", bRed, bGreen, bBlue );

			IBrush			bsh				= GetBrush( "FF", strColor );

			return bsh;
		}

		public IBrush GetBrushSolid( Color clr )
		{
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B );

			IBrush			bshReturn		= GetBrush( "FF", strColor );

			return bshReturn;
		}

		public IBrush GetBrush( Color clr, byte bAlpha )
		{
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B );
			string			strAlpha		= string.Format( "{0:X2}", bAlpha );

			IBrush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public IBrush GetBrush( Color clr )
		{
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", clr.R, clr.G, clr.B );
			string			strAlpha		= STR_OPACITY_DEFAULT;

			IBrush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public IBrush GetBrush( IBrush bsh )
		{
			ISolidColorBrush	scb				= bsh as ISolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );
			string			strAlpha		= STR_OPACITY_DEFAULT;

			IBrush			bshReturn		= GetBrush( strAlpha, strColor );

			return bshReturn;
		}

		public IBrush GetBrush( string strAlpha, string strColor )
		{
			IBrush			bsh				= null;
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

				bsh				= new ImmutableSolidColorBrush( clr );

				m_dicBrush.Add( strAlpha + strColor, bsh );
			}

			return bsh;
		}

		public IBrush GetBrush( string strColor )
		{
			IBrush			bsh				= GetBrush( STR_OPACITY_DEFAULT, strColor );

			return bsh;
		}

		public IBrush GetBrushRandom()
		{
			int				nIndex			= m_rnd.Next( OBJ_BRUSH.Count() );
			ISolidColorBrush	scb				= OBJ_BRUSH[ nIndex ] as ISolidColorBrush;

			byte			bR				= scb.Color.R > 200 ? ( byte ) ( scb.Color.R - 125 ) : scb.Color.R;
			byte			bG				= scb.Color.G > 200 ? ( byte ) ( scb.Color.G - 125 ) : scb.Color.G;
			byte			bB				= scb.Color.B > 200 ? ( byte ) ( scb.Color.B - 125 ) : scb.Color.B;

			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", bR, bG, bB );

			IBrush			bshReturn		= GetBrush( strColor );

			return bshReturn;
		}
	}
}
