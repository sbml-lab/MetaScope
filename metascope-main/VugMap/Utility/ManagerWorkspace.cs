using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.Web;

using VugMap.Utility.Data;
using VugMap.Utility.Reader;
using VugMap.Window;

namespace VugMap.Utility
{
	using	DicSetting						= Dictionary< string, string >;											
	using	ListWorkspaceData				= List< WorkspaceData >;
	using	ListWorkspaceMap				= List< WorkspaceMap >;
	using	ListWorkspaceLane				= List< WorkspaceLane >;
	using	ListWorkspaceType				= List< WorkspaceType >;
	using	ListBookmark					= List< DataBookmark >;
	
	[ XmlRootAttribute( ElementName = "Workspace", IsNullable = false ) ]
	public class ManagerWorkspace
	{
		//			.								.								.
		public		const string					STR_LAYOUT_FILE					= "Layout.File";
		public		const string					STR_LAYOUT_APPLICAIONSCALEX		= "Layout.Application.ScaleX";
		public		const string					STR_LAYOUT_APPLICAIONSCALEY		= "Layout.Application.ScaleY";
		public		const string					STR_LAYOUT_DOCUMENTSCALEX		= "Layout.Document.ScaleX";
		public		const string					STR_LAYOUT_DOCUMENTSCALEY		= "Layout.Document.ScaleY";

		public		const string					STR_STARTUP_SHOWINTRODUCTION	= "Startup.ShowIntroduction";

		public		const string					STR_DATAFILE					= "Data.File";
		public		const string					STR_DATA_FILE					= "File";
		public		const string					STR_DATA_FILEPATH				= "Path";
		public		const string					STR_DATAMAP						= "Data.Map";
		public		const string					STR_DATA_MAP					= "Map";
		public		const string					STR_DATA_MAPSEQUENCEID			= "SequenceId";
		public		const string					STR_DATA_MAPLANE				= "Map.Lane";
		public		const string					STR_DATA_LANE					= "Lane";
		public		const string					STR_DATA_TYPE					= "Type";
		public		const string					STR_DATA_MAPLANELIST			= "TypeList";
		public		const string					STR_DATA_TYPEHEIGHT				= "Height";
		public		const string					STR_DATA_TYPESCALEMAX			= "ScaleMax";
		public		const string					STR_DATA_TYPESCALEMIN			= "ScaleMin";
		public		const string					STR_DATA_TYPECOLOR				= "Color";
		public		const string					STR_DATA_TYPETEXT				= "Text";
		public		const string					STR_DATA_TYPEDISPLAY			= "Display";
		public		const string					STR_DATA_BOOKMARKLIST			= "Bookmark.List";
		public		const string					STR_DATA_BOOKMARK				= "Bookmark";
		public		const string					STR_DATA_BOOKMARKSEQUENCEID		= "SequenceId";
		public		const string					STR_DATA_BOOKMARKTITLE			= "Title";
		public		const string					STR_DATA_BOOKMARKPOSITION		= "Position";
		public		const string					STR_DATA_BOOKMARKZOOM			= "Zoom";

		private		string							m_strFile						= null;
		private		DicSetting						m_dicSetting					= null;
		private		ListWorkspaceData				m_lstData						= null;
		private		ListWorkspaceMap				m_lstMap						= null;
		private		ListBookmark					m_lstBookmark					= null;
		private		bool							m_bEdited						= false;
		
		public ManagerWorkspace()
		{
			m_dicSetting	= new DicSetting();
			m_lstData		= new ListWorkspaceData();
			m_lstMap		= new ListWorkspaceMap();
			m_lstBookmark	= new ListBookmark();
			m_strFile		= Path.Combine( AppDomain.CurrentDomain.BaseDirectory, Constant.S_APP_SETTING );
	
			LayoutApplicationScaleX			= 1.0;
			LayoutApplicationScaleY			= 1.0;
			LayoutDocumentScaleX			= 1.0;
			LayoutDocumentScaleY			= 1.0;

			m_bEdited		= false;
		}
				
		[ XmlElement( ElementName = STR_LAYOUT_FILE, DataType = "string" ) ]
		public string LayoutFile
		{
			get {	return GetSetting( STR_LAYOUT_FILE ); }
			set {	SetSetting( STR_LAYOUT_FILE, value ); }
		}

		[ XmlElement( ElementName = STR_LAYOUT_APPLICAIONSCALEX, DataType = "double" ) ]
		public double LayoutApplicationScaleX
		{
			get {	return GetSettingDouble( STR_LAYOUT_APPLICAIONSCALEX ); }
			set {	SetSetting( STR_LAYOUT_APPLICAIONSCALEX, value ); }
		}

		[ XmlElement( ElementName = STR_LAYOUT_APPLICAIONSCALEY, DataType = "double" ) ]
		public double LayoutApplicationScaleY
		{
			get {	return GetSettingDouble( STR_LAYOUT_APPLICAIONSCALEY ); }
			set {	SetSetting( STR_LAYOUT_APPLICAIONSCALEY, value ); }
		}

		[ XmlElement( ElementName = STR_LAYOUT_DOCUMENTSCALEX, DataType = "double" ) ]
		public double LayoutDocumentScaleX
		{
			get {	return GetSettingDouble( STR_LAYOUT_DOCUMENTSCALEX ); }
			set {	SetSetting( STR_LAYOUT_DOCUMENTSCALEX, value ); }
		}

		[ XmlElement( ElementName = STR_LAYOUT_DOCUMENTSCALEY, DataType = "double" ) ]
		public double LayoutDocumentScaleY
		{
			get {	return GetSettingDouble( STR_LAYOUT_DOCUMENTSCALEY ); }
			set {	SetSetting( STR_LAYOUT_DOCUMENTSCALEY, value ); }
		}

		[ XmlElement( ElementName = STR_STARTUP_SHOWINTRODUCTION, DataType = "string" ) ]
		public string StartupShowIntroduction
		{
			get {	return GetSetting( STR_STARTUP_SHOWINTRODUCTION ); }
			set {	SetSetting( STR_STARTUP_SHOWINTRODUCTION, value ); }
		}
				
		[ XmlArray( ElementName = STR_DATA_BOOKMARKLIST ) ]
		[ XmlArrayItem( ElementName = STR_DATA_BOOKMARK, IsNullable = false, Type = typeof( DataBookmark ) ) ]
		public ListBookmark Bookmark
		{
			get {	return m_lstBookmark; }
			set {	m_lstBookmark = value; }
		}
		
		[ XmlArray( ElementName = STR_DATAFILE ) ]
		[ XmlArrayItem( ElementName = ManagerWorkspace.STR_DATA_FILE, IsNullable = false, Type = typeof( WorkspaceData ) ) ]
		public ListWorkspaceData Data
		{
			get {	return m_lstData; }
			set {	m_lstData = value; }
		}
				
		[ XmlArray( ElementName = STR_DATAMAP ) ]		
		[ XmlArrayItem( ElementName = ManagerWorkspace.STR_DATA_MAP, IsNullable = false, Type = typeof( WorkspaceMap ) ) ]
		public ListWorkspaceMap Map
		{
			get {	return m_lstMap; }
			set {	m_lstMap = value; }
		}

		[ XmlIgnore ]
		public string File
		{
			get {	return m_strFile; }
			set {	m_strFile = value; }
		}

		public string FileName
		{
			get
			{
				FileInfo		fi				= new FileInfo( m_strFile );
				string			str				= string.Format( "{0}", fi.Name );

				return str;
			}
		}

		public string[] GetFileArray()
		{
			int				nCount			= m_lstData.Count;
			string[]		strA			= new string[ nCount ];
						
			FileInfo		fiWorkspace		= new FileInfo( m_strFile );
			Uri				uriWorkspace	= new Uri( fiWorkspace.FullName );

			for( int i = 0; i < nCount; i++ )
			{
				Uri				uriAbs			= new Uri( uriWorkspace, m_lstData[ i ].File );
				FileInfo		fiAbs			= new FileInfo( uriAbs.LocalPath );				

				string			strAbs			= fiAbs.FullName;

				if( UtilityFile.GetFileExist( strAbs ) == true )
				{
					strA[ i ]		= strAbs;
				}
				else
				{
					strA[ i ]		= m_lstData[ i ].File;
				}
			}

			return strA;
		}

		public void DoDataFileAdd( string strFile )
		{
			WorkspaceData	wd				= GetWorkspaceData( strFile );
			
			if( wd != null )
			{
			}
			else
			{
				wd				= new WorkspaceData();
				wd.File			= strFile;

				m_lstData.Add( wd );
			}
			
			m_bEdited						= true;	
		}

		public void DoDataFileRemove( string strFile )
		{
			WorkspaceData	wd				= GetWorkspaceData( strFile );
			m_lstData.Remove( wd );

			m_bEdited						= true;
		}

		private WorkspaceData GetWorkspaceData( string strFile )
		{
			foreach( WorkspaceData wd in m_lstData )
			{
				if( wd.File == strFile )
					return wd;
			}

			return null;
		}

		private void DoFillSetting()
		{
			/*
			public		const string					STR_LAYOUT_FILE					= "Layout.File";
			public		const string					STR_LAYOUT_APPLICAIONSCALEX		= "Layout.Application.ScaleX";
			public		const string					STR_LAYOUT_APPLICAIONSCALEY		= "Layout.Application.ScaleY";
			public		const string					STR_LAYOUT_DOCUMENTSCALEX		= "Layout.Document.ScaleX";
			public		const string					STR_LAYOUT_DOCUMENTSCALEY		= "Layout.Document.ScaleY";
						
			public		const string					STR_STARTUP_SHOWINTRODUCTION	= "Startup.ShowIntroduction";
			*/

			MainWindow		mw				= MainWindow.GetMainWindow();

			SetSetting( STR_LAYOUT_FILE, GetRelativePath( mw.GetFileLayout() ) );
			SetSetting( STR_LAYOUT_APPLICAIONSCALEX, mw.GetLayoutApplicationScaleX() );
			SetSetting( STR_LAYOUT_APPLICAIONSCALEY, mw.GetLayoutApplicationScaleY() );
			SetSetting( STR_LAYOUT_DOCUMENTSCALEX, mw.GetLayoutDocumentScaleX() );
			SetSetting( STR_LAYOUT_DOCUMENTSCALEY, mw.GetLayoutDocumentScaleY() );
			SetSetting( STR_STARTUP_SHOWINTRODUCTION, mw.IsDocumentIntroductionVisible() == true ? "true" : "false" );

			m_lstBookmark.Clear();
			m_lstBookmark.AddRange( ManagerBookmark.GetManager().ListBookmark );
		}

		private string GetRelativePath( string strPath )
		{
			if( string.IsNullOrEmpty( strPath ) )
				return null;

			try
			{
				Uri				uriWorkspace	= new Uri( m_strFile );
				Uri				uriFile			= new Uri( strPath );
				Uri				uriRelative		= uriWorkspace.MakeRelativeUri( uriFile );
				string			strRelative		= uriRelative.ToString();

				strRelative						= HttpUtility.UrlDecode( strRelative );

				return strRelative;
			}
			catch( UriFormatException )
			{
				return strPath;
			}
		}

		private void DoFillFile()
		{
			ManagerData		md				= ManagerData.GetManager();
			
			m_lstData.Clear();

			for( int i = 0; i < md.GetCountDataFile(); i++ )
			{
				DataFile		df				= md.GetDataFile( i );
				string			str				= df.File;				
				string			strRelative		= GetRelativePath( str );
				
				WorkspaceData	wd				= new WorkspaceData();
				wd.File							= strRelative;
				
				m_lstData.Add( wd );
			}
		}

		private void DoFillLane()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();

			m_lstMap.Clear();
			
			foreach( DocMap doc in mw.ListDocument )
			{
				WorkspaceMap	wm				= new WorkspaceMap();
				wm.SequenceId					= doc.SequenceId;
				wm.Position						= doc.PanelActive.Position;
				wm.Zoom							= doc.PanelActive.Zoom;

				for( int i = 0; i < doc.PanelActive.GetCountLane(); i++ )
				{
					PnlMapLane		pnl				= doc.PanelActive.GetLane( i );

					WorkspaceLane	wl				= new WorkspaceLane();						
					wl.Height						= pnl.LaneHeight;					

					for( int j = 0; j < pnl.GetCountDataType(); j++ )
					{
						DataType		dt				= pnl.DoDataTypeGet( j );
						WorkspaceType	wt				= new WorkspaceType( dt.Type, dt.GetColorString() );
						wt.Display						= dt.Display.ToString();
						wt.ScaleMax						= dt.Scale == true ? dt.ScaleMax : double.NaN;
						wt.ScaleMin						= dt.Scale == true ? dt.ScaleMin : double.NaN;						
							
						wl.ListType.Add( wt );
					}

					wm.ListLane.Add( wl );
				}

				m_lstMap.Add( wm );				
			}
		}

		public void DoSave()
		{
			DoSave( m_strFile );
		}

		public void DoSave( string strFile )
		{
			m_strFile						= strFile;

			DoFillSetting();
			DoFillFile();
			DoFillLane();

			XmlSerializer	xs				= new XmlSerializer( typeof( ManagerWorkspace ) );
			TextWriter		tw				= new StreamWriter( strFile );
			
			xs.Serialize( tw, this );
			tw.Close();

			m_bEdited						= false;
		}	

		public void DoClose()
		{
			m_strFile		= null;
			m_dicSetting.Clear();
		}

		public void SetSetting( string strSetting, string strValue )
		{
			m_dicSetting[ strSetting ]		= strValue;
	
			m_bEdited						= true;
		}

		public void SetSetting( string strSetting, double dValue )
		{
			m_dicSetting[ strSetting ]		= dValue.ToString();			

			m_bEdited						= true;
		}

		public string GetSetting( string strSetting )
		{
			if( m_dicSetting.Keys.Contains( strSetting ) == false )
			{
				return null;
			}
			else
			{
				string			strValue		= m_dicSetting[ strSetting ];

				return strValue;
			}
		}

		public double GetSettingDouble( string strSetting )
		{
			if( m_dicSetting.Keys.Contains( strSetting ) == false )
			{
				return double.NaN;
			}
			else
			{
				string			strValue		= m_dicSetting[ strSetting ];
				double			dValue			= double.Parse( strValue );

				return dValue;
			}
		}

		[ XmlIgnore ]
		public string this[ string strSetting ]
		{
			get {	return GetSetting( strSetting ); }
		}

		[ XmlIgnore ]
		public bool IsEdited
		{
			get {	return m_bEdited; }
			set {	m_bEdited = value; }
		}

		public static ManagerWorkspace MakeFromFile( string strFile )
		{
			XmlSerializer	xs				= new XmlSerializer( typeof( ManagerWorkspace ) );
			TextReader		tr				= new StreamReader( strFile );

			ManagerWorkspace	mw			= xs.Deserialize( tr ) as ManagerWorkspace;
			mw.File							= strFile;
			
			tr.Close();			

			mw.File							= strFile;
			mw.IsEdited						= false;

			return mw;
		}
	}
		
	public class WorkspaceData
	{
		//			.								.								.
		private		string							m_strFile						= null;
				
		public WorkspaceData()
		{
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_FILEPATH, DataType = "string" ) ]
		public string File
		{
			get {	return m_strFile; }
			set {	m_strFile = value; }
		}
	}
		
	public class WorkspaceMap
	{
		//			.								.								.
		private		string							m_strSequenceId					= null;
		private		ListWorkspaceLane				m_lstLane						= null;
		private		int								m_nPosition						= 0;
		private		double							m_dZoom							= 1.0;

		public WorkspaceMap()
		{
			m_lstLane		= new ListWorkspaceLane();
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_MAPSEQUENCEID, DataType = "string" ) ]
		public string SequenceId
		{
			get {	return m_strSequenceId; }
			set {	m_strSequenceId = value; }
		}

		[ XmlAttribute( AttributeName = "Position" ) ]
		public int Position
		{
			get {	return m_nPosition; }
			set {	m_nPosition = value; }
		}

		[ XmlAttribute( AttributeName = "Zoom" ) ]
		public double Zoom
		{
			get {	return m_dZoom; }
			set {	m_dZoom = value; }
		}

		[ XmlArrayItem( ElementName = ManagerWorkspace.STR_DATA_LANE, IsNullable = false, Type = typeof( WorkspaceLane ) ) ]
		[ XmlArray( ElementName = ManagerWorkspace.STR_DATA_MAPLANE ) ]
		public ListWorkspaceLane ListLane
		{
			get {	return m_lstLane; }
			set {	m_lstLane = value; }
		}
	}

	public class WorkspaceLane
	{
		//			.								.								.
		private		double							m_dHeight						= double.NaN;				
		private		ListWorkspaceType				m_lstType						= null;

		public WorkspaceLane()
		{
			m_lstType		= new ListWorkspaceType();
		}
				
		[ XmlArray( ElementName = ManagerWorkspace.STR_DATA_MAPLANELIST ) ]
		[ XmlArrayItem( ElementName = ManagerWorkspace.STR_DATA_TYPE, IsNullable = false, Type = typeof( WorkspaceType ) ) ]
		public ListWorkspaceType ListType
		{
			get {	return m_lstType; }
			set {	m_lstType = value; }
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_TYPEHEIGHT, DataType = "double" ) ]
		public double Height
		{
			get {	return m_dHeight; }
			set {	m_dHeight = value; }
		}			
	}

	public class WorkspaceType
	{
		//			.								.								.
		private		string							m_strType						= null;
		private		double							m_dScaleMax						= double.NaN;
		private		double							m_dScaleMin						= double.NaN;		
		private		string							m_strColor						= null;
		private		EDataTypeDisplay				m_eDisplay						= EDataTypeDisplay.BAR;

		public WorkspaceType()
		{
		}

		public WorkspaceType( string strType, string strColor )
		{
			m_strType		= strType;
			m_strColor		= strColor;
		}

		public EDataTypeDisplay GetDisplay()
		{
			return m_eDisplay;
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_TYPEDISPLAY, DataType = "string" ) ]
		public string Display
		{
			get {	return m_eDisplay.ToString(); }
			set 
			{	
				if( value == "BAR" )			m_eDisplay		= EDataTypeDisplay.BAR;
				else if( value == "POINT" )		m_eDisplay		= EDataTypeDisplay.POINT;		
				else if( value == "LINE" )		m_eDisplay		= EDataTypeDisplay.LINE;
				else if( value == "STACK" )		m_eDisplay		= EDataTypeDisplay.STACK;	
			}
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_TYPESCALEMAX, DataType = "double" ) ]
		public double ScaleMax
		{
			get {	return m_dScaleMax; }
			set {	m_dScaleMax = value; }
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_TYPESCALEMIN, DataType = "double" ) ]
		public double ScaleMin
		{
			get {	return m_dScaleMin; }
			set {	m_dScaleMin = value; }
		}
				
		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_TYPETEXT, DataType = "string" ) ]
		public string Type
		{
			get {	return m_strType; }
			set {	m_strType = value; }
		}

		[ XmlAttribute( AttributeName = ManagerWorkspace.STR_DATA_TYPECOLOR, DataType = "string" ) ]
		public string Color
		{
			get {	return m_strColor; }
			set {	m_strColor = value; }
		}
	}
}



