using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MetaScope.Services
{
	public static class AppSetting
	{
		// 20 MB = 20,000,000 bytes. Load read-only mode if file size exceeds this
		public		static long						ReadOnlyThresholdBytes			= 20000000;

		private		static List< string >			s_lstRecentWorkspace			= new List< string >();
		private		static List< string >			s_lstRecentGff					= new List< string >();
		private		static readonly int				N_MRU_MAX						= 5;

		private		static readonly string			s_strAppDataDir;
		private		static readonly string			s_strSettingPath;

		private		static readonly string			S_KEY_AUTOSAVE					= "AutoSave";

		static AppSetting()
		{
			string			strAppData		= RuntimeInformation.IsOSPlatform( OSPlatform.OSX )
												? Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ),
													"Library", "Application Support", "MetaScope" )
												: Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
													"MetaScope" );

			s_strAppDataDir					= strAppData;
			if( !Directory.Exists( s_strAppDataDir ) )
				Directory.CreateDirectory( s_strAppDataDir );

			s_strSettingPath				= Path.Combine( s_strAppDataDir, "MetaScope.setting" );

			// Migration: exe dir setting file exists and AppData does not — move it
			string			strOldPath		= Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "MetaScope.setting" );
			if( File.Exists( strOldPath ) && !File.Exists( s_strSettingPath ) )
			{
				try { File.Move( strOldPath, s_strSettingPath ); } catch { }
			}

			if( File.Exists( s_strSettingPath ) )
			{
				try
				{
					ReaderSetting	rs	= new ReaderSetting( s_strSettingPath );

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

					// Migration: read legacy RecentFile entries
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

		public static bool AutoSave
		{
			get
			{
				try
				{
					if( File.Exists( s_strSettingPath ) )
					{
						ReaderSetting	rs	= new ReaderSetting( s_strSettingPath );
						string			val	= rs[ S_KEY_AUTOSAVE ];
						if( val != null )
							return Convert.ToBoolean( val );
					}
				}
				catch {	}
				return false;
			}
			set
			{
				try
				{
					DoSave( value );
				}
				catch {	}
			}
		}

		public static void DoSave()
		{
			DoSave( null );
		}

		private static void DoSave( bool? bAutoSaveOverride )
		{
			try
			{
				// Read current AutoSave value before overwriting file
				bool			bAutoSave		= false;
				if( bAutoSaveOverride.HasValue )
				{
					bAutoSave				= bAutoSaveOverride.Value;
				}
				else
				{
					// Preserve existing AutoSave value from file
					try
					{
						if( File.Exists( s_strSettingPath ) )
						{
							ReaderSetting	rs	= new ReaderSetting( s_strSettingPath );
							string			val	= rs[ S_KEY_AUTOSAVE ];
							if( val != null )
								bAutoSave	= Convert.ToBoolean( val );
						}
					}
					catch { }
				}

				using( StreamWriter sw = new StreamWriter( s_strSettingPath ) )
				{
					sw.WriteLine( string.Format( "ReadOnlyThreshold={0}", ReadOnlyThresholdBytes ) );
					sw.WriteLine( string.Format( "{0}={1}", S_KEY_AUTOSAVE, bAutoSave ) );

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
