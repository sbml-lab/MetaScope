using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace VugMap.Utility
{
	public static class AppSetting
	{
		// 20 MB = 20,000,000 bytes. 이 크기 이상이면 read-only 모드로 로딩
		public		static long						ReadOnlyThresholdBytes			= 20000000;

		private		static List< string >			s_lstRecentWorkspace			= new List< string >();
		private		static List< string >			s_lstRecentGff					= new List< string >();
		private		static readonly int				N_MRU_MAX						= 5;

		private		static readonly string			s_strAppDataDir;
		private		static readonly string			s_strSettingPath;

		static AppSetting()
		{
			s_strAppDataDir					= Path.Combine(
												Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
												"MetaScope" );
			if( !Directory.Exists( s_strAppDataDir ) )
				Directory.CreateDirectory( s_strAppDataDir );

			s_strSettingPath				= Path.Combine( s_strAppDataDir, "MetaScope.setting" );

			// Migration: exe 옆 setting 파일이 있고 AppData에 없으면 이동
			string			strOldPath		= Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "MetaScope.setting" );
			if( File.Exists( strOldPath ) && !File.Exists( s_strSettingPath ) )
			{
				try { File.Move( strOldPath, s_strSettingPath ); } catch { }
			}

			if( File.Exists( s_strSettingPath ) )
			{
				try
				{
					Reader.ReaderSetting	rs	= new Reader.ReaderSetting( s_strSettingPath );

					string		strThreshold	= rs[ "ReadOnlyThreshold" ];
					if( strThreshold != null )
					{
						ReadOnlyThresholdBytes	= long.Parse( strThreshold );
					}

					for( int i = 1; i <= N_MRU_MAX; i++ )
					{
						string	strRecent		= rs[ string.Format( "RecentWorkspace{0}", i ) ];
						if( strRecent != null && File.Exists( strRecent ) )
						{
							s_lstRecentWorkspace.Add( strRecent );
						}
					}

					for( int i = 1; i <= N_MRU_MAX; i++ )
					{
						string	strRecent		= rs[ string.Format( "RecentGff{0}", i ) ];
						if( strRecent != null && File.Exists( strRecent ) )
						{
							s_lstRecentGff.Add( strRecent );
						}
					}

					// Migration: 기존 RecentFile 항목 읽기
					if( s_lstRecentWorkspace.Count == 0 && s_lstRecentGff.Count == 0 )
					{
						for( int i = 1; i <= 10; i++ )
						{
							string	strRecent		= rs[ string.Format( "RecentFile{0}", i ) ];
							if( strRecent != null && File.Exists( strRecent ) )
							{
								s_lstRecentGff.Add( strRecent );
								if( s_lstRecentGff.Count >= N_MRU_MAX )
									break;
							}
						}
					}
				}
				catch
				{
				}
			}
		}

		public static string AppDataDir
		{
			get {	return s_strAppDataDir; }
		}

		public static List< string > RecentWorkspaceList
		{
			get {	return s_lstRecentWorkspace; }
		}

		public static List< string > RecentGffList
		{
			get {	return s_lstRecentGff; }
		}

		public static void DoRecentWorkspaceAdd( string strFile )
		{
			s_lstRecentWorkspace.Remove( strFile );
			s_lstRecentWorkspace.Insert( 0, strFile );

			while( s_lstRecentWorkspace.Count > N_MRU_MAX )
			{
				s_lstRecentWorkspace.RemoveAt( s_lstRecentWorkspace.Count - 1 );
			}

			DoSave();
		}

		public static void DoRecentGffAdd( string strFile )
		{
			s_lstRecentGff.Remove( strFile );
			s_lstRecentGff.Insert( 0, strFile );

			while( s_lstRecentGff.Count > N_MRU_MAX )
			{
				s_lstRecentGff.RemoveAt( s_lstRecentGff.Count - 1 );
			}

			DoSave();
		}

		private static readonly string		S_REGKEY		= @"Software\MetaScope";

		public static bool AutoSave
		{
			get
			{
				try
				{
					using( RegistryKey key = Registry.CurrentUser.OpenSubKey( S_REGKEY ) )
					{
						if( key != null )
						{
							object	val		= key.GetValue( "AutoSave" );
							if( val != null )
								return Convert.ToBoolean( val );
						}
					}
				}
				catch {	}
				return false;
			}
			set
			{
				try
				{
					using( RegistryKey key = Registry.CurrentUser.CreateSubKey( S_REGKEY ) )
					{
						key.SetValue( "AutoSave", value );
					}
				}
				catch {	}
			}
		}

		public static void DoSave()
		{
			try
			{
				using( StreamWriter sw = new StreamWriter( s_strSettingPath ) )
				{
					sw.WriteLine( string.Format( "ReadOnlyThreshold={0}", ReadOnlyThresholdBytes ) );

					for( int i = 0; i < s_lstRecentWorkspace.Count; i++ )
					{
						sw.WriteLine( string.Format( "RecentWorkspace{0}={1}", i + 1, s_lstRecentWorkspace[ i ] ) );
					}

					for( int i = 0; i < s_lstRecentGff.Count; i++ )
					{
						sw.WriteLine( string.Format( "RecentGff{0}={1}", i + 1, s_lstRecentGff[ i ] ) );
					}
				}
			}
			catch
			{
			}
		}
	}
}
