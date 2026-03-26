using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

using MetaScope.Models;
using MetaScope.Services.Error;

namespace MetaScope.Services
{
	public class ReaderGff
	{
		private		string							m_strFile						= null;
		private		DataFile						m_dfData						= null;
		private		DelegateDoLoadingUpdate			m_delUpdate						= null;
		private		string							m_strHeader						= null;
		private		Stream							m_stmExternal					= null;		// External stream (gz/zip)

		public		delegate void DelegateDoLoadingUpdate( long lCurrent, long lTotal );

		public ReaderGff( string strFile )
		{
			m_strFile		= strFile;
			m_dfData		= new DataFile( null, strFile );
		}

		public ReaderGff( string strFile, Stream stm )
		{
			m_strFile		= strFile;
			m_dfData		= new DataFile( null, strFile );
			m_stmExternal	= stm;
		}

		public DelegateDoLoadingUpdate LoadingUpdate
		{
			get {	return m_delUpdate; }
			set {	m_delUpdate		= value; }
		}

		public DataFile DataFile
		{
			get {	return m_dfData; }
		}

		public bool DoReadFile()
		{
			if( m_stmExternal == null )
			{
				bool			bExist			= File.Exists( m_strFile );
				if( bExist == false )
				{
					return false;
				}
			}

			StreamReader	srFile			= null;
			Stream			stm				= null;

			DateTime		dtStart			= DateTime.Now;
			int				nCountLine		= 0;
			int				nCountSkipped	= 0;

			try
			{
				if( m_stmExternal != null )
				{
					stm				= m_stmExternal;
				}
				else
				{
					stm				= ( Stream ) File.OpenRead( m_strFile );
				}

				srFile			= new StreamReader( stm, System.Text.Encoding.UTF8 );

				if( stm.CanSeek )
					srFile.BaseStream.Seek( 0, SeekOrigin.Begin );

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				bool			bHeader			= true;
				long			lTotal			= stm.CanSeek ? stm.Length : 0;

				while( srFile.Peek() > -1 )
				{
					string			strLine			= srFile.ReadLine();
					strLine							= strLine.Trim();

					sw.Stop();

					if( m_delUpdate != null && sw.ElapsedMilliseconds >= 100 )
					{
						long			lCurrent		= stm.CanSeek ? srFile.BaseStream.Position : nCountLine;
						long			lTotalReport	= stm.CanSeek ? lTotal : 0;

						m_delUpdate( lCurrent, lTotalReport );
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					if( strLine == "" )
					{
						continue;
					}

					if( strLine.StartsWith( "#" ) == true )		// comments
					{
						if( bHeader == true )
						{
							m_strHeader		+= string.Format( "{0}{1}", strLine, "\r\n" );
							continue;
						}
						else
						{
							if( strLine.StartsWith( "##FASTA" ) == true )
							{
								break;
							}

							if( strLine == "###" )
							{
								break;
							}
						}
					}

					bHeader							= false;

					string[]		strItemA		= strLine.Split( '\t' );
					if( ( strItemA.Length >= 5 ) == false )
						throw ExceptionInvalidFormat.MakeException( string.Format( "The data line ({0}) is invalid.", nCountLine ) );

					string			strSequence		= strItemA[ 0 ];
					string			strSource		= strItemA[ 1 ];
					string			strFeature		= strItemA[ 2 ];
					string			strStart		= strItemA[ 3 ];
					string			strEnd			= strItemA[ 4 ];
					string			strScore		= ".";
					string			strStrand		= ".";
					string			strFrame		= ".";
					string			strAttribute	= "";

					if( strItemA.Length >= 6 )		strScore		= strItemA[ 5 ];
					if( strItemA.Length >= 7 )		strStrand		= strItemA[ 6 ];
					if( strItemA.Length >= 8 )		strFrame		= strItemA[ 7 ];
					if( strItemA.Length >= 9 )		strAttribute	= strItemA[ 8 ];

					int				nStart			= 0;
					int				nEnd			= 0;
					double			dScore			= 0.0f;

					try
					{
						nStart			= Int32.Parse( strStart );
						nEnd			= Int32.Parse( strEnd );
						if( strScore == "." )			dScore			= double.NaN;
						else if( strScore != "." )		dScore			= double.Parse( strScore );
					}
					catch( SystemException )
					{
						Logger.PrintLine( "# WARNING, ReaderGff:DoReadFile - skipped line {0} (parse error)", nCountLine );
						nCountSkipped++;
						nCountLine++;
						continue;
					}

					DataFeature		df				= new DataFeature( strSource, nStart, nEnd, dScore, strStrand, strFrame, strAttribute );

					m_dfData.AddFeature( strSequence, strFeature, df );

					nCountLine++;
				}

				//Logger.PrintLine( string.Format( "# ReaderGff:DoReadFile() {0} lines read", nCountLine ) );
			}
			catch( Exception e )
			{
				string			strException	= e.Message;
				string			strLogException	= string.Format( "# ERROR, ReaderGff:DoReadFile - {0}", strException );
				Logger.PrintLine( strLogException );

				string			strName			= UtilityFile.GetFileName( m_strFile );

				ErrorMessage.ShowErrorFileInvalid( m_strFile, strException );
				m_dfData						= null;

				return false;
			}
			finally
			{
				if( srFile != null )
					srFile.Close();
			}

			m_dfData.Header	= m_strHeader;
			m_dfData.BuildIndex();

			if( m_delUpdate != null )
			{
				m_delUpdate( 100, 100 );
			}

			TimeSpan		ts				= DateTime.Now - dtStart;

			string			strLog			= string.Format( "# ReaderGff:DoReadFile - {0} types, {1} features, {2} skipped ({3})",
															  m_dfData.GetCountDataType(), m_dfData.GetCountFeature(), nCountSkipped, ts.ToString() );
			Logger.PrintLine( strLog );

			if( nCountSkipped > 0 )
			{
				Logger.PrintLine( "# WARNING, ReaderGff:DoReadFile - {0} lines skipped due to parse errors", nCountSkipped );
			}

			return true;
		}

	}
}
