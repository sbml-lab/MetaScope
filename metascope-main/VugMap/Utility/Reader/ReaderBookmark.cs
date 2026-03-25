using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using VugMap.Utility.Data;

namespace VugMap.Utility.Reader
{
	using			ListBookmark					= List< DataBookmark >;

	public class ReaderBookmark
	{
		//			.								.								.		
		private		string							m_strFile						= null;
		private		ListBookmark					m_lstBookmark					= null;        

		public ReaderBookmark( string strFile )
		{
			m_strFile		= strFile;
			m_lstBookmark	= new ListBookmark( 100 );            
                                                             
			ReadFile();
		}

        private void ReadFile()
        {
			Debug.Assert( m_lstBookmark != null );            

			StreamReader	sr				= new StreamReader( (System.IO.Stream) File.OpenRead( m_strFile ), System.Text.Encoding.Default );
                
			sr.BaseStream.Seek( 0, SeekOrigin.Begin );

			while( sr.Peek() > -1 )
			{
				string			strLine			= sr.ReadLine();
				strLine.Trim();

				// 첫번째 문자가 #이면 주석이다.
				if( strLine != "" && strLine[0] != '#' )
				{
					continue;
				}

				string[]		strA			= strLine.Split( ',' );
				string			strSeqId		= strA[ 0 ].Trim();
				string			strTitle		= strA[ 1 ].Trim();
				string			strPosition		= strA[ 2 ].Trim();
				string			strZoom			= strA[ 3 ].Trim();

				int				nPosition		= Int32.Parse( strPosition );
				double			dZoom			= Double.Parse( strZoom );

				DataBookmark	db				= new DataBookmark( strSeqId, strTitle, nPosition, dZoom );

				m_lstBookmark.Add( db );
			}

			sr.Close();
        }

		public int Count
		{
			get {	return m_lstBookmark.Count; }
		}

		public ListBookmark GetBookmark()
		{
			return m_lstBookmark;
		}

		public DataBookmark this[ int nIndex ]
		{
			get {	return GetBookmark( nIndex ); }
		}

		public DataBookmark GetBookmark( int nIndex )
        {
			if( nIndex < 0 || nIndex >= m_lstBookmark.Count )
			{
				return null;
			}

			return m_lstBookmark[ nIndex ];
        }
	}
}
