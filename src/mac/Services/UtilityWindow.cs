using System;
using Avalonia.Controls;

namespace MetaScope.Services
{
	public class UtilityWindow
	{
		/// <summary>
		/// Tracks per-window maximize/minimize enabled state since Avalonia
		/// does not expose individual maximize/minimize button visibility
		/// as separate readable properties.
		/// </summary>
		private		static readonly System.Collections.Generic.Dictionary< Window, bool >
					s_dictMaximizeEnabled		= new System.Collections.Generic.Dictionary< Window, bool >();
		private		static readonly System.Collections.Generic.Dictionary< Window, bool >
					s_dictMinimizeEnabled		= new System.Collections.Generic.Dictionary< Window, bool >();

		public static void DisableMaximize( Window wnd )
		{
			lock( wnd )
			{
				s_dictMaximizeEnabled[ wnd ]	= false;
				ApplyChrome( wnd );
			}
		}

		public static void DisableMinimize( Window wnd )
		{
			lock( wnd )
			{
				s_dictMinimizeEnabled[ wnd ]	= false;
				ApplyChrome( wnd );
			}
		}

		public static void EnableMaximize( Window wnd )
		{
			lock( wnd )
			{
				s_dictMaximizeEnabled[ wnd ]	= true;
				ApplyChrome( wnd );
			}
		}

		public static void EnableMinimize( Window wnd )
		{
			lock( wnd )
			{
				s_dictMinimizeEnabled[ wnd ]	= true;
				ApplyChrome( wnd );
			}
		}

		public static void ToggleMaximize( Window wnd )
		{
			lock( wnd )
			{
				bool			bEnabled		= true;
				if( s_dictMaximizeEnabled.ContainsKey( wnd ) )
					bEnabled				= s_dictMaximizeEnabled[ wnd ];

				s_dictMaximizeEnabled[ wnd ]	= !bEnabled;
				ApplyChrome( wnd );
			}
		}

		public static void ToggleMinimize( Window wnd )
		{
			lock( wnd )
			{
				bool			bEnabled		= true;
				if( s_dictMinimizeEnabled.ContainsKey( wnd ) )
					bEnabled				= s_dictMinimizeEnabled[ wnd ];

				s_dictMinimizeEnabled[ wnd ]	= !bEnabled;
				ApplyChrome( wnd );
			}
		}

		/// <summary>
		/// Applies the current maximize/minimize state to the window's
		/// CanResize property. When both are disabled, resizing is fully
		/// disabled. Avalonia does not expose per-button control on macOS
		/// the way Win32 does, so CanResize is the closest equivalent.
		/// </summary>
		private static void ApplyChrome( Window wnd )
		{
			bool			bMaximize		= true;
			bool			bMinimize		= true;

			if( s_dictMaximizeEnabled.ContainsKey( wnd ) )
				bMaximize				= s_dictMaximizeEnabled[ wnd ];
			if( s_dictMinimizeEnabled.ContainsKey( wnd ) )
				bMinimize				= s_dictMinimizeEnabled[ wnd ];

			// On macOS, Avalonia does not provide independent control over
			// the maximize and minimize title-bar buttons. CanResize is the
			// closest cross-platform lever: when false, both zoom and
			// resize are disabled.
			wnd.CanResize					= bMaximize || bMinimize;
		}
	}
}
