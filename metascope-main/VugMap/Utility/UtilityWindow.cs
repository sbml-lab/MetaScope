using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;


namespace VugMap.Utility
{
	public class UtilityWindow
	{	
		//			.								.								.
		private		const Int32						GWL_STYLE						= -16;
		private		const Int32						WS_MAXIMIZEBOX					= 0x00010000;
		private		const Int32						WS_MINIMIZEBOX					= 0x00020000;
		
		private		const uint						MF_BYCOMMAND					= 0x00000000;
		private		const uint						MF_GRAYED						= 0x00000001;
		private		const uint						SC_CLOSE = 0xF060;

		private		const int						WM_SHOWWINDOW					= 0x00000018;
		private		const int						WM_CLOSE						= 0x10;

		[ DllImport( "User32.dll", EntryPoint = "GetWindowLong" ) ]
		private extern static Int32 GetWindowLongPtr( IntPtr hWnd, Int32 nIndex );

		[ DllImport( "User32.dll", EntryPoint = "SetWindowLong" ) ]
		private extern static Int32 SetWindowLongPtr( IntPtr hWnd, Int32 nIndex, Int32 dwNewLong );

		[ DllImport( "user32.dll" ) ]
		private static extern IntPtr GetSystemMenu( IntPtr hWnd, bool bRevert );

		[ DllImport( "user32.dll" ) ]
		private static extern bool EnableMenuItem( IntPtr hMenu, uint uIDEnableItem, uint uEnable );

		public static IntPtr HwndSourceHook( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			switch( msg )
			{
				case WM_SHOWWINDOW:
				{
					IntPtr			hMenu			= GetSystemMenu( hwnd, false );
					if( hMenu != IntPtr.Zero )
					{
						EnableMenuItem( hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED );
					}

					break;
				}				
			}

			return IntPtr.Zero;
		}

		public static void DisableMaximize( System.Windows.Window wnd )
		{
			lock( wnd )
			{
				IntPtr			hWnd			= new WindowInteropHelper( wnd ).Handle;
				Int32			nStyle			= GetWindowLongPtr( hWnd, GWL_STYLE );
				
				SetWindowLongPtr( hWnd, GWL_STYLE, nStyle & ~WS_MAXIMIZEBOX);
			}
		}
		
		public static void DisableMinimize( System.Windows.Window wnd )
		{
			lock( wnd )
			{
				IntPtr			hWnd			= new WindowInteropHelper( wnd ).Handle;
				Int32			nStyle			= GetWindowLongPtr( hWnd, GWL_STYLE );
				
				SetWindowLongPtr( hWnd, GWL_STYLE, nStyle & ~WS_MINIMIZEBOX );
			}
		}

		public static void EnableMaximize( System.Windows.Window wnd )
		{
			lock( wnd )
			{
				IntPtr			hWnd			= new WindowInteropHelper( wnd ).Handle;
				Int32			nStyle			= GetWindowLongPtr( hWnd, GWL_STYLE );
				
				SetWindowLongPtr( hWnd, GWL_STYLE, nStyle | WS_MAXIMIZEBOX );
			}
		}

		public static void EnableMinimize( System.Windows.Window wnd )
		{
			lock( wnd )
			{
				IntPtr			hWnd			= new WindowInteropHelper( wnd ).Handle;
				Int32			nStyle			= GetWindowLongPtr( hWnd, GWL_STYLE );
				
				SetWindowLongPtr( hWnd, GWL_STYLE, nStyle | WS_MINIMIZEBOX );
			}
		}

		public static void ToggleMaximize( System.Windows.Window wnd )
		{
			lock( wnd )
			{
				IntPtr			hWnd			= new WindowInteropHelper( wnd ).Handle;
				Int32			nStyle			= GetWindowLongPtr( hWnd, GWL_STYLE );
				
				if( (nStyle | WS_MAXIMIZEBOX ) == nStyle )
				{
					SetWindowLongPtr( hWnd, GWL_STYLE, nStyle & ~WS_MAXIMIZEBOX );
				}
				else
				{
					SetWindowLongPtr( hWnd, GWL_STYLE, nStyle | WS_MAXIMIZEBOX );
				}
			}
		}
		
		public static void ToggleMinimize( System.Windows.Window wnd )
		{
			lock( wnd )
			{
				IntPtr			hWnd			= new WindowInteropHelper( wnd ).Handle;
				Int32			nStyle			= GetWindowLongPtr( hWnd, GWL_STYLE );
				
				if( ( nStyle | WS_MINIMIZEBOX ) == nStyle )
				{
					SetWindowLongPtr( hWnd, GWL_STYLE, nStyle & ~WS_MINIMIZEBOX );
				}
				else
				{
					SetWindowLongPtr( hWnd, GWL_STYLE, nStyle | WS_MINIMIZEBOX );
				}
			}
		}
	}
}
