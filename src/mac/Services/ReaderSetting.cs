using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using MetaScope.Services.Error;

namespace MetaScope.Services
{
    public class ReaderSetting
    {
		//			.								.								.
        private		ArrayList						m_alItem						= null;
        private		ArrayList						m_alValue						= null;

        public ReaderSetting( string strFile )
        {
            m_alItem		= new ArrayList( 20 );
            m_alValue		= new ArrayList( 20 );

            ReadFile( strFile );
        }

        private void ReadFile( string strFile )
        {
            Debug.Assert( m_alItem != null );
            Debug.Assert( m_alValue != null );

            StreamReader	sr				= null;

            try
            {
                Stream			stm				= ( System.IO.Stream ) File.OpenRead( strFile );
                sr				= new StreamReader( stm, System.Text.Encoding.UTF8 );

                sr.BaseStream.Seek( 0, SeekOrigin.Begin );

                while (sr.Peek() > -1)
                {
                    string strLine = sr.ReadLine();
                    strLine = strLine.Trim();

                    // First character is # means comment
                    if (strLine != "" && strLine[0] != '#')
                    {
                        int nEqual = strLine.IndexOf( "=" );

                        if (nEqual == -1)
                        {
                            // No = sign found
                            ErrorMessage.ShowErrorFileInvalid( strFile );
                            break;
                        }

                        string strItem        = strLine.Substring( 0, nEqual );
                        string strValue        = strLine.Substring( nEqual + 1, strLine.Length - nEqual - 1 );

                        strItem = strItem.Trim();
                        strValue = strValue.Trim();

                        m_alItem.Add( strItem );
                        m_alValue.Add( strValue );

                        // FIXME : Need routine to check for duplicate Item values.
                    }

                    // Item count and Value count must be equal.
                    Debug.Assert( m_alItem.Count == m_alValue.Count );
                }
            }
            finally
            {
                if( sr != null )		sr.Close();
            }
        }

        public int Count
        {
            get
            {
                return m_alItem.Count;
            }
        }

        public string this[ int nIndex ]
        {
            get
            {
                return GetValue( nIndex );
            }
        }

        public string this[ string strItem ]
        {
            get
            {
                return GetValue( strItem );
            }
        }

        public string GetValue( string strItem )
        {
            int nIndex = m_alItem.IndexOf( strItem );
            if( nIndex == -1 )
            {
                // No matching item found, return null
                return null;
            }

            return m_alValue[ nIndex ].ToString();
        }

        public string GetValue( int nIndex )
        {
            if( nIndex < 0 || nIndex >= m_alItem.Count )
            {
                return null;
            }

            return m_alValue[ nIndex ].ToString();
        }

        public string GetItem( int nIndex )
        {
            if( nIndex < 0 || nIndex >= m_alItem.Count )
            {
                return null;
            }

            return m_alItem[ nIndex ].ToString();
        }
    }
}
