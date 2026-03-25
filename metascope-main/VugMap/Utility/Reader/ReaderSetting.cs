using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using VugMap.Utility.Error;

namespace VugMap.Utility.Reader
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
                sr				= new StreamReader( stm, System.Text.Encoding.Default );

                sr.BaseStream.Seek( 0, SeekOrigin.Begin );

                while (sr.Peek() > -1)
                {
                    string strLine = sr.ReadLine();
                    strLine = strLine.Trim();

                    // 첫번째 문자가 #이면 주석이다.
                    if (strLine != "" && strLine[0] != '#')
                    {
                        int nEqual = strLine.IndexOf( "=" );

                        if (nEqual == -1)
                        {
                            // =가 없으면
                            ErrorMessage.ShowErrorFileInvalid( strFile );
                            break;
                        }

                        string strItem        = strLine.Substring( 0, nEqual );
                        string strValue        = strLine.Substring( nEqual + 1, strLine.Length - nEqual - 1 );

                        strItem = strItem.Trim();
                        strValue = strValue.Trim();

                        m_alItem.Add( strItem );
                        m_alValue.Add( strValue );

                        // FIXME : 중복된 Item 값이 있는지 검사하는 루틴이 필요.
                    }

                    // Item 갯수와 Value 갯수는 같아야함.
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
                // 해당하는 항목이 없으면 null 반환                
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