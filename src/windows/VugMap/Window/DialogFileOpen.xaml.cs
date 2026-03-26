using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Interop;

using VugMap.Utility;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;
using VugMap.Utility.Reader;

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogFileOpen.xaml
	/// </summary>
	public partial class DialogFileOpen : System.Windows.Window
	{
		//			.								.								.
		private		string[]						m_strFileA						= null;
		private		bool							m_bEndWork						= false;

		public		delegate void DelegateDoReadFileThreadEnd();
		public		delegate void DelegateDoLoadingUpdateUI( long lCurrent, long lTotal );
		public		delegate void DelegateDoReadFileText( string strFile );

		public DialogFileOpen()
		{
			InitializeComponent();
		}

		protected override void OnSourceInitialized( EventArgs ea )
		{
			base.OnSourceInitialized( ea );

			HwndSource		hwndSource		= PresentationSource.FromVisual( this ) as HwndSource;

			if( hwndSource != null )
			{
				hwndSource.AddHook( UtilityWindow.HwndSourceHook );
			}
		}

		public bool EndWork
		{
			get {	return m_bEndWork; }
			set {	m_bEndWork = value; }
		}

		public void SetFile( string[] strFileA )
		{
			m_strFileA		= strFileA;
		}

		public bool DoReadFile()
		{
			ManagerData		md				= ManagerData.GetManager();

			m_tbFileOpenFiles.Text			= null;

			foreach( string strFile in m_strFileA )
			{
				if( md.IsContainingFile( strFile ) == true )
				{
					ErrorMessage.ShowErrorFileAlreadyOpen( strFile );
					return false;
				}

				m_tbFileOpenFiles.Text		+= strFile.Trim() + "\r\n";
				Logger.PrintLine( "# DialogFileOpen:DoReadFile, {0}", strFile );
			}

			Thread			thd				= new Thread( new ThreadStart( DoReadFileThread ) );
			thd.Start();

			return true;
		}

		private void DoReadFileThread()
		{
			ManagerData		md				= ManagerData.GetManager();
			Debug.Assert( md != null );

			try
			{
				foreach( string strFile in m_strFileA )
				{
					Logger.PrintLine( "# DialogFileOpen::DoReadFileThread() {0}", strFile );

					FileInfo		fi				= new FileInfo( strFile );
					if( fi.Exists == false )
					{
						ErrorMessage.ShowErrorFileNotFound( strFile );
						continue;
					}

					string			strExt			= fi.Extension.Substring( 1 );

					switch( strExt.ToLower() )
					{
						case "gff" :
						{
							string			str				= strFile.Trim();

							Dispatcher.BeginInvoke( new DelegateDoReadFileText( DoReadFileText ), DispatcherPriority.Normal, str );

							ReaderGff		rdr				= new ReaderGff( str );
							rdr.DataFile.IsReadOnly			= ( fi.Length >= Utility.AppSetting.ReadOnlyThresholdBytes );

							if( rdr.DataFile.IsReadOnly )
								DataFeature.SkipAttributeStorage	= true;

							rdr.LoadingUpdate				= new ReaderGff.DelegateDoLoadingUpdate( DoLoadingUpdate );

							bool			b				= rdr.DoReadFile();

							DataFeature.SkipAttributeStorage	= false;

							if( b == true )
							{
								md.DoDataFileAdd( rdr.DataFile );
							}

							break;
						}

						case "gz" :
						case "gzip" :
						{
							string			str				= strFile.Trim();
							Dispatcher.BeginInvoke( new DelegateDoReadFileText( DoReadFileText ), DispatcherPriority.Normal, str );
							Stream			stmFile			= null;
							GZipStream		stmGzip			= null;
							try
							{
								stmFile			= File.OpenRead( str );
								stmGzip			= new GZipStream( stmFile, CompressionMode.Decompress );
								ReaderGff		rdr				= new ReaderGff( str, stmGzip );
								rdr.DataFile.IsReadOnly			= true;
								DataFeature.SkipAttributeStorage	= true;
								rdr.LoadingUpdate				= new ReaderGff.DelegateDoLoadingUpdate( DoLoadingUpdate );
								bool			b				= rdr.DoReadFile();
								DataFeature.SkipAttributeStorage	= false;
								if( b == true )
								{
									md.DoDataFileAdd( rdr.DataFile );
								}
							}
							finally
							{
								if( stmGzip != null )		stmGzip.Close();
								if( stmFile != null )		stmFile.Close();
							}
							break;
						}
						case "zip" :
						{
							string			str				= strFile.Trim();
							Dispatcher.BeginInvoke( new DelegateDoReadFileText( DoReadFileText ), DispatcherPriority.Normal, str );
							using( ZipArchive za = ZipFile.OpenRead( str ) )
							{
								foreach( ZipArchiveEntry entry in za.Entries )
								{
									if( entry.FullName.EndsWith( ".gff", StringComparison.OrdinalIgnoreCase ) == false )
										continue;
									Stream			stmEntry		= null;
									try
									{
										stmEntry		= entry.Open();
										ReaderGff		rdr				= new ReaderGff( str, stmEntry );
										rdr.DataFile.IsReadOnly			= true;
										DataFeature.SkipAttributeStorage	= true;
										rdr.LoadingUpdate				= new ReaderGff.DelegateDoLoadingUpdate( DoLoadingUpdate );
										bool			b				= rdr.DoReadFile();
										DataFeature.SkipAttributeStorage	= false;
										if( b == true )
										{
											md.DoDataFileAdd( rdr.DataFile );
										}
									}
									finally
									{
										if( stmEntry != null )		stmEntry.Close();
									}
								}
							}
							break;
						}
						case "workspace" :
						{
							// Do nothing
							break;
						}

						default :
						{
							ErrorMessage.ShowErrorFileNotSupported( strExt );
							continue;
						}
					}
				}
			}
			catch( Exception e )
			{
				Logger.PrintLine( "# ERROR, DialogFileOpen:DoReadFileThread - {0}", e.ToString() );
				ErrorMessage.ShowError( string.Format( "An error occurred while loading files.\r\n\r\n{0}", e.Message ) );
			}

			Dispatcher.BeginInvoke( new DelegateDoReadFileThreadEnd( DoReadFileThreadEnd ), DispatcherPriority.Normal, null );
		}

		private void DoReadFileText( string strFile )
		{
			string			strName			= UtilityFile.GetFileName( strFile );

			m_tbFileOpen.Text				= strName;
		}

		private void DoLoadingUpdate( long lCurrent, long lTotal )
		{
			Dispatcher.BeginInvoke( new DelegateDoLoadingUpdateUI( DoLoadingUpdateUI ), DispatcherPriority.Normal, lCurrent, lTotal );
		}

		private void DoLoadingUpdateUI( long lCurrent, long lTotal )
		{
			if( lTotal <= 0 )
			{
				m_pgbFileOpenReading.IsIndeterminate		= true;
			}
			else
			{
				m_pgbFileOpenReading.IsIndeterminate		= false;
				m_pgbFileOpenReading.Minimum				= 0;
				m_pgbFileOpenReading.Maximum				= lTotal;
				m_pgbFileOpenReading.Value				= lCurrent;
			}
		}

		private void DoReadFileThreadEnd()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();

			mw.DoMapUpdate( m_strFileA );

			foreach( string strFile in m_strFileA )
			{
				AppSetting.DoRecentGffAdd( strFile );
			}
			mw.DoMruMenuUpdate();

			if( m_bEndWork == true )
			{
				m_bEndWork		= false;

				mw.DoDropEndWork();
			}

			Close();
		}
	}
}
