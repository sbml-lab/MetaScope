using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

using VugMap;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Window;

namespace VugMap.Utility
{
	using		DicDataFile						= Dictionary< string, DataFile >;
	using		DicDataType						= Dictionary< string, DataType >;
	using		ListDataType					= List< DataType >;
	using		ListString						= List< string >;

	public class ManagerData
	{
		//			.								.								.
		private		static ManagerData				S_MANAGER						= null;

		public static ManagerData GetManager()
		{
			if( S_MANAGER == null  )
			{
				S_MANAGER		= new ManagerData();
			}
			
			return S_MANAGER;
		}
				
		private		DicDataFile						m_dicDataFile					= null;
		private		DicDataType						m_dicDataType					= null;
		private		ListString						m_lstSequenceId					= null;
		private		bool							m_bEdited						= false;
	private		int								m_nCachedFeatureCount			= 0;

		public ManagerData()
		{
			m_dicDataFile	= new DicDataFile();
			m_dicDataType	= new DicDataType();
			m_lstSequenceId	= new ListString();
		}

		public DataFile DoFileClose( string strFile )
		{
			DataFile		df				= GetDataFile( strFile );

			if( df == null )
				return null;
			
			if( df.IsEdited == true )
			{
				string			str				= string.Format( "The file \"{0}\" has been modified.\nDo you want to save it before close?", df.FileName );
				MessageBoxResult	mbr			= MessageBox.Show( str, "File Save", MessageBoxButton.YesNo, MessageBoxImage.Question );
				
				if( mbr == MessageBoxResult.Yes )
				{
					df.DoSave();
				}				
			}

			df.DoDispose();

			DoDataFileRemove( df );

			return df;
		}

		public void DoFileCloseAll()
		{			
			ListString		lst				= new ListString();

			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				lst.Add( kv.Value.File );
			}

			foreach( string strFile in lst )
			{
				DoFileClose( strFile );
			}

			m_dicDataFile.Clear();
			m_dicDataType.Clear();
			m_lstSequenceId.Clear();
			m_bEdited		= false;
			m_nCachedFeatureCount	= 0;
		}

		public ListString DoFileSaveAll()
		{
			ListString		lst				= new ListString();

			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				DataFile		df				= kv.Value;

				bool			bSaved			= DoFileSave( df );
				if( bSaved == true )
				{
					lst.Add( df.FileName );
				}
			}

			return lst;
		}

		public bool DoFileSave( DataFile df )
		{
			if( df.IsEdited == true )
			{
				df.DoSave();
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool DoFileSave( string strFile )
		{
			DataFile		df				= GetDataFile( strFile );
			Debug.Assert( df != null );

			bool			b				= DoFileSave( df );

			return b;
		}

		public bool DoFileSaveAs( string strFile )
		{
			DataFile		df				= GetDataFile( strFile );
			Debug.Assert( df != null );

			Microsoft.Win32.SaveFileDialog
							dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Save a Data File";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;			
			dlg.DefaultExt					= ".gff";
			dlg.Filter						= "GFF file (.gff)|*.gff";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strNewFile		= dlg.FileName;				

				df.File			= strNewFile;
				df.DoSave();
			}

			if( bResult == null )
				return false;
			else
				return bResult.Value;
		}

		public DataFile GetDataFile( string strFile )
		{
			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				if( kv.Value.File == strFile || kv.Value.FileName == strFile )
				{
					return kv.Value;
				}
			}

			return null;
		}

		public bool IsContainingSequenceId( string strSequenceId )
		{
			if( m_lstSequenceId.Contains( strSequenceId ) == true )
				return true;
			else
				return false;
		}

		public bool IsSequenceIdReadOnly( string strSequenceId )
		{
			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				for( int i = 0; i < kv.Value.GetCountDataType(); i++ )
				{
					DataType dt = kv.Value.GetDataType( i );
					if( dt.SequenceId == strSequenceId && kv.Value.IsReadOnly == false )
						return false;
				}
			}
			return true;
		}

		public bool IsContainingFile( string strFile )
		{
			if( m_dicDataFile.Keys.Contains( strFile ) == true )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public string GetSequenceId( int nIndex )
		{
			string			strSequenceId	= m_lstSequenceId[ nIndex ];

			return strSequenceId;
		}

		public int GetCountSequenceId()
		{
			int				nCount			= m_lstSequenceId.Count;

			return nCount;
		}

		public int GetCountDataFile()
		{
			int				nCount			= m_dicDataFile.Count;

			return nCount;
		}

		public int GetCountDataType( string strSequenceId )
		{
			int				nCount			= 0;

			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				nCount			+= kv.Value.GetCountDataType( strSequenceId );
			}

			return nCount;
		}

		public DataFile GetDataFile( int nIndex )
		{
			string			strFile			= m_dicDataFile.Keys.ElementAt( nIndex );
			DataFile		dfFile			= m_dicDataFile[ strFile ];

			return dfFile;
		}

		public DataType GetDataType( string strSequenceId, string strType )
		{
			string			strKey			= string.Format( "{0}:{1}", strSequenceId, strType );

			if( m_dicDataType.ContainsKey( strKey ) == false )
			{
				return null;
			}
			else
			{
				DataType		dt				= m_dicDataType[ strKey ];

				return dt;
			}		
		}
				
		public DataType GetDataType( string strSequenceId, int nIndex )
		{
			int				nIndex0			= 0;

			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				if( kv.Value.SequenceId == strSequenceId )
				{
					if( nIndex0 == nIndex )
					{
						return kv.Value;
					}

					nIndex0++;
				}
			}
			
			return null;
		}

		public int GetPositionMin( string strSequenceId )
		{
			int				nPositionMin	= int.MaxValue;

			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				nPositionMin	= Math.Min( nPositionMin, kv.Value.GetPositionMin( strSequenceId ) );				
			}

			return nPositionMin;			
		}
				
		public int GetPositionMax( string strSequenceId )
		{
			int				nPositionMax	= int.MinValue;

			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				nPositionMax	= Math.Max( nPositionMax, kv.Value.GetPositionMax( strSequenceId ) );
			}

			return nPositionMax;
		}

		public void DoDataFileRemove( DataFile df )
		{
			m_dicDataFile.Remove( df.File );

			DoDataFileUpdate();
			DoUpdateCachedFeatureCount();
		}

		private void DoDataFileUpdate()
		{
			m_dicDataType.Clear();
			m_lstSequenceId.Clear();

			foreach( KeyValuePair<string, DataFile> kv in m_dicDataFile )
			{
				for( int i = 0; i < kv.Value.GetCountDataType(); i++ )
				{
					DataType		dt				= kv.Value.GetDataType( i );
					string			strSequenceId	= dt.SequenceId;
					string			strType			= dt.Type;
					string			strKey			= string.Format( "{0}:{1}", strSequenceId, strType );

					m_dicDataType.Add( strKey, dt );

					if( m_lstSequenceId.Contains( strSequenceId ) == false )
						m_lstSequenceId.Add( strSequenceId );
				}
			}
		}
		
		public void DoDataFileAdd( DataFile df )
		{
			df.ManagerData					= this;
											
			m_dicDataFile.Add( df.File, df );

			for( int i = 0; i < df.GetCountDataType(); i++ )
			{
				DataType		dt				= df.GetDataType( i );
				string			strSequenceId	= dt.SequenceId;
				string			strType			= dt.Type;
				string			strKey			= string.Format( "{0}:{1}", strSequenceId, strType );

				if( m_dicDataType.Keys.Contains( strKey ) == true )
				{
					DoDataFileRemove( df );
					ErrorMessage.ShowErrorFileTypeExists( strKey );				
					continue;
				}				
			}

			DoDataFileUpdate();
			DoUpdateCachedFeatureCount();
		}

		public int CachedFeatureCount
		{
			get {	return m_nCachedFeatureCount; }
		}

		public void DoUpdateCachedFeatureCount()
		{
			int				nCount			= 0;
			foreach( KeyValuePair< string, DataFile > kv in m_dicDataFile )
			{
				nCount		+= kv.Value.GetCountFeature();
			}
			m_nCachedFeatureCount			= nCount;
		}

		public void SetEdited( bool bEdited )
		{
			m_bEdited			= bEdited;
		}

		public bool IsEdited
		{
			get
			{
				return m_bEdited;
			}

			set
			{
				SetEdited( value );
			}
		}
	}
}
