using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MetaScope.Services
{
	public class UtilityFile
	{
		public static bool GetFileExist( string strFile )
		{
			FileInfo		fi				= null;

			try
			{
				fi				= new FileInfo( strFile );
				if( fi.Exists == true )
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch( Exception )
			{
				return false;
			}
		}

		public static string GetFileName( string strFile )
		{
			bool			bExist			= GetFileExist( strFile );

			if( bExist == false )
			{
				return null;
			}
			else
			{
				FileInfo		fi				= new FileInfo( strFile );
				string			strName			= fi.Name;

				return strName;
			}
		}
	}
}
