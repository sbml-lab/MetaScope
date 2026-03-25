using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace VugMap.Utility.Logger
{
	sealed class Logger
	{
		private		static string					S_STRFILE						= null;
		private		static bool						S_BFLUSHCONSOLE					= true;
		private		static bool						S_BFLUSHFILE					= false;
		private		static StreamWriter				S_SWFILE						= null;
		
		public static void SetFlushConsole( bool bFlushConsole )
		{
			S_BFLUSHCONSOLE	= bFlushConsole;
		}
		
		public static void SetFlushFile( bool bFlushFile, string strFile )
		{
			S_BFLUSHFILE	= bFlushFile;			
			
			if( bFlushFile == true )
			{
				S_STRFILE		= strFile;
				S_SWFILE		= new StreamWriter( S_STRFILE );
			}
			else
			{
				S_STRFILE		= null;
				DoCloseFileStream();			
			}
		}
		
		private static void DoCloseFileStream()
		{
			if( S_SWFILE != null )
			{
				S_SWFILE.Flush();
				S_SWFILE.Close();
				S_SWFILE		= null;
			}
		}
		
		public static void DoDispose()
		{
			DoCloseFileStream();
		}
		
		public static void PrintLine( string strFormat, params object[] objArgumentA )
		{
			string			strPrint		= string.Format( strFormat, objArgumentA );
			
			PrintLine( strPrint );
		}
		
		public static void PrintLine( string strPrint )
		{
			strPrint		= strPrint + "\r\n";
			Print( strPrint );
		}
		
		public static void Print( string strFormat, params object[] objArgumentA )
		{
			string			strPrint		= string.Format( strFormat, objArgumentA );
			
			Print( strPrint );
		}
		
		public static void Print( string strPrint )
		{
			if( S_BFLUSHCONSOLE == true )
			{
				Console.Write( strPrint );
			}
			
			if( S_BFLUSHFILE == true )
			{
				S_SWFILE.Write( strPrint );
				S_SWFILE.Flush();
			}
		}
	}
}
