using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using MetaScope.Models;
using MetaScope.Services.Error;

namespace MetaScope.Services
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

		/// <summary>
		/// UI layer sets this to provide a confirmation dialog for unsaved files.
		/// Signature: Func(message, title) => true if user chose Yes.
		/// If null, defaults to saving without confirmation.
		/// </summary>
		public static Func<string, string, bool>	ConfirmSaveFunc;

		/// <summary>
		/// UI layer sets this to provide a Save File dialog.
		/// Signature: Func(defaultExtension) => chosen file path, or null if cancelled.
		/// If null, DoFileSaveAs returns false.
		/// </summary>
		public static Func<string, string>			SaveFileDialogFunc;

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

				// Confirm via delegate set by MainWindow
				bool			bSave			= false;
				if( ConfirmSaveFunc != null )
				{
					bSave		= ConfirmSaveFunc( str, "File Save" );
				}

				if( bSave == true )
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

			// Save dialog via delegate set by MainWindow
			if( SaveFileDialogFunc == null )
				return false;

			string			strNewFile		= SaveFileDialogFunc( ".gff" );

			if( strNewFile != null )
			{
				df.File			= strNewFile;
				df.DoSave();
				return true;
			}

			return false;
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
