using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using AvalonDock;
using VugMap.Utility;
using VugMap.Utility.Command;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;
using VugMap.Window;

namespace VugMap
{
	using			ListMap							= List< DocMap >;
	using			ListFeature						= List< DataFeature >;
	using			DicRectFeature					= Dictionary< Rectangle, DataFeature >;
	using			ListString						= List< string >;
	using			ListDataType					= List< DataType >;
	using			ListMapLane						= List< PnlMapLane >;	

	public partial class MainWindow : System.Windows.Window
	{
		private		static	MainWindow				S_SINGLETON						= null;

		public		static RoutedCommand			CMD_FILE_NEWDOCUMENT			= null;
		public		static RoutedCommand			CMD_FILE_OPEN					= null;
		public		static RoutedCommand			CMD_FILE_OPENWORKSPACE			= null;
		public		static RoutedCommand			CMD_FILE_OPENLAYOUT				= null;
		public		static RoutedCommand			CMD_FILE_SAVEWORKSPACE			= null;
		public		static RoutedCommand			CMD_FILE_SAVEWORKSPACEAS		= null;
		public		static RoutedCommand			CMD_FILE_SAVELAYOUTAS			= null;
		public		static RoutedCommand			CMD_FILE_SAVEALL				= null;
		public		static RoutedCommand			CMD_FILE_EXPORTIMAGE			= null;
		public		static RoutedCommand			CMD_FILE_CLOSEALL				= null;
		public		static RoutedCommand			CMD_FILE_EXIT					= null;

		public		static RoutedCommand			CMD_DATA_SEARCH					= null;
		public		static RoutedCommand			CMD_DATA_FEATUREUNITE			= null;
		public		static RoutedCommand			CMD_DATA_FEATUREMERGE			= null;
		public		static RoutedCommand			CMD_DATA_FEATUREMOVE			= null;
		public		static RoutedCommand			CMD_DATA_FEATURECOPY			= null;
		public		static RoutedCommand			CMD_DATA_FEATUREDELETE			= null;
		public		static RoutedCommand			CMD_DATA_FEATUREUNDO			= null;		
		public		static RoutedCommand			CMD_DATA_SELECTBYPOSITION		= null;
		public		static RoutedCommand			CMD_DATA_TRACKSETCOLOR			= null;
		public		static RoutedCommand			CMD_DATA_TRACKSETHEIGHT			= null;
		public		static RoutedCommand			CMD_DATA_TRACKDISPLAYBAR		= null;
		public		static RoutedCommand			CMD_DATA_TRACKDISPLAYPOINT		= null;
		public		static RoutedCommand			CMD_DATA_TRACKDISPLAYLINE		= null;
		public		static RoutedCommand			CMD_DATA_TRACKMOVEUP			= null;
		public		static RoutedCommand			CMD_DATA_TRACKMOVEDOWN			= null;
		public		static RoutedCommand			CMD_DATA_TRACKGROUP				= null;
		public		static RoutedCommand			CMD_DATA_TRACKUNGROUP			= null;
		public		static RoutedCommand			CMD_DATA_TRACKSELECTTOEDIT		= null;
		public		static RoutedCommand			CMD_DATA_TRACKSELECTALLFEATURES	= null;
		public		static RoutedCommand			CMD_DATA_TRACKMANUALSCALE		= null;
		public		static RoutedCommand			CMD_DATA_TRACKCHANGETYPE		= null;
		public		static RoutedCommand			CMD_DATA_TRACKHIDELANE			= null;
		public		static RoutedCommand			CMD_DATA_TRACKCLOSEFILE			= null;
		public		static RoutedCommand			CMD_DATA_TRACKOPAVERAGE			= null;
		public		static RoutedCommand			CMD_DATA_TRACKOPDIFFERENCE		= null;
		public		static RoutedCommand			CMD_DATA_TRACKOPSUMMATION		= null;
		public		static RoutedCommand			CMD_DATA_TRACKOPMERGE			= null;
		public		static RoutedCommand			CMD_DATA_TRACKOPFILTER			= null;
		public		static RoutedCommand			CMD_DATA_INTEGRATIONPORF		= null;
		public		static RoutedCommand			CMD_DATA_INTEGRATIONRTS			= null;
		public		static RoutedCommand			CMD_DATA_INTEGRATIONTU			= null;
		public		static RoutedCommand			CMD_DATA_INTEGRATIONTRN			= null;
		
		public		static RoutedCommand			CMD_VIEW_APPSCALEUP				= null;
		public		static RoutedCommand			CMD_VIEW_APPSCALEDOWN			= null;
		public		static RoutedCommand			CMD_VIEW_SCALEUP				= null;
		public		static RoutedCommand			CMD_VIEW_SCALEDOWN				= null;
		public		static RoutedCommand			CMD_VIEW_ZOOMIN					= null;
		public		static RoutedCommand			CMD_VIEW_ZOOMOUT				= null;
		public		static RoutedCommand			CMD_VIEW_ZOOMTO					= null;
		public		static RoutedCommand			CMD_VIEW_SCROLLLEFT				= null;
		public		static RoutedCommand			CMD_VIEW_SCROLLRIGHT			= null;
		public		static RoutedCommand			CMD_VIEW_POSITIONTO				= null;
		public		static RoutedCommand			CMD_VIEW_SPLIT					= null;
		public		static RoutedCommand			CMD_VIEW_FEATUREOPACITY			= null;
		public		static RoutedCommand			CMD_VIEW_SCROLLLEFT_SMALL		= null;
		public		static RoutedCommand			CMD_VIEW_SCROLLRIGHT_SMALL		= null;
		public		static RoutedCommand			CMD_VIEW_GOHOME					= null;
		public		static RoutedCommand			CMD_VIEW_GOEND					= null;
		public		static RoutedCommand			CMD_VIEW_TABNEXT				= null;
		public		static RoutedCommand			CMD_VIEW_TABPREV				= null;
		public		static RoutedCommand			CMD_VIEW_REFRESH				= null;

		public		static RoutedCommand			CMD_FEATURE_MOVELEFT			= null;
		public		static RoutedCommand			CMD_FEATURE_MOVERIGHT			= null;
		public		static RoutedCommand			CMD_FEATURE_SHRINKSTART			= null;
		public		static RoutedCommand			CMD_FEATURE_EXPANDEND				= null;

		public		static RoutedCommand			CMD_WINDOW_WINDOWINTRODUCTION	= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWFILEEXPLORER	= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWEDIT			= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWBOOKMARK		= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWSETTING		= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWFEATURE		= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWFEATURESELECTED= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWLOG			= null;
		public		static RoutedCommand			CMD_WINDOW_WINDOWSEARCH			= null;

		public		static RoutedCommand			CMD_WINDOW_LAYOUTSAVE			= null;
		public		static RoutedCommand			CMD_WINDOW_LAYOUTRESTORE		= null;

		public		static RoutedCommand			CMD_HELP_TEST					= null;
		public		static RoutedCommand			CMD_HELP_ABOUT					= null;
		public		static RoutedCommand			CMD_HELP_DOCUMENTATION			= null;
		public		static RoutedCommand			CMD_HELP_SHORTCUTS				= null;
		public		static RoutedCommand			CMD_HELP_AUTOSAVE				= null;
		public		static RoutedCommand			CMD_HELP_UPDATE					= null;
		public		static RoutedCommand			CMD_HELP_GARBAGECOLLECTION		= null;

		public		static RoutedCommand			CMD_VUG_TEST1					= null;
		public		static RoutedCommand			CMD_VUG_TEST2					= null;
		public		static RoutedCommand			CMD_VUG_TEST4					= null;
		public		static RoutedCommand			CMD_VUG_TEST5					= null;
					
		static MainWindow()
		{
			CMD_FILE_NEWDOCUMENT			= new RoutedCommand();
			CMD_FILE_NEWDOCUMENT.InputGestures.Add( new KeyGesture( Key.N, ModifierKeys.Control ) );

			CMD_FILE_OPEN					= new RoutedCommand();
			CMD_FILE_OPEN.InputGestures.Add( new KeyGesture( Key.O, ModifierKeys.Control ) );

			CMD_FILE_OPENWORKSPACE			= new RoutedCommand();
			CMD_FILE_OPENWORKSPACE.InputGestures.Add( new KeyGesture( Key.O, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_FILE_OPENLAYOUT				= new RoutedCommand();			

			CMD_FILE_SAVEWORKSPACE			= new RoutedCommand();
			CMD_FILE_SAVEWORKSPACE.InputGestures.Add( new KeyGesture( Key.S, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_FILE_SAVEWORKSPACEAS		= new RoutedCommand();

			CMD_FILE_SAVELAYOUTAS			= new RoutedCommand();

			CMD_FILE_SAVEALL				= new RoutedCommand();
			CMD_FILE_SAVEALL.InputGestures.Add( new KeyGesture( Key.S, ModifierKeys.Control ) );

			CMD_FILE_EXPORTIMAGE			= new RoutedCommand();
			CMD_FILE_EXPORTIMAGE.InputGestures.Add( new KeyGesture( Key.E, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_FILE_EXIT					= new RoutedCommand();
			CMD_FILE_EXIT.InputGestures.Add( new KeyGesture( Key.X, ModifierKeys.Control ) );

			CMD_DATA_FEATUREUNITE			= new RoutedCommand();
			CMD_DATA_FEATUREUNITE.InputGestures.Add( new KeyGesture( Key.U, ModifierKeys.Control ) );

			CMD_DATA_FEATUREMERGE			= new RoutedCommand();
			CMD_DATA_FEATUREMERGE.InputGestures.Add( new KeyGesture( Key.M, ModifierKeys.Control ) );

			CMD_DATA_FEATUREMOVE			= new RoutedCommand();
			CMD_DATA_FEATUREMOVE.InputGestures.Add( new KeyGesture( Key.V, ModifierKeys.Control ) );

			CMD_DATA_FEATURECOPY			= new RoutedCommand();
			CMD_DATA_FEATURECOPY.InputGestures.Add( new KeyGesture( Key.C, ModifierKeys.Control ) );

			CMD_DATA_FEATUREDELETE			= new RoutedCommand();
			CMD_DATA_FEATUREDELETE.InputGestures.Add( new KeyGesture( Key.D, ModifierKeys.Control ) );

			CMD_DATA_FEATUREUNDO			= new RoutedCommand();
			CMD_DATA_FEATUREUNDO.InputGestures.Add( new KeyGesture( Key.Z, ModifierKeys.Control ) );

			CMD_DATA_SEARCH					= new RoutedCommand();
			CMD_DATA_SEARCH.InputGestures.Add( new KeyGesture( Key.F, ModifierKeys.Control ) );

			CMD_DATA_SELECTBYPOSITION		= new RoutedCommand();
			
			CMD_DATA_TRACKSETCOLOR			= new RoutedCommand();
			CMD_DATA_TRACKSETCOLOR.InputGestures.Add( new KeyGesture( Key.C, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKSETHEIGHT			= new RoutedCommand();
			CMD_DATA_TRACKSETHEIGHT.InputGestures.Add( new KeyGesture( Key.H, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKDISPLAYBAR		= new RoutedCommand();
			CMD_DATA_TRACKDISPLAYBAR.InputGestures.Add( new KeyGesture( Key.B, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKDISPLAYPOINT		= new RoutedCommand();
			CMD_DATA_TRACKDISPLAYPOINT.InputGestures.Add( new KeyGesture( Key.P, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKDISPLAYLINE		= new RoutedCommand();
			CMD_DATA_TRACKDISPLAYLINE.InputGestures.Add( new KeyGesture( Key.L, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKMOVEUP			= new RoutedCommand();
			CMD_DATA_TRACKMOVEUP.InputGestures.Add( new KeyGesture( Key.Up, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKMOVEDOWN			= new RoutedCommand();
			CMD_DATA_TRACKMOVEDOWN.InputGestures.Add( new KeyGesture( Key.Down, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKGROUP				= new RoutedCommand();
			CMD_DATA_TRACKGROUP.InputGestures.Add( new KeyGesture( Key.G, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKUNGROUP			= new RoutedCommand();
			CMD_DATA_TRACKUNGROUP.InputGestures.Add( new KeyGesture( Key.U, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKSELECTTOEDIT		= new RoutedCommand();
			CMD_DATA_TRACKSELECTTOEDIT.InputGestures.Add( new KeyGesture( Key.E, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKSELECTALLFEATURES	= new RoutedCommand();
			CMD_DATA_TRACKSELECTALLFEATURES.InputGestures.Add( new KeyGesture( Key.A, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKMANUALSCALE		= new RoutedCommand();
			CMD_DATA_TRACKMANUALSCALE.InputGestures.Add( new KeyGesture( Key.C, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKCHANGETYPE		= new RoutedCommand();
			CMD_DATA_TRACKCHANGETYPE.InputGestures.Add( new KeyGesture( Key.T, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKHIDELANE			= new RoutedCommand();
			CMD_DATA_TRACKHIDELANE.InputGestures.Add( new KeyGesture( Key.D, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKCLOSEFILE			= new RoutedCommand();
			CMD_DATA_TRACKCLOSEFILE.InputGestures.Add( new KeyGesture( Key.X, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_DATA_TRACKOPAVERAGE			= new RoutedCommand();

			CMD_DATA_TRACKOPDIFFERENCE		= new RoutedCommand();

			CMD_DATA_TRACKOPSUMMATION		= new RoutedCommand();

			CMD_DATA_TRACKOPMERGE			= new RoutedCommand();

			CMD_DATA_TRACKOPFILTER			= new RoutedCommand();

			CMD_DATA_INTEGRATIONPORF		= new RoutedCommand();
			CMD_DATA_INTEGRATIONRTS			= new RoutedCommand();
			CMD_DATA_INTEGRATIONTU			= new RoutedCommand();
			CMD_DATA_INTEGRATIONTRN			= new RoutedCommand();

			CMD_FILE_CLOSEALL				= new RoutedCommand();
			CMD_FILE_CLOSEALL.InputGestures.Add( new KeyGesture( Key.X, ModifierKeys.Control | ModifierKeys.Shift ) );

			/*
			CMD_VIEW_APPSCALEUP				= new RoutedCommand();
			CMD_VIEW_APPSCALEUP.InputGestures.Add( new KeyGesture( Key.OemPlus, ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt ) );
			
			CMD_VIEW_APPSCALEDOWN			= new RoutedCommand();
			CMD_VIEW_APPSCALEDOWN.InputGestures.Add( new KeyGesture( Key.OemMinus, ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt ) );
			 */

			CMD_VIEW_SCALEUP				= new RoutedCommand();
			CMD_VIEW_SCALEUP.InputGestures.Add( new KeyGesture( Key.OemPlus, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift++" ) );
			
			CMD_VIEW_SCALEDOWN				= new RoutedCommand();
			CMD_VIEW_SCALEDOWN.InputGestures.Add( new KeyGesture( Key.OemMinus, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+-" ) );

			CMD_VIEW_ZOOMIN					= new RoutedCommand();
			CMD_VIEW_ZOOMIN.InputGestures.Add( new KeyGesture( Key.OemPlus, ModifierKeys.Control, "Ctrl++" ) );
			
			CMD_VIEW_ZOOMOUT				= new RoutedCommand();
			CMD_VIEW_ZOOMOUT.InputGestures.Add( new KeyGesture( Key.OemMinus, ModifierKeys.Control, "Ctrl+-" ) );

			CMD_VIEW_ZOOMTO					= new RoutedCommand();
			CMD_VIEW_ZOOMTO.InputGestures.Add( new KeyGesture( Key.D0, ModifierKeys.Control ) );

			CMD_VIEW_SCROLLLEFT				= new RoutedCommand();
			CMD_VIEW_SCROLLLEFT.InputGestures.Add( new KeyGesture( Key.Left, ModifierKeys.Shift ) );

			CMD_VIEW_SCROLLRIGHT			= new RoutedCommand();
			CMD_VIEW_SCROLLRIGHT.InputGestures.Add( new KeyGesture( Key.Right, ModifierKeys.Shift ) );

			CMD_VIEW_POSITIONTO				= new RoutedCommand();
			CMD_VIEW_POSITIONTO.InputGestures.Add( new KeyGesture( Key.G, ModifierKeys.Control ) );

			CMD_VIEW_SPLIT					= new RoutedCommand();
			CMD_VIEW_SPLIT.InputGestures.Add( new KeyGesture( Key.T, ModifierKeys.Control ) );

			CMD_VIEW_FEATUREOPACITY			= new RoutedCommand();

			CMD_VIEW_SCROLLLEFT_SMALL		= new RoutedCommand();
			CMD_VIEW_SCROLLLEFT_SMALL.InputGestures.Add( new KeyGesture( Key.Left, ModifierKeys.None ) );

			CMD_VIEW_SCROLLRIGHT_SMALL		= new RoutedCommand();
			CMD_VIEW_SCROLLRIGHT_SMALL.InputGestures.Add( new KeyGesture( Key.Right, ModifierKeys.None ) );

			CMD_VIEW_GOHOME					= new RoutedCommand();
			CMD_VIEW_GOHOME.InputGestures.Add( new KeyGesture( Key.Home, ModifierKeys.None ) );

			CMD_VIEW_GOEND					= new RoutedCommand();
			CMD_VIEW_GOEND.InputGestures.Add( new KeyGesture( Key.End, ModifierKeys.None ) );

			CMD_VIEW_TABNEXT				= new RoutedCommand();
			CMD_VIEW_TABNEXT.InputGestures.Add( new KeyGesture( Key.Tab, ModifierKeys.Control ) );

			CMD_VIEW_TABPREV				= new RoutedCommand();
			CMD_VIEW_TABPREV.InputGestures.Add( new KeyGesture( Key.Tab, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_VIEW_REFRESH				= new RoutedCommand();
			CMD_VIEW_REFRESH.InputGestures.Add( new KeyGesture( Key.F5, ModifierKeys.None ) );

			CMD_FEATURE_MOVELEFT			= new RoutedCommand();
			CMD_FEATURE_MOVELEFT.InputGestures.Add( new KeyGesture( Key.NumPad1, ModifierKeys.None ) );
			CMD_FEATURE_MOVELEFT.InputGestures.Add( new KeyGesture( Key.Left, ModifierKeys.Alt ) );

			CMD_FEATURE_MOVERIGHT			= new RoutedCommand();
			CMD_FEATURE_MOVERIGHT.InputGestures.Add( new KeyGesture( Key.NumPad2, ModifierKeys.None ) );
			CMD_FEATURE_MOVERIGHT.InputGestures.Add( new KeyGesture( Key.Right, ModifierKeys.Alt ) );

			CMD_FEATURE_SHRINKSTART			= new RoutedCommand();
			CMD_FEATURE_SHRINKSTART.InputGestures.Add( new KeyGesture( Key.NumPad4, ModifierKeys.None ) );
			CMD_FEATURE_SHRINKSTART.InputGestures.Add( new KeyGesture( Key.Down, ModifierKeys.Alt ) );

			CMD_FEATURE_EXPANDEND				= new RoutedCommand();
			CMD_FEATURE_EXPANDEND.InputGestures.Add( new KeyGesture( Key.NumPad5, ModifierKeys.None ) );
			CMD_FEATURE_EXPANDEND.InputGestures.Add( new KeyGesture( Key.Up, ModifierKeys.Alt ) );

			CMD_WINDOW_WINDOWINTRODUCTION	= new RoutedCommand();			
			CMD_WINDOW_WINDOWFILEEXPLORER	= new RoutedCommand();			
			CMD_WINDOW_WINDOWEDIT			= new RoutedCommand();			
			CMD_WINDOW_WINDOWBOOKMARK		= new RoutedCommand();			
			CMD_WINDOW_WINDOWSETTING		= new RoutedCommand();			
			CMD_WINDOW_WINDOWFEATURE		= new RoutedCommand();
			CMD_WINDOW_WINDOWFEATURESELECTED= new RoutedCommand();
			CMD_WINDOW_WINDOWLOG			= new RoutedCommand();
			CMD_WINDOW_WINDOWSEARCH			= new RoutedCommand();

			CMD_WINDOW_LAYOUTSAVE			= new RoutedCommand();
			CMD_WINDOW_LAYOUTSAVE.InputGestures.Add( new KeyGesture( Key.S, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_WINDOW_LAYOUTRESTORE		= new RoutedCommand();
			CMD_WINDOW_LAYOUTRESTORE.InputGestures.Add( new KeyGesture( Key.S, ModifierKeys.Control | ModifierKeys.Shift ) );

			CMD_HELP_TEST					= new RoutedCommand();
			
			CMD_HELP_ABOUT					= new RoutedCommand();
			
			CMD_HELP_DOCUMENTATION			= new RoutedCommand();
			CMD_HELP_DOCUMENTATION.InputGestures.Add( new KeyGesture( Key.H, ModifierKeys.Control ) );

			CMD_HELP_SHORTCUTS				= new RoutedCommand();

			CMD_HELP_AUTOSAVE				= new RoutedCommand();
			
			CMD_HELP_UPDATE					= new RoutedCommand();			
			CMD_HELP_UPDATE.InputGestures.Add( new KeyGesture( Key.U, ModifierKeys.Control ) );

			CMD_HELP_GARBAGECOLLECTION		= new RoutedCommand();
		}

		public static MainWindow GetMainWindow()
		{
			return S_SINGLETON;
		}		

		private		ManagerWorkspace				m_mgrWorkspace					= null;

		private		ListMap							m_lstMap						= null;
		private		PropertyFeature					m_pptFeature					= null;
		private		PropertyFeatureSelected			m_pptSelected					= null;
		private		string							m_strFileLayout					= null;
		private		double							m_dDocumentScaleX				= 1.0f;
		private		double							m_dDocumentScaleY				= 1.0f;

		private		ContextMenu						m_cmExplorerFile				= null;
		private		bool							m_bSelectByPosition				= true;
		private		DispatcherTimer					m_tmrAutoSave					= null;
		private		DispatcherTimer					m_tmrWorkspaceSave				= null;
		private		bool							m_bAutoSave						= false;
		private		List< MenuItem >				m_lstMruMenuItem				= new List< MenuItem >();

		public MainWindow()
		{
			if( S_SINGLETON == null )
			{
				S_SINGLETON		= this;
			}
						
			m_lstMap		= new ListMap();

			InitializeComponent();
						
			InitializePropertyVugmap();
			InitializePropertyFeature();
			InitializePropertyFeatureSelected();
			
			BuildElementMenu();
			DoMruMenuUpdate();

			m_tmrAutoSave					= new DispatcherTimer();
			m_tmrAutoSave.Interval			= TimeSpan.FromSeconds( 5 );
			m_tmrAutoSave.Tick				+= OnAutoSaveTick;

			m_tmrWorkspaceSave				= new DispatcherTimer();
			m_tmrWorkspaceSave.Interval		= TimeSpan.FromSeconds( 5 );
			m_tmrWorkspaceSave.Tick			+= OnWorkspaceSaveTick;

			if( AppSetting.AutoSave )
			{
				IsAutoSave					= true;
				m_mniAutoSave.IsChecked		= true;
			}

			m_dckmVugmap.PropertyChanged	+= OnDockingManagerPropertyChanged;
		}

		public ListMap ListDocument
		{
			get {	return m_lstMap; }
		}

		private void BuildElementMenu()
		{
			ManagerData		md				= ManagerData.GetManager();

			m_cmExplorerFile				= new ContextMenu();
			
			MenuItem		miSave			= new MenuItem();
			miSave.Header					= "Save";
			miSave.Click					+= delegate( object obj, RoutedEventArgs ea )
			{				
				TreeViewItem	tvi				= m_trvFile.SelectedItem as TreeViewItem;
				string			strFile			= tvi.Header as string;
				if( strFile.EndsWith( " (*)" ) == true )
					strFile			= strFile.Substring( 0, strFile.Length - 4 );
				if( strFile.Contains( ":" ) == true )
					strFile			= ( tvi.Parent as TreeViewItem ).Header as string;

				bool			b				= md.DoFileSave( strFile );

				if( b == true )
				{
					FileInfo		fi				= new FileInfo( strFile );				
					UtilityMessage.ShowMessageFileSave( fi.Name );

					DoExplorerUpdate();
				}
			};

			MenuItem		miSaveAs		= new MenuItem();
			miSaveAs.Header					= "Save As";
			miSaveAs.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				TreeViewItem	tvi				= m_trvFile.SelectedItem as TreeViewItem;
				string			strFile			= tvi.Header as string;
				if( strFile.EndsWith( " (*)" ) == true )
					strFile			= strFile.Substring( 0, strFile.Length - 4 );
				if( strFile.Contains( ":" ) == true )
					strFile			= ( tvi.Parent as TreeViewItem ).Header as string;

				bool			b				= md.DoFileSaveAs( strFile );

				if( b == true )
				{
					FileInfo		fi				= new FileInfo( strFile );				
					UtilityMessage.ShowMessageFileSave( fi.Name );

					DoExplorerUpdate();
				}
			};

			MenuItem		miClose			= new MenuItem();
			miClose.Header					= "Close";
			miClose.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				TreeViewItem	tvi				= m_trvFile.SelectedItem as TreeViewItem;
				string			strFile			= tvi.Header as string;
				if( strFile.EndsWith( " (*)" ) == true )
					strFile			= strFile.Substring( 0, strFile.Length - 4 );
				if( strFile.Contains( ":" ) == true )
					strFile			= ( tvi.Parent as TreeViewItem ).Header as string;

				DataFile		df				= md.DoFileClose( strFile );
				ListMap			lst				= new ListMap();
				lst.AddRange( ListDocument );

				foreach( DocMap doc in lst )
				{
					doc.DoLaneRemove( df );

					if( m_dckmVugmap.MainDocumentPane.Items.Contains( doc ) == true )
					{
						doc.DoUpdateView();		
					}
				}				

				DoExplorerUpdate();
			};
								
			m_cmExplorerFile.Items.Add( miSave );
			m_cmExplorerFile.Items.Add( miSaveAs );
			m_cmExplorerFile.Items.Add( new Separator() );
			m_cmExplorerFile.Items.Add( miClose );			
		}

		private void OnCommandFileNewDocument( object obj, ExecutedRoutedEventArgs ea )
		{
			DoDocumentNew();
		}

		private void OnCommandDataFeatureUnite( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataFeatureUnite();
		}

		private void DoCommandDataFeatureUnite()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else
			{				
				doc.PanelActive.DoLaneFeatureUniteSelected();
			}			
		}

		private void OnCommandDataFeatureUniteCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			ListMapLane		lst				= pm.ListLaneEditable;
			
			foreach( PnlMapLane pnl in lst )
			{
				if( pnl.GetCountFeatureSelected() > 0 )
				{
					ea.CanExecute	= true;
					return;
				}
			}

			ea.CanExecute	= false;
			return;
		}

		private void OnCommandDataFeatureMerge( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataFeatureMerge();
		}

		private void DoCommandDataFeatureMerge()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else
			{				
				doc.PanelActive.ListLaneEditable[ 0 ].DoHeadFeatOpMergeClick();
			}			
		}

		private void OnCommandDataFeatureMergeCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			ListMapLane		lst				= pm.ListLaneEditable;
			
			foreach( PnlMapLane pnl in lst )
			{
				if( pnl.GetCountFeatureSelected() > 0 )
				{
					ea.CanExecute	= true;
					return;
				}
			}

			ea.CanExecute	= false;
			return;
		}

		private void OnCommandDataFeatureMove( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataFeatureMove();
		}

		private void DoCommandDataFeatureMove()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else
			{				
				doc.PanelActive.ListLaneEditable[ 0 ].DoHeadFeatOpMoveClick();
			}			
		}

		private void OnCommandDataFeatureMoveCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			ListMapLane		lst				= pm.ListLaneEditable;
			if( pm.ListLaneEditable.Count != 1 )
			{
				ea.CanExecute	= false;
				return;
			}
			
			if( pm.ListLaneEditable[ 0 ].GetCountFeatureSelected() > 0 )
			{
				ea.CanExecute	= true;
				return;
			}			

			ea.CanExecute	= false;
			return;
		}

		private void OnCommandDataFeatureCopy( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataFeatureCopy();
		}

		private void DoCommandDataFeatureCopy()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else
			{				
				doc.PanelActive.ListLaneEditable[ 0 ].DoHeadFeatOpCopyClick();
			}			
		}

		private void OnCommandDataFeatureCopyCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			ListMapLane		lst				= pm.ListLaneEditable;
			if( pm.ListLaneEditable.Count != 1 )
			{
				ea.CanExecute	= false;
				return;
			}
			
			if( pm.ListLaneEditable[ 0 ].GetCountFeatureSelected() > 0 )
			{
				ea.CanExecute	= true;
				return;
			}			

			ea.CanExecute	= false;
			return;
		}

		private void OnCommandDataFeatureDelete( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataFeatureDelete();
		}

		private void DoCommandDataFeatureDelete()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;			

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else
			{
				doc.PanelActive.DoLaneFeatureDeleteSelected();
			}			
		}

		private void OnCommandDataFeatureDeleteCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			ListMapLane		lst				= pm.ListLaneEditable;
			
			foreach( PnlMapLane pnl in lst )
			{
				if( pnl.GetCountFeatureSelected() > 0 )
				{
					ea.CanExecute	= true;
					return;
				}
			}

			ea.CanExecute	= false;
			return;
		}

		private void OnCommandDataFeatureUndo( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataFeatureUndo();
		}

		private void DoCommandDataFeatureUndo()
		{
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandBase		cb				= me.RemoveCommandLast();

			Cursor			cur				= Cursor;
			Cursor							= Cursors.Wait;

			cb.DoUndo();

			Cursor							= cur;

			cb.DoLaneUpdate();
			DoEditUpdate();

			DoAutoSaveImmediate();
		}

		private void OnCommandDataFeatureUndoCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagerEdit			me			= ManagerEdit.GetManager();
			
			if( me.GetCount() == 0 )
			{
				ea.CanExecute	= false;
			}
			else
			{
				ea.CanExecute	= true;
			}
		}
		
		private void OnCommandDataSearch( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataSearch();
		}

		private void DoCommandDataSearch()
		{
			DialogSearch	dlg				= new DialogSearch();										
			dlg.Owner						= MainWindow.GetMainWindow();
									
			Nullable<bool>	b				= dlg.ShowDialog();			
		}
		
		private void OnCommandDataSearchCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagerData			md			= ManagerData.GetManager();
			
			if( md.GetCountDataFile() == 0 )
			{
				ea.CanExecute	= false;
			}
			else
			{
				ea.CanExecute	= true;
			}
		}

		public bool IsSelectByPosition
		{
			get {	return m_bSelectByPosition; }
		}

		private void OnCommandDataSelectByPosition(  object obj, ExecutedRoutedEventArgs ea )
		{
			m_bSelectByPosition					= !m_bSelectByPosition;
		}

		private void OnCommandDataSelectByPositionCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_bSelectByPosition == true )
			{				
				m_mniDataFeatureSelectByPosition.IsChecked
												= true;
			}
			else
			{
				m_mniDataFeatureSelectByPosition.IsChecked
												= false;
			}

			ea.CanExecute	= true;
		}

		private void OnCommandDataTrackSetColor(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackSetColor();
		}

		private void DoCommandDataTrackSetColor()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadSetColorClick();
		}

		private void OnCommandDataTrackSetColorCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}
						
			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackSetHeight(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackSetHeight();
		}

		private void DoCommandDataTrackSetHeight()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadSetHeightClick();
		}

		private void OnCommandDataTrackSetHeightCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackDisplayBar(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackDisplayBar();
		}

		private void DoCommandDataTrackDisplayBar()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadDisplayBox();
		}

		private void OnCommandDataTrackDisplayBarCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackDisplayPoint(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackDisplayPoint();
		}

		private void DoCommandDataTrackDisplayPoint()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadDisplayPoint();
		}

		private void OnCommandDataTrackDisplayBarPointCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackDisplayLine(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackDisplayLine();
		}

		private void DoCommandDataTrackDisplayLine()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadDisplayLine();
		}

		private void OnCommandDataTrackDisplayBarLineCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackMoveUp(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackMoveUp();
		}

		private void DoCommandDataTrackMoveUp()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.DoLaneMoveUp( dm.PanelActive.LaneSelected[ 0 ] );
		}

		private void OnCommandDataTrackMoveUpCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count == 1 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackMoveDown(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackMoveDown();
		}

		private void DoCommandDataTrackMoveDown()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.DoLaneMoveDown( dm.PanelActive.LaneSelected[ 0 ] );
		}

		private void OnCommandDataTrackMoveDownCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count == 1 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackGroup(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackGroup();
		}

		private void DoCommandDataTrackGroup()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.DoLaneGroup();
		}

		private void OnCommandDataTrackGroupCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 1 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackUngroup(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackUngroup();
		}

		private void DoCommandDataTrackUngroup()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.DoLaneUngroup();
		}

		private void OnCommandDataTrackUngroupCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count == 1 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackSelectToEdit(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackSelectToEdit();
		}

		private void DoCommandDataTrackSelectToEdit()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadSelectToEditClick();
		}

		private void OnCommandDataTrackSelectToEditCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackSelectAllFeatures(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackSelectAllFeatures();
		}

		private void DoCommandDataTrackSelectAllFeatures()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadSelectAllClick();
		}

		private void OnCommandDataTrackSelectAllFeaturesCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count == 1 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackManualScale(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackManualScale();
		}

		private void DoCommandDataTrackManualScale()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadManualScaleClick();
		}

		private void OnCommandDataTrackManualScaleCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackChangeType(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackChangeType();
		}

		private void DoCommandDataTrackChangeType()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadChangeTypeClick();
		}

		private void OnCommandDataTrackChangeTypeCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count == 1 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackHideLane(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackHideLane();
		}

		private void DoCommandDataTrackHideLane()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadHideClick();
		}

		private void OnCommandDataTrackHideLaneCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackCloseFile(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackCloseFile();
		}

		private void DoCommandDataTrackCloseFile()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadCloseClick();
		}

		private void OnCommandDataTrackCloseFileCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackOpAverage(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackOpAverage();
		}

		private void DoCommandDataTrackOpAverage()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadOpeartionAverageClick();
		}

		private void OnCommandDataTrackOpAverageCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackOpDifference(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackOpDifference();
		}

		private void DoCommandDataTrackOpDifference()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadOpeartionDiffClick();
		}

		private void OnCommandDataTrackOpDifferenceCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackOpSummation(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackOpSummation();
		}

		private void DoCommandDataTrackOpSummation()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadOpeartionSumClick();
		}

		private void OnCommandDataTrackOpSummationCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackOpMerge(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackOpMerge();
		}

		private void DoCommandDataTrackOpMerge()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadOpeartionMergeClick();
		}

		private void OnCommandDataTrackOpMergeCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataTrackOpFilter(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataTrackOpFilter();
		}

		private void DoCommandDataTrackOpFilter()
		{
			DocMap			dm				= m_dckmVugmap.ActiveDocument as DocMap;

			dm.PanelActive.LaneSelected[ 0 ].DoHeadOpeartionFilterClick();
		}

		private void OnCommandDataTrackOpFilterCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagedContent	mc				= m_dckmVugmap.ActiveDocument;
			if( mc is DocMap == false )
			{
				ea.CanExecute	= false;
				return;
			}

			DocMap			dm				= mc as DocMap;
			PnlMap			pm				= dm.PanelActive;

			if( pm == null )
			{
				ea.CanExecute	= false;
				return;
			}

			if( pm.LaneSelected.Count > 0 )
			{
				ea.CanExecute	= true;
				return;
			}
			else
			{
				ea.CanExecute	= false;
				return;
			}	
		}

		private void OnCommandDataIntegrationPorf(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataIntegrationPorf();
		}

		private void DoCommandDataIntegrationPorf()
		{
			DocMap			dm				= DoDocumentActive();

			dm.PanelActive.LaneSelected[ 0 ].DoHeadIntegrationPorfClick();
		}

		private void OnCommandDataIntegrationRts(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataIntegrationRts();
		}

		private void DoCommandDataIntegrationRts()
		{
			DocMap			dm				= DoDocumentActive();

			dm.PanelActive.LaneSelected[ 0 ].DoHeadIntegrationRtsClick();
		}

		private void OnCommandDataIntegrationTu(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataIntegrationTu();
		}

		private void DoCommandDataIntegrationTu()
		{
			DocMap			dm				= DoDocumentActive();

			dm.PanelActive.LaneSelected[ 0 ].DoHeadIntegrationTuClick();
		}

		private void OnCommandDataIntegrationTrn(  object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandDataIntegrationTrn();
		}

		private void DoCommandDataIntegrationTrn()
		{
			DocMap			dm				= DoDocumentActive();

			dm.PanelActive.LaneSelected[ 0 ].DoHeadIntegrationTrnClick();
		}

		private void OnCommandWindowWindowIntroduction( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.MainDocumentPane.Items.Add( m_docIntroduction );
			m_docIntroduction.SetAsActive();
		}

		private void OnCommandWindowWindowIntroductionCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.MainDocumentPane.Items.Contains( m_docIntroduction ) == true )
			{
				ea.CanExecute	= false;
			}
			else
			{
				ea.CanExecute	= true;
			}
		}

		private void OnCommandWindowWindowFileExplorer( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntExplorer, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowFileExplorerCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntExplorer.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowSetting( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntSetting, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowSettingCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntSetting.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowEdit( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntEdit, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowEditCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntEdit.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowBookmark( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntBookmark, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowBookmarkCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntBookmark.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowFeature( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntFeature, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowFeatureCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntFeature.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowFeatureSelected( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntFeatureSelected, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowFeatureSelectedCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntFeatureSelected.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowLog( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntLog, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowLogCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntLog.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandWindowWindowSearch( object obj, ExecutedRoutedEventArgs ea )
		{
			m_dckmVugmap.Show( m_dcntSearch, DockableContentState.FloatingWindow );			
		}

		private void OnCommandWindowWindowSearchCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dcntSearch.State == DockableContentState.Hidden )
			{
				ea.CanExecute	= true;
			}
			else
			{
				ea.CanExecute	= false;
			}
		}

		private void OnCommandLayoutSave( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandLayoutSave();
		}

		private void DoCommandLayoutSave()
		{
			DoLayoutSave();
		}

		private void OnCommandLayoutRestore( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandLayoutRestore();
		}

		private void DoCommandLayoutRestore()
		{
			DoLayoutRestore();
		}

		private void OnCommandFileOpen( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileOpen();
		}

		private void DoCommandFileOpen()
		{
			Microsoft.Win32.OpenFileDialog
							dlg				= new Microsoft.Win32.OpenFileDialog();

			dlg.Title						= "Open Data Files";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;
			dlg.Multiselect					= true;
			dlg.DefaultExt					= ".gff,.workspace";
			dlg.Filter						= "GFF file (.gff)|*.gff|Workspace file (.workspace)|*.workspace";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string[]		strFileA		= dlg.FileNames;

				DoDrop( strFileA );
			}
		}

		private void OnCommadFileOpenCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute	= true;			
		}

		private void OnCommandFileOpenWorkspace( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileOpenWorkspace();
		}

		private void DoCommandFileOpenWorkspace()
		{
			Microsoft.Win32.OpenFileDialog
							dlg				= new Microsoft.Win32.OpenFileDialog();

			dlg.Title						= "Open Worksapce File";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;
			dlg.Multiselect					= true;
			dlg.DefaultExt					= ".workspace";
			dlg.Filter						= "Workspace file (.workspace)|*.workspace";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string[]		strFileA		= dlg.FileNames;

				DoDrop( strFileA );
			}
		}

		private void OnCommandFileOpenLayout( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileOpenLayout();
		}

		private void DoCommandFileOpenLayout()
		{
			Microsoft.Win32.OpenFileDialog
							dlg				= new Microsoft.Win32.OpenFileDialog();

			dlg.Title						= "Open a Layout File";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;
			dlg.Multiselect					= false;
			dlg.DefaultExt					= ".xml";
			dlg.Filter						= "Layout XML files (.xml)|*.xml";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strFile			= dlg.FileName;				

				DoLayoutRestore( strFile );
			}
		}

		private void OnCommandFileSaveWorkspace( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileSaveWorkspace();
		}

		private void DoCommandFileSaveWorkspace()
		{
			if( System.IO.File.Exists( m_mgrWorkspace.File ) )
			{
				DoWorkspaceSave();
			}
			else
			{
				DoCommandFileSaveWorkspaceAs();
			}
		}

		private void OnCommandFileSaveWorkspaceAs( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileSaveWorkspaceAs();
		}

		private void DoCommandFileSaveWorkspaceAs()
		{
			Microsoft.Win32.SaveFileDialog
							dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Save a Workspace File";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;
			dlg.DefaultExt					= ".workspace";
			dlg.Filter						= "Workspace file (.workspace)|*.workspace";

			string			strCurrentName	= m_mgrWorkspace.File != null
												? System.IO.Path.GetFileNameWithoutExtension( m_mgrWorkspace.File ) : null;

			if( strCurrentName != null && strCurrentName.StartsWith( Constant.S_TEMP_PREFIX ) )
				dlg.FileName				= strCurrentName.Substring( Constant.S_TEMP_PREFIX.Length );
			else
				dlg.FileName				= DateTime.Now.ToString( "yyyy-MM-dd_HH'hr'_mm'm'" );

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strOldFile		= m_mgrWorkspace.File;
				bool			bWasTemp		= strOldFile != null
													&& System.IO.Path.GetFileName( strOldFile ).StartsWith( Constant.S_TEMP_PREFIX );

				string			strFile			= dlg.FileName;

				DoWorkspaceSave( strFile );

				AppSetting.DoRecentWorkspaceAdd( strFile );
				DoMruMenuUpdate();

				if( bWasTemp && strOldFile != strFile )
				{
					try { System.IO.File.Delete( strOldFile ); } catch { }
				}
			}
		}

		private void OnCommandFileSaveLayoutAs( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileSaveLayoutAs();
		}

		private void DoCommandFileSaveLayoutAs()
		{
			Microsoft.Win32.SaveFileDialog
							dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Save a Layout File";
			dlg.InitialDirectory			= AppDomain.CurrentDomain.BaseDirectory;			
			dlg.DefaultExt					= ".xml";
			dlg.Filter						= "Layout XML file (.xml)|*.xml";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strFile			= dlg.FileName;				

				DoLayoutSave( strFile );
			}
		}

		private void OnCommandFileSaveAll( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileSaveAll();
		}

		private void DoCommandFileSaveAll()
		{
			ManagerData		dm				= ManagerData.GetManager();

			ListString		lst				= dm.DoFileSaveAll();

			DoExplorerUpdate();

			StringBuilder	sb				= new StringBuilder();
			foreach( string str in lst )
			{
				sb.Append( str + "\r\n" );
			}

			UtilityMessage.ShowMessageFilesSave( sb.ToString() );
		}

		private void OnCommadFileSaveAllCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagerData		dm				= ManagerData.GetManager();
			
			if( dm.IsEdited == true )
			{
				ea.CanExecute					= true;
															
			}
			else
			{
				ea.CanExecute					= false;
			}
		}

		private void OnCommandFileExportImage( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileExportImage();
		}

		private void OnCommandFileExportImageCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute		= m_dckmVugmap.ActiveDocument is DocMap;
		}

		private void DoCommandFileExportImage()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
				return;

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;

			Microsoft.Win32.SaveFileDialog
								dlg				= new Microsoft.Win32.SaveFileDialog();

			dlg.Title						= "Export Image";
			dlg.FileName					= DateTime.Now.ToString( "yyyy-MM-dd_HH'hr'_mm'm'" );
			dlg.DefaultExt					= ".png";
			dlg.Filter						= "PNG Image (*.png)|*.png|SVG Image (*.svg)|*.svg";

			Nullable< bool >	bResult		= dlg.ShowDialog();

			if( bResult == true )
			{
				string			strFile			= dlg.FileName;
				string			strExt			= System.IO.Path.GetExtension( strFile ).ToLower();

				Cursor			cur				= Cursor;
				Cursor							= Cursors.Wait;

				if( strExt == ".svg" )
					doc.PanelActive.DoExportSvg( strFile );
				else
					doc.PanelActive.DoExportPng( strFile, 300.0 );

				Cursor							= cur;
			}
		}

		private void OnCommandFileCloseAll( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileCloseAll();
		}

		private void DoCommandFileCloseAll()
		{
			ManagerData		dm				= ManagerData.GetManager();

			if( dm.IsEdited == true )
			{
				dm.DoFileCloseAll();
			}		
						
			DoDocumentCloseAll();
			DoExplorerUpdate();

			DoTitleSet( null );
		}		

		private void OnCommadFileCloseAllCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ManagerData		md				= ManagerData.GetManager();
			
			if( md.GetCountDataFile() > 0 )
			{
				ea.CanExecute					= true;
															
			}
			else
			{
				ea.CanExecute					= false;
			}
		}

		private void OnCommandFileExit( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFileExit();
		}

		private void DoCommandFileExit()
		{			
			Close();
		}		

		private void OnCommandViewAppScaleup( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewAppScaleup();
		}

		private void OnCommadViewAppScaleupCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute	= true;
		}

		private void DoCommandViewAppScaleup()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			
			ScaleTransform	stApp			= mw.m_dpVugmap.LayoutTransform as ScaleTransform;

			double			dX				= stApp.ScaleX * 0.9;
			double			dY				= stApp.ScaleY * 0.9;

			stApp.ScaleX	= dX;
			stApp.ScaleY	= dY;
		}

		private void OnCommandViewAppScaledown( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewAppScaledown();
		}

		private void OnCommandViewAppScaledownCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute	= true;
		}

		private void DoCommandViewAppScaledown()
		{
			ScaleTransform	stDoc			= m_dpVugmap.LayoutTransform as ScaleTransform;

			double			dX				= stDoc.ScaleX / 0.9;
			double			dY				= stDoc.ScaleY / 0.9;

			stDoc.ScaleX	= dX;
			stDoc.ScaleY	= dY;
		}

		private void OnCommandViewScaledown( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewScaledown();
		}

		private void OnCommandViewScaledownCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute					= false;
				return;
			}
			else
			{
				ea.CanExecute					= true;
				return;
			}
		}

		private void DoCommandViewScaledown()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );
			
			doc.DoPanelScaleDown();
		}

		private void OnCommandViewScaleup( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewScaleup();
		}

		private void OnCommadViewScaleupCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute					= false;
				return;
			}
			else
			{
				ea.CanExecute					= true;
				return;
			}
		}

		private void DoCommandViewScaleup()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );
			
			doc.DoPanelScaleUp();
		}

		private void OnCommandViewZoomin( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewZoomin();
		}

		private void OnCommandViewZoominCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute					= false;
				return;
			}

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.Zoom >= PnlMap.N_MAP_ZOOMMAX )
			{
				ea.CanExecute					= false;
				return;
			}
			else
			{
				ea.CanExecute					= true;
				return;
			}
		}

		private void DoCommandViewZoomin()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelZoomIn();
			}
		}

		private void OnCommandViewZoomout( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewZoomout();
		}

		private void OnCommandViewZoomoutCan( object obj, CanExecuteRoutedEventArgs ea )
		{			
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.Zoom <= PnlMap.N_MAP_ZOOMMIN )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				ea.CanExecute		= true;
				return;
			}
		}

		private void DoCommandViewZoomout()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelZoomOut();
			}
		}

		private void OnCommandViewZoomTo( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewZoomTo();
		}

		private void OnCommandViewZoomToCan( object obj, CanExecuteRoutedEventArgs ea )
		{			
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;				
				Debug.Assert( doc != null );						

				ea.CanExecute		= true;
				return;			
			}		
		}

		private void DoCommandViewZoomTo()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap			doc				= DoDocumentActive();			

				DialogZoomTo	dlg				= new DialogZoomTo( doc );								
				dlg.Owner						= MainWindow.GetMainWindow();
				dlg.SetElementValue();
			
				Nullable< bool >	b			= dlg.ShowDialog();
				if( b == true )
				{
					double			dZoom			= dlg.DoZoomGet();

					doc.DoPanelZoomTo( dZoom );
				}
			}
		}

		private void OnCommandViewScrollLeft( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewScrollLeft();
		}

		private void OnCommandViewScrollLeftCan( object obj, CanExecuteRoutedEventArgs ea )
		{			
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );
						
			if( doc.PanelActive.Position <= doc.PanelActive.PositionMin )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				ea.CanExecute		= true;
				return;
			}
		}

		private void DoCommandViewScrollLeft()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelScrollLeft();
			}
		}

		private void OnCommandViewScrollRight( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewScrollRight();
		}

		private void OnCommandViewScrollRightCan( object obj, CanExecuteRoutedEventArgs ea )
		{			
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );
			
			int				nPage			= UtilityMath.DoRound( ( ( double ) ( doc.PanelActive.PositionMax - doc.PanelActive.PositionMin ) ) / doc.PanelActive.Zoom );
			int				nMax			= doc.PanelActive.PositionMax - nPage;


			if( doc.PanelActive.Position >= nMax )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				ea.CanExecute		= true;
				return;
			}
		}

		private void DoCommandViewScrollRight()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelScrollRight();
			}
		}

		private void OnCommandViewScrollLeftSmall( object obj, ExecutedRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelScrollLeftSmall();
			}
		}

		private void OnCommandViewScrollLeftSmallCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			ea.CanExecute		= doc.PanelActive.Position > doc.PanelActive.PositionMin;
		}

		private void OnCommandViewScrollRightSmall( object obj, ExecutedRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelScrollRightSmall();
			}
		}

		private void OnCommandViewScrollRightSmallCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			int				nPage			= UtilityMath.DoRound( ( ( double ) ( doc.PanelActive.PositionMax - doc.PanelActive.PositionMin ) ) / doc.PanelActive.Zoom );
			ea.CanExecute		= doc.PanelActive.Position < doc.PanelActive.PositionMax - nPage;
		}

		private void OnCommandViewGoHome( object obj, ExecutedRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelPositionTo( doc.PanelActive.PositionMin );
			}
		}

		private void OnCommandViewGoHomeCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute		= m_dckmVugmap.ActiveDocument is DocMap;
		}

		private void OnCommandViewGoEnd( object obj, ExecutedRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				int				nPage			= UtilityMath.DoRound( ( ( double ) ( doc.PanelActive.PositionMax - doc.PanelActive.PositionMin ) ) / doc.PanelActive.Zoom );
				doc.DoPanelPositionTo( doc.PanelActive.PositionMax - nPage );
			}
		}

		private void OnCommandViewGoEndCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute		= m_dckmVugmap.ActiveDocument is DocMap;
		}

		private void OnCommandViewTabNext( object obj, ExecutedRoutedEventArgs ea )
		{
			int				nCount			= m_dckmVugmap.MainDocumentPane.Items.Count;
			if( nCount <= 1 )
				return;

			int				nIndex			= m_dckmVugmap.MainDocumentPane.Items.IndexOf( m_dckmVugmap.ActiveDocument );
			int				nNext			= ( nIndex + 1 ) % nCount;
			ManagedContent	mc				= m_dckmVugmap.MainDocumentPane.Items[ nNext ] as ManagedContent;
			if( mc != null )
				mc.SetAsActive();
		}

		private void OnCommandViewTabNextCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute		= m_dckmVugmap.MainDocumentPane.Items.Count > 1;
		}

		private void OnCommandViewTabPrev( object obj, ExecutedRoutedEventArgs ea )
		{
			int				nCount			= m_dckmVugmap.MainDocumentPane.Items.Count;
			if( nCount <= 1 )
				return;

			int				nIndex			= m_dckmVugmap.MainDocumentPane.Items.IndexOf( m_dckmVugmap.ActiveDocument );
			int				nPrev			= ( nIndex - 1 + nCount ) % nCount;
			ManagedContent	mc				= m_dckmVugmap.MainDocumentPane.Items[ nPrev ] as ManagedContent;
			if( mc != null )
				mc.SetAsActive();
		}

		private void OnCommandViewTabPrevCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute		= m_dckmVugmap.MainDocumentPane.Items.Count > 1;
		}

		private void OnCommandViewRefresh( object obj, ExecutedRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
				doc.DoPanelUpdateView();
			}
		}

		private void OnCommandViewRefreshCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			ea.CanExecute		= m_dckmVugmap.ActiveDocument is DocMap;
		}

		//				.								.								.
		//				NumPad Feature Adjust Commands
		//				.								.								.

		private void OnCommandFeatureMoveLeft( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFeatureAdjust( -1, -1 );
		}

		private void OnCommandFeatureMoveRight( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFeatureAdjust( 1, 1 );
		}

		private void OnCommandFeatureShrinkStart( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFeatureAdjust( 0, -1 );
		}

		private void OnCommandFeatureExpandEnd( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandFeatureAdjust( 0, 1 );
		}

		private void OnCommandFeatureAdjustCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}

			ea.CanExecute			= true;
		}

		private void DoCommandFeatureAdjust( int nStartDelta, int nEndDelta )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
				return;

			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;
			PnlMap				pm				= doc.PanelActive;
			if( pm == null )
				return;

			//	Collect selected features across all lanes
			ListFeature			lstSelected		= new ListFeature();
			PnlMapLane			pnlOwner		= null;

			foreach( PnlMapLane pnl in pm.LaneList )
			{
				ListFeature		lst				= pnl.ListFeatureSelected;
				if( lst != null && lst.Count > 0 )
				{
					lstSelected.AddRange( lst );
					pnlOwner					= pnl;
				}
			}

			if( lstSelected.Count == 0 )
				return;

			if( lstSelected.Count >= 2 )
			{
				MessageBox.Show( "Please select only one feature.", "MetaScope", MessageBoxButton.OK, MessageBoxImage.Information );
				return;
			}

			//	Single feature selected — adjust
			DataFeature			dfOld			= lstSelected[ 0 ];
			DataFeature			dfNew			= new DataFeature( dfOld );
			if( dfNew.ColorBrush == null )
				dfNew.ColorBrush				= dfOld.ColorBrush;

			dfNew.Start						   += nStartDelta;
			dfNew.End						   += nEndDelta;

			//	Undo support — consolidate consecutive adjustments on same feature
			ManagerEdit			me				= ManagerEdit.GetManager();
			CommandEdit			cmd				= null;

			if( me.GetCount() > 0 )
			{
				CommandBase		cbLast			= me.GetCommandLast();
				if( cbLast is CommandEdit )
				{
					CommandEdit	ceLast			= cbLast as CommandEdit;
					if( ceLast.LaneOwner == pnlOwner && ceLast.FeatureCurrent == dfOld )
					{
						cmd						= ceLast;
						cmd.UpdateAdjust( dfOld, dfNew );
					}
				}
			}

			if( cmd == null )
			{
				cmd								= me.MakeCommandEdit();
				cmd.DoFeatureAdd( pnlOwner, dfOld, dfNew );
				cmd.SetAdjustInfo( pnlOwner, dfOld, dfNew );
			}

			//	Apply
			DataType			dt				= pnlOwner.DataTypeSelected;
			dt.DoFeatureRemove( dfOld );
			dt.DoFeatureAdd( dfNew );

			//	Re-select the new feature
			pnlOwner.DoFeatureSelect( dfNew );

			//	Update highlight to follow adjusted feature
			pm.DoFeatureHighlightSet( dfNew.Start, dfNew.End );

			DoEditUpdate();
			pnlOwner.DoLayoutUpdate();

			DoAutoSaveDebounce();
		}

		private void OnCommandViewPositionTo( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewPositionTo();
		}

		private void OnCommandViewPositionToCan( object obj, CanExecuteRoutedEventArgs ea )
		{			
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;				
				Debug.Assert( doc != null );						

				ea.CanExecute		= true;
				return;			
			}
		}

		private void DoCommandViewPositionTo()
		{
			if( m_dckmVugmap.ActiveDocument is DocMap )
			{
				DocMap			doc				= DoDocumentActive();			

				DialogPositionTo	dlg			= new DialogPositionTo( doc );								
				dlg.Owner						= MainWindow.GetMainWindow();
				dlg.SetElementValue();
			
				Nullable< bool >	b			= dlg.ShowDialog();
				if( b == true )
				{
					int				nPosition		= dlg.DoPositionGet();					

					doc.DoPanelPositionTo( nPosition );
				}
			}
		}

		private void OnCommandViewSplit( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewSplit();
		}

		private void OnCommandViewSplitCan( object obj, CanExecuteRoutedEventArgs ea )
		{			
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;				
				Debug.Assert( doc != null );
			
				if( doc.IsSplitted == true )
					m_miViewSplit.IsChecked				= true;
				else
					m_miViewSplit.IsChecked				= false;

				ea.CanExecute		= true;
				return;			
			}
		}

		private void DoCommandViewSplit()
		{
			DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;				
			Debug.Assert( doc != null );

			if( doc.IsSplitted == true )
				doc.DoSplitSet( false );
			else
				doc.DoSplitSet( true );
		}

		private void OnCommandViewFeatureOpacity( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandViewFeatureOpacity();
		}

		private void OnCommandViewFeatureOpacityCan( object obj, CanExecuteRoutedEventArgs ea )
		{
			if( m_dckmVugmap.ActiveDocument is DocMap == false )
			{
				ea.CanExecute		= false;
				return;
			}
			else
			{
				DocMap				doc				= m_dckmVugmap.ActiveDocument as DocMap;				
				Debug.Assert( doc != null );						

				ea.CanExecute		= true;
				return;			
			}
		}	

		private void DoCommandViewFeatureOpacity()
		{
			DialogFeatureOpacity	dlg		= new DialogFeatureOpacity();
			dlg.Owner						= MainWindow.GetMainWindow();			
			dlg.SetElementValue();
			
			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
				string			strOpacity		= dlg.DoOpacityGet();
				
				ManagerBrush.DoOpacitySet( strOpacity );

				foreach( DocMap dm in m_lstMap )
				{
					foreach( PnlMapLane pml in dm.PanelMap.LaneList )
					{
						foreach( DataType dt in pml.DataTypeList )
						{
							Brush			bshOld			= dt.DoBrushGet();
							Brush			bshNew			= ManagerBrush.GetManager().GetBrush( bshOld as SolidColorBrush );
							
							dt.DoBrushSet( bshNew );
						}
					}
					
					dm.DoUpdateView();
				}
			}
		}		

		private void OnCommandHelpTest( object obj, ExecutedRoutedEventArgs ea )
		{
			//Debug.Assert( false );	
			DialogTest		dlg				= new DialogTest();
			//dlg.Owner						= this;
			dlg.ShowDialog();			
		}

		private void OnCommandHelpDocumentation( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandHelpDocumentation();
		}

		private void DoCommandHelpDocumentation()
		{
			System.Diagnostics.Process.Start( "https://github.com/sbml-lab/MetaScope" );
		}

		private void OnCommandHelpUpdate( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandHelpUpdate();
		}

		private void DoCommandHelpUpdate()
		{
			System.Diagnostics.Process.Start( "https://github.com/sbml-lab/MetaScope" );
		}

		private void OnCommandHelpShortcuts( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandHelpShortcuts();
		}

		private void DoCommandHelpShortcuts()
		{
			DialogShortcuts		dlg			= new DialogShortcuts();
			dlg.Owner						= this;
			dlg.ShowDialog();
		}

		private void OnCommandHelpAutoSave( object obj, ExecutedRoutedEventArgs ea )
		{
			IsAutoSave						= !IsAutoSave;
			m_mniAutoSave.IsChecked			= IsAutoSave;
			AppSetting.AutoSave				= IsAutoSave;
			string		strMsg				= IsAutoSave ? "Auto Save enabled" : "Auto Save disabled";
			UtilityMessage.ShowMessage( strMsg );
		}

		private void OnAutoSaveTick( object obj, EventArgs ea )
		{
			m_tmrAutoSave.Stop();

			if( m_bAutoSave == false )
				return;

			ManagerData		dm				= ManagerData.GetManager();
			if( dm == null || dm.IsEdited == false )
				return;

			dm.DoFileSaveAll();
		}

		public bool IsAutoSave
		{
			get {	return m_bAutoSave; }
			set
			{
				m_bAutoSave						= value;
				if( m_bAutoSave == false )
					m_tmrAutoSave.Stop();
			}
		}

		public void DoAutoSaveDebounce()
		{
			if( m_bAutoSave == false )
				return;
			m_tmrAutoSave.Stop();
			m_tmrAutoSave.Start();
		}

		public void DoAutoSaveImmediate()
		{
			if( m_bAutoSave == false )
				return;

			m_tmrAutoSave.Stop();

			ManagerData		dm				= ManagerData.GetManager();
			if( dm == null || dm.IsEdited == false )
				return;

			dm.DoFileSaveAll();
		}

		public void DoWorkspaceSaveDebounce()
		{
			if( m_bAutoSave == false )
				return;
			if( m_mgrWorkspace == null || System.IO.File.Exists( m_mgrWorkspace.File ) == false )
				return;

			m_tmrWorkspaceSave.Stop();
			m_tmrWorkspaceSave.Start();
		}

		private void OnWorkspaceSaveTick( object obj, EventArgs ea )
		{
			m_tmrWorkspaceSave.Stop();

			if( m_bAutoSave == false )
				return;
			if( m_mgrWorkspace == null || System.IO.File.Exists( m_mgrWorkspace.File ) == false )
				return;

			m_mgrWorkspace.DoSave();
		}

		private void OnCommandHelpAbout( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandHelpAbout();
		}

		private void DoCommandHelpAbout()
		{
			System.Diagnostics.Process.Start( "https://sbml-lab.ai/software" );
		}

		private void OnCommandHelpGarbageCollection( object obj, ExecutedRoutedEventArgs ea )
		{
			DoCommandHelpGarbageCollection();
		}

		private void DoCommandHelpGarbageCollection()
		{
			GC.Collect();
		}

		private void OnCommandVugTest1( object obj, ExecutedRoutedEventArgs ea )
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else if( doc.PanelActive.ListLaneEditable[ 0 ].GetCountFeatureSelected() != 1 )
			{
				MessageBox.Show( "Select only 1 feature!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );				
			}			
			else
			{
				doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ].Start--;
				doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ].End--;
				doc.PanelActive.ListLaneEditable[ 0 ].InvalidateVisual();
				doc.PanelActive.ListLaneEditable[ 0 ].DoFeatureSelect( doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ] );
				doc.PanelActive.DoSelection();
			}
		}

		private void OnCommandVugTest2( object obj, ExecutedRoutedEventArgs ea )
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else if( doc.PanelActive.ListLaneEditable[ 0 ].GetCountFeatureSelected() != 1 )
			{
				MessageBox.Show( "Select only 1 feature!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );				
			}			
			else
			{
				doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ].Start++;
				doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ].End++;
				doc.PanelActive.ListLaneEditable[ 0 ].InvalidateVisual();
				doc.PanelActive.ListLaneEditable[ 0 ].DoFeatureSelect( doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ] );
			}
		}
		
		private void OnCommandVugTest4( object obj, ExecutedRoutedEventArgs ea )
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else if( doc.PanelActive.ListLaneEditable[ 0 ].GetCountFeatureSelected() != 1 )
			{
				MessageBox.Show( "Select only 1 feature!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );				
			}			
			else
			{
				doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ].End--;
				doc.PanelActive.ListLaneEditable[ 0 ].InvalidateVisual();
				doc.PanelActive.ListLaneEditable[ 0 ].DoFeatureSelect( doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ] );
			}		
		}

		private void OnCommandVugTest5( object obj, ExecutedRoutedEventArgs ea )
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;
			Debug.Assert( doc != null );

			if( doc.PanelActive.ListLaneEditable.Count == 0 )
			{
				ErrorMessage.ShowErrorSelectLaneFirst();
			}
			else if( doc.PanelActive.ListLaneEditable[ 0 ].GetCountFeatureSelected() != 1 )
			{
				MessageBox.Show( "Select only 1 feature!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );				
			}			
			else
			{
				doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ].End++;
				doc.PanelActive.ListLaneEditable[ 0 ].InvalidateVisual();
				doc.PanelActive.ListLaneEditable[ 0 ].DoFeatureSelect( doc.PanelActive.ListLaneEditable[ 0 ].ListFeatureSelected[ 0 ] );
			}		
		}

		public void DoFeatureSelectedSet( ListFeature lstFeature )
		{
			PropertyFeatureGroup	
							pptGroup		= new PropertyFeatureGroup();
			pptGroup.SetFeature( lstFeature );

			WindowsFormsHost	wfh			= m_dcntFeatureSelected.Content as WindowsFormsHost;
			System.Windows.Forms.PropertyGrid
								pg			= wfh.Child as System.Windows.Forms.PropertyGrid;

			pg.SelectedObject				= null;
			pg.SelectedObject				= pptGroup;
		}

		public void DoFeatureSelectedSet( DataFeature df )
		{
			WindowsFormsHost	wfh			= m_dcntFeatureSelected.Content as WindowsFormsHost;
			System.Windows.Forms.PropertyGrid
								pg			= wfh.Child as System.Windows.Forms.PropertyGrid;

			if( df == null )
			{
				pg.SelectedObject				= null;
			}
			else
			{
				m_pptSelected.SetFeature( df );

				pg.SelectedObject				= null;				
				pg.SelectedObject				= m_pptSelected;
			}
		}

		public void DoFeatureSet( DataFeature df )
		{
			WindowsFormsHost	wfh			= m_dcntFeature.Content as WindowsFormsHost;
			System.Windows.Forms.PropertyGrid
								pg			= wfh.Child as System.Windows.Forms.PropertyGrid;

			if( df == null )
			{
				pg.SelectedObject				= null;
			}
			else
			{
				//pg.SelectedObject				= new PropertyFeature( df );
				//m_pptFeature.SetFeature( df );

				pg.SelectedObject				= null;
				pg.SelectedObject				= PropertyFeature.BuildProperty( df );
			}		
		}
		
		public void DoEditUpdate()
		{
			m_ltvEdit.Items.Clear();

			ManagerEdit		me				= ManagerEdit.GetManager();

			foreach( CommandBase cb in me.GetCommand() )
			{
				string			str				= cb.GetString();

				m_ltvEdit.Items.Add( str );
			}

			m_dcntEdit.SetAsActive();
		}

		private void InitializePropertyFeature()
		{
			System.Windows.Forms.PropertyGrid	
							pg				= new System.Windows.Forms.PropertyGrid();
			WindowsFormsHost	
							wfh				= new WindowsFormsHost();
			
			m_pptFeature					= null;
			
			pg.SelectedObject				= m_pptFeature;
			pg.HelpVisible					= false;

			pg.PropertyValueChanged			+= delegate( object obj, System.Windows.Forms.PropertyValueChangedEventArgs ea ) 
			{
				MessageBox.Show( ea.ToString() ); 
			}; 

			wfh.Child						= pg;

			m_dcntFeature.Content			= wfh;
		}

		private void InitializePropertyFeatureSelected()
		{
			System.Windows.Forms.PropertyGrid	
							pg				= new System.Windows.Forms.PropertyGrid();
			WindowsFormsHost	
							wfh				= new WindowsFormsHost();
			
			m_pptSelected					= new PropertyFeatureSelected();
			
			pg.SelectedObject				= m_pptSelected;			
			pg.HelpVisible					= false;

			pg.PropertyValueChanged			+= delegate( object obj, System.Windows.Forms.PropertyValueChangedEventArgs ea ) 
			{
				MessageBox.Show( ea.ToString() ); 
			}; 

			wfh.Child						= pg;

			m_dcntFeatureSelected.Content	= wfh;
		}

		private void InitializePropertyVugmap()
		{
			System.Windows.Forms.PropertyGrid	
							pg				= new System.Windows.Forms.PropertyGrid();
			WindowsFormsHost	
							wfh				= new WindowsFormsHost();
			
			pg.SelectedObject				= new PropertyVugmap();

			pg.PropertyValueChanged			+= delegate( object obj, System.Windows.Forms.PropertyValueChangedEventArgs ea ) 
			{
				MessageBox.Show( ea.ToString() ); 
			}; 

			wfh.Child						= pg;

			m_dcntSetting.Content			= wfh;
		}
		
		public void PrintLog( string strLog )
		{
			m_tbLogLog.Text					= strLog + m_tbLogLog.Text;											
		}

		public void PrintLogLine( string strLog )
		{
			strLog			= string.Format( "{0}\r\n", strLog );

			PrintLog( strLog );
		}

		public void PrintLog( string strFormat, params object[] objArgumentA )
		{
			string			strLog			= string.Format( strFormat, objArgumentA );

			PrintLog( strLog );
		}

		public void PrintLogLine( string strFormat, params object[] objArgumentA )
		{
			string			strLog			= string.Format( strFormat, objArgumentA );

			PrintLogLine( strLog );
		}
		
		private void mniWindow_ViewZoomin_Click( object obj, RoutedEventArgs ea )
		{
			DoCommandViewZoomin();
		}

		private void mniWindow_ViewZoomout_Click( object obj, RoutedEventArgs ea )
		{
			DoCommandViewZoomout();
		}

		private void mniWindow_ThemeReset_Click( object obj, RoutedEventArgs ea )
        {
            ( ( DocumentContent) m_dckmVugmap.ActiveDocument ).Close();

            ColorFactory.ResetColors();

			PrintLogLine( string.Format( "MainWindow\t\t: mniWindowThemeReset_Click" ) );
		}

		private void mniWindow_ThemeColor_Click( object obj, RoutedEventArgs ea )
		{
			MenuItem		mi				= obj as MenuItem;

			switch( mi.Tag.ToString() )
			{
				case "red" :
					ColorFactory.ChangeColors( Colors.Red );
					break;
				case "green" :
					ColorFactory.ChangeColors( Colors.DarkGreen );
					break;
				case "blue" :
					ColorFactory.ChangeColors( Color.FromRgb( 93, 136, 230 ) );
					break;
				case "gray" :
					ColorFactory.ChangeColors( Colors.Black );
					break;
				case "orange" :
					ColorFactory.ChangeColors( Colors.DarkOrange );
					break;
				case "lime" :
					ColorFactory.ChangeColors( Colors.Lime );
					break;
				case "magenta" :
					ColorFactory.ChangeColors( Colors.Magenta );
					break;
			}

			PrintLogLine( string.Format( "MainWindow\t\t: mniWindowThemeColor_Click, color change to {0}", mi.Tag.ToString() ) );
		}
				
		private void mniFile_Newdocument_Click( object obj, RoutedEventArgs ea )
        {
			DoDocumentNew();
        }

		private void m_docIntroductionNewDocument( object obj, RoutedEventArgs ae )
		{
			DoDocumentNew();
		}

		public void DoMapUpdateActive()
		{
			object			obj				= m_dckmVugmap.ActiveDocument;
			if( obj is DocMap )
			{
				DocMap			doc				= obj as DocMap;

				doc.DoPanelUpdateView();
			}
		}

		public void DoMapUpdate( string[] strFileA )
		{
			ManagerData		md				= ManagerData.GetManager();

			for( int i = 0; i < md.GetCountSequenceId(); i++ )
			{
				string			strSequenceId	= md.GetSequenceId( i );
				DocMap			dm				= DoDocumentFind( strSequenceId );

				if( dm == null )
				{
					dm				= DoDocumentNew();
					dm.SequenceId	= strSequenceId;
					dm.IsLocked		= md.IsSequenceIdReadOnly( strSequenceId );
				}			

				if( dm.PanelActive.Zoom == 0.0f )
					dm.DoPanelZoomSet( 1.0f );
				
				if( strFileA == null )
				{
					dm.DoPanelLaneRemove();
				}
				else
				{					
					dm.DoPanelLaneAdd( strFileA );
				}

				dm.DoPanelUpdateView();
			}

			ListMap			lstMap			= m_lstMap.GetRange( 0, m_lstMap.Count );

			foreach( DocMap dm in lstMap )
			{
				if( md.IsContainingSequenceId( dm.SequenceId ) == false )
				{
					DoDocumentClose( dm );
				}
			}

			DoMapRemoveNoSequenceId();
			DoExplorerUpdate();
			DoStatusBarUpdate();
		}

		private void DoMapRemoveNoSequenceId()
		{
			lock( m_lstMap )
			{
				ListMap			lstRemove		= new ListMap();

				foreach( DocMap dm in m_lstMap )
				{
					if( dm.SequenceId == null )
					{
						lstRemove.Add( dm );
					}
				}

				foreach( DocMap dm in lstRemove )
				{
					DoDocumentClose( dm );
				}
			}
		}

		public DocMap DoDocumentActive()
		{
			object			obj				= m_dckmVugmap.ActiveDocument;

			if( obj is DocMap )
			{
				DocMap			doc				= obj as DocMap;

				return doc;
			}

			return null;
		}

		public DocMap DoDocumentFind( string strSequenceId )
		{
			foreach( DocMap dm in m_lstMap )
			{
				if( dm.SequenceId == strSequenceId )
					return dm;
			}

			return null;
		}

		private DocMap DoDocumentShow( string strSequenceId )
		{
			DocMap			dm				= DoDocumentFind( strSequenceId );
			if( dm != null )
			{
				if( m_dckmVugmap.MainDocumentPane.Items.Contains( dm ) == false )
					m_dckmVugmap.MainDocumentPane.Items.Add( dm );

				dm.SetAsActive();
			}

			return dm;
		}

		private void DoDocumentCloseAll()
		{
			ManagerData		md				= ManagerData.GetManager();
			md.DoFileCloseAll();

			if( m_dckmVugmap.MainDocumentPane != null )
			{
				ListMap			lst				= new ListMap();
				lst.AddRange( ListDocument );
				
				foreach( DocMap doc in lst )
				{
					DoDocumentClose( doc );					
				}
			}

			DoExplorerUpdate();			
		}

		public bool IsDocumentIntroductionVisible()
		{
			if( m_dckmVugmap.MainDocumentPane.Items.Contains( m_docIntroduction ) == true )
				return true;
			else
				return false;
		}

		public void DoDocumentClose( DocMap doc )
		{
			doc.DoClose();

			m_lstMap.Remove( doc );
			if( m_dckmVugmap.MainDocumentPane.Items.Contains( doc ) == true )
			{
				m_dckmVugmap.MainDocumentPane.Items.Remove( doc );
			}

			if( m_lstMap.Count == 0 )
			{
				m_docIntroduction.Focus();
			}
		}

		public void DoDocumentMove( string strSequenceId, int nPosition, int nEnd )
		{
			DocMap			dm				= DoDocumentFind( strSequenceId );
			if( dm == null )
			{
				ErrorMessage.ShowErrorSearchNoDocumentOpen( strSequenceId );
				return;
			}

			int				nPosCenter		= UtilityMath.DoRound( nPosition - dm.PanelActive.PositionRange / dm.PanelActive.Zoom / 2 );
			int				nPosEnd			= UtilityMath.DoRound( nPosCenter + dm.PanelActive.PositionRange / dm.PanelActive.Zoom );			
					
			dm.SetPanelPosition( nPosCenter );
			dm.DoPanelUpdateView();		
		}

		public void DoDocumentSelection( string strSequenceId, int nPosition, int nEnd )
		{
			DocMap			dm				= DoDocumentFind( strSequenceId );
			if( dm == null )
			{
				ErrorMessage.ShowErrorSearchNoDocumentOpen( strSequenceId );
				return;
			}

			double			dStart			= dm.PanelActive.GetPixelFromPosition( nPosition );
			double			dEnd			= dm.PanelActive.GetPixelFromPosition( nEnd );
			double			dWidth			= dEnd - dStart;
			
			dm.DoPanelSelection( dStart, dWidth );
		}

		private DocMap DoDocumentNew()
		{
			DocMap			dm				= new DocMap();
			dm.Title						= "NewMap";
			dm.IsFloatingAllowed			= true;
			dm.Dispatcher.Thread.Priority	= ThreadPriority.AboveNormal;
			dm.LayoutTransform				= new ScaleTransform();

			string			strDocScaleX	= m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_DOCUMENTSCALEX ];
			if( strDocScaleX != null )
			{
				double			d			= double.Parse( strDocScaleX );

				if( double.IsNaN( d ) == false )
				{
					ScaleTransform	st			= dm.LayoutTransform as ScaleTransform;
					st.ScaleX					= d;
				}				
			}

			string			strDocScaleY	= m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_DOCUMENTSCALEY ];
			if( strDocScaleY != null )
			{
				double			d			= double.Parse( strDocScaleY );

				if( double.IsNaN( d ) == false )
				{
					ScaleTransform	st			= dm.LayoutTransform as ScaleTransform;
					st.ScaleY					= d;
				}
			}

			m_dckmVugmap.MainDocumentPane.Items.Add( dm );			
			dm.SetAsActive();

			m_lstMap.Add( dm );

			return dm;
		}
		
		public void DoBookmarkUpdate()
		{
			m_ltvBookmark.Items.Clear();

			ManagerBookmark	mb			= ManagerBookmark.GetManager();

			foreach( DataBookmark db in mb.ListBookmark )
			{
				string			str				= db.GetString();

				ListViewItem	lvi				= new ListViewItem();				
				lvi.Content						= str;

				m_ltvBookmark.Items.Add( str );
			}

			m_dcntBookmark.SetAsActive();							
		}

		public void DoExplorerUpdate()
		{
			DoExplorerUpdateFile();	
			DoExplorerUpdateSequenceId();
			DoExplorerUpdateType();

			m_dcntExplorer.SetAsActive();
		}

		private void DoExplorerUpdateFile()
		{
			ManagerData		md				= ManagerData.GetManager();

			m_tviFile.Items.Clear();
			m_tviFile.IsExpanded			= true;

			for( int i = 0;i < md.GetCountDataFile(); i++ )
			{
				DataFile		df				= md.GetDataFile( i );

				TreeViewItem	tviFile			= new TreeViewItem();
				tviFile.Header					= ( df.IsReadOnly ? "[R] " : "" ) + ( df.IsEdited == false ? string.Format( "{0}", df.FileName ) : string.Format( "{0} (*)", df.FileName ) );
				tviFile.IsExpanded				= true;
				tviFile.ToolTip					= string.Format( "File: {0}", df.FileName );
				tviFile.ContextMenu				= m_cmExplorerFile;				

				for( int j = 0; j < df.GetCountDataType(); j++ )
				{
					DataType		dt				= df.GetDataType( j );

					TreeViewItem	tviType			= new TreeViewItem();
					tviType.Header					= dt.IsEdited == false ? 
														string.Format( "{0}:{1}", dt.SequenceId, dt.Type ) : 
														string.Format( "{0}:{1} (*)", dt.SequenceId, dt.Type );
					tviType.IsExpanded				= false;
					tviType.ContextMenu				= null;
					tviType.MouseDoubleClick		+= delegate( object obj, MouseButtonEventArgs ea )
					{
						string			strSequenceId	= dt.SequenceId;
						string			strType			= dt.Type;

						DocMap			doc				= DoDocumentShow( strSequenceId );
						doc.DoPanelLaneShow( strType );
					};

					tviFile.Items.Add( tviType );
				}

				m_tviFile.Items.Add( tviFile );
			}
		}

		private void DoExplorerUpdateSequenceId()
		{
			ManagerData		md				= ManagerData.GetManager();

			m_tviSequenceId.Items.Clear();
			m_tviSequenceId.IsExpanded		= true;

			ListString		lst				= new ListString();

			for( int i = 0;i < md.GetCountDataFile(); i++ )
			{
				DataFile		df				= md.GetDataFile( i );

				for( int j = 0; j < df.GetCountSequenceId(); j++ )
				{
					string			strSequenceId	= df.GetSequenceId( j );

					if( lst.Contains( strSequenceId ) == false )
						lst.Add( strSequenceId );				
				}				
			}

			foreach( string str in lst )
			{
				TreeViewItem	tvi				= new TreeViewItem();
				tvi.Header						= str;
				tvi.IsExpanded					= true;
				tvi.ToolTip						= string.Format( "Type: {0}", str );
				tvi.MouseDoubleClick			+= delegate( object obj, MouseButtonEventArgs ea )
				{
					DoDocumentShow( str );
				};

				m_tviSequenceId.Items.Add( tvi );
			}
		}

		private void DoExplorerUpdateType()
		{
			ManagerData		md				= ManagerData.GetManager();

			m_tviType.Items.Clear();
			m_tviType.IsExpanded			= true;

			ListString		lst				= new ListString();

			for( int i = 0;i < md.GetCountDataFile(); i++ )
			{
				DataFile		df				= md.GetDataFile( i );

				for( int j = 0; j < df.GetCountType(); j++ )
				{
					string			strType			= df.GetType( j );

					if( lst.Contains( strType ) == false )
						lst.Add( strType );				
				}				
			}

			foreach( string str in lst )
			{
				TreeViewItem	tvi				= new TreeViewItem();
				tvi.Header						= str;
				tvi.IsExpanded					= true;				
				tvi.ToolTip						= string.Format( "Type: {0}", str );

				m_tviType.Items.Add( tvi );
			}
		}

		private void DoLayoutSave( string strFile )
		{
			m_dckmVugmap.SaveLayout( strFile );

			FileInfo		fi				= new FileInfo( strFile );
			UtilityMessage.ShowMessageFileSaveLayout( fi.Name );
		}

		private void DoLayoutSave()
		{
			string			strPath			= System.IO.Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            string			strLayout		= string.Format( "{0}\\{1}", strPath, @"VugMap.Layout.xml" );

			DoLayoutSave( strLayout );
		}

		private void DoLayoutRestore()
		{
			string			strPath			= System.IO.Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
			string			strLayout		= string.Format( "{0}\\{1}", strPath, @"VugMap.Layout.xml" );

			DoLayoutRestore( strLayout );
		}

		public string GetFileLayout()
		{
			return m_strFileLayout;
		}

		public double GetLayoutApplicationScaleX()
		{
			ScaleTransform	st			= m_dpVugmap.LayoutTransform as ScaleTransform;
			double			d			= st.ScaleX;

			return d;
		}

		public double GetLayoutApplicationScaleY()
		{
			ScaleTransform	st			= m_dpVugmap.LayoutTransform as ScaleTransform;
			double			d			= st.ScaleY;

			return d;
		}

		public double GetLayoutDocumentScaleX()
		{
			return m_dDocumentScaleX;
		}

		public double GetLayoutDocumentScaleY()
		{
			return m_dDocumentScaleY;
		}

		private void DoLayoutRestore( string strFile )
		{				
            if( File.Exists( strFile ) == false )
			{
				return;
			}
             
			m_dckmVugmap.DeserializationCallback = ( s, e_args ) =>
				{
					if( e_args.Name == "_contentDummy" )
					{
						e_args.Content					= new DockableContent();
						e_args.Content.Title			= "Dummy Content";
						e_args.Content.Content			= new TextBlock() { Text = "Content Loaded On Demand!"};														
					}
				};

            FileInfo		fi				= new FileInfo( strFile );
			FileStream		fs				= new FileStream( strFile, FileMode.Open, FileAccess.Read );
            
			m_dckmVugmap.RestoreLayout( fs) ;
            fs.Close();

			m_strFileLayout					= fi.FullName;
		}
		
		private void OnIntroductionMouseDown( object obj, MouseButtonEventArgs ea )
		{
			if( ea.ClickCount == 2 )
			{
				DoDocumentNew();
			}
		}

		public void DoDrop( string[] strFileA )
		{
			DoDrop( strFileA, false );
		}

		public void DoDrop( string[] strFileA, bool bEndWork )
		{
			if( strFileA == null || strFileA.Length == 0 || UtilityFile.GetFileExist( strFileA[ 0 ] ) == false )
				return;

			FileInfo		fi				= new FileInfo( strFileA[ 0 ] );
			if( fi.Extension == ".workspace" )
			{
				if( strFileA.Length > 1 )
				{
					ErrorMessage.ShowErrorFileWorkspace();
					return;
				}

				DoWorkspaceOpen( strFileA[ 0 ] );

				m_mgrWorkspace.IsEdited				= false;
			}
			else
			{
				DialogFileOpen	dlg				= new DialogFileOpen();
				dlg.Owner						= this;			
				dlg.EndWork						= bEndWork;			
				dlg.SetFile( strFileA );

				bool			b				= dlg.DoReadFile();
				if( b == true )
					dlg.ShowDialog();

				m_mgrWorkspace.IsEdited				= true;

				if( System.IO.File.Exists( m_mgrWorkspace.File ) == false )
				{
					string		strTempName		= string.Format( "{0}{1}.workspace",
													Constant.S_TEMP_PREFIX,
													DateTime.Now.ToString( "yyyy-MM-dd_HH'hr'_mm'm'" ) );
					string		strTempPath		= System.IO.Path.Combine(
													AppSetting.AppDataDir, strTempName );

					m_mgrWorkspace.File				= strTempPath;
					m_mgrWorkspace.DoSave();
				}
			}
		}

		public void DoDropEndWork()
		{				
			foreach( WorkspaceMap wm in m_mgrWorkspace.Map )
			{
				ManagerData		md				= ManagerData.GetManager();
				DocMap			dm				= DoDocumentFind( wm.SequenceId );

				ListMapLane		lst				= new ListMapLane();
								
				foreach( WorkspaceLane wl in wm.ListLane )
				{										
					PnlMapLane		pnl				= dm.PanelActive.GetLane( wl.ListType[ 0 ].Type );

					if( pnl == null )				continue;

					foreach( WorkspaceType wt in wl.ListType )
					{
						DataType		dt				= md.GetDataType( wm.SequenceId, wt.Type );
						
						if( dt == null )				continue;

						dt.DoColorSet( wt.Color );
						dt.Display		= wt.GetDisplay();

						dt.ScaleMax						= wt.ScaleMax;
						dt.ScaleMin						= wt.ScaleMin;
						if( double.IsNaN( wt.ScaleMax ) == false && double.IsNaN( wt.ScaleMin ) == false )
							dt.Scale						= true;
						
						if( pnl.IsContainingDataType( dt ) == false )
						{
							pnl.DoDataTypeAdd( dt );							
						}
					}
										
					pnl.LaneHeight					= wl.Height;
					
					lst.Add( pnl );
				}

				dm.PanelActive.DoLaneRemoveAll();

				foreach( PnlMapLane pnl in lst )
				{
					dm.PanelActive.DoLaneAdd( pnl );
				}

				dm.PanelActive.Zoom				= wm.Zoom;
				dm.PanelActive.SetPosition( wm.Position );
			}
		}

		protected void OnDrop( object obj, DragEventArgs ea )
		{
			string[]		strFileA		= ( string[] ) ea.Data.GetData( DataFormats.FileDrop, false );

			DoDrop( strFileA );

			ea.Handled						= true;
		}

		private void DoTitleSet( string strFile )
		{
			string			strFileName		= UtilityFile.GetFileName( strFile );
			string			strVersion		= System.Reflection.Assembly.GetExecutingAssembly()
													.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()
													?.InformationalVersion ?? "1.0.0";

			string			strTitle		= string.Format( "MetaScope {0} {1}",
												strVersion,
												strFile == null ? "" : string.Format( " - {0}", strFileName ) );

			Title							= strTitle;
		}

		public void DoWorkspaceOpen( string strFile )
		{
			DoDocumentCloseAll();

			m_mgrWorkspace					= ManagerWorkspace.MakeFromFile( strFile );

			string			strLayout		= m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_FILE ];
			if( strLayout != null )
			{
				DoLayoutRestore( strLayout );
			}

			string[]		strFileA			= m_mgrWorkspace.GetFileArray();
			if( strFileA != null )
			{
				DoDrop( strFileA, true );
				m_mgrWorkspace.IsEdited			= false;
			}

			string			strShowIntro	= m_mgrWorkspace[ ManagerWorkspace.STR_STARTUP_SHOWINTRODUCTION ];
			if( strShowIntro.ToLower() == "true" )
			{
				if( m_dckmVugmap.MainDocumentPane.Items.Contains( m_docIntroduction ) == false )
					m_dckmVugmap.MainDocumentPane.Items.Add( m_docIntroduction );
			}
			else
			{
				if( m_dckmVugmap.MainDocumentPane.Items.Contains( m_docIntroduction ) == true )
					m_dckmVugmap.MainDocumentPane.Items.Remove( m_docIntroduction );
			}	

			string			strAppScaleX	= m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_APPLICAIONSCALEX ];
			if( strAppScaleX != null )
			{
				double			d			= double.Parse( strAppScaleX );
				ScaleTransform	st			= m_dpVugmap.LayoutTransform as ScaleTransform;
				st.ScaleX					= d;
			}

			string			strAppScaleY	= m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_APPLICAIONSCALEY ];
			if( strAppScaleY != null )
			{
				double			d			= double.Parse( strAppScaleY );
				ScaleTransform	st			= m_dpVugmap.LayoutTransform as ScaleTransform;
				st.ScaleY					= d;
			}

			if( m_mgrWorkspace.Bookmark != null )
			{
				ManagerBookmark.GetManager().ListBookmark.Clear();
				ManagerBookmark.GetManager().ListBookmark.AddRange( m_mgrWorkspace.Bookmark );				
			}			
	
			m_mgrWorkspace.IsEdited			= false;

			DoTitleSet( strFile );

			string			strFileName		= System.IO.Path.GetFileName( strFile );
			if( strFileName != null && !strFileName.StartsWith( Constant.S_TEMP_PREFIX ) )
			{
				AppSetting.DoRecentWorkspaceAdd( strFile );
				DoMruMenuUpdate();
			}
		}

		public void DoWorkspaceSave( string strFile )
		{
			m_mgrWorkspace.File				= strFile;

			DoWorkspaceSave();
		}

		public void DoWorkspaceSave()
		{
			ManagerData		md				= ManagerData.GetManager();

			m_mgrWorkspace.DoSave();

			UtilityMessage.ShowMessageFileSaveWorkspace( m_mgrWorkspace.FileName );

			DoTitleSet( m_mgrWorkspace.FileName );
		}

		private void OnIntroductionLoaded(object sender, RoutedEventArgs e)
		{
			string			strVer			= Assembly.GetExecutingAssembly()
													.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
													?.InformationalVersion ?? "1.0.0";
			m_runVersion.Text				= string.Format( "Version {0}", strVer );

			m_docIntroduction.SetAsActive();
			
			if( m_mgrWorkspace != null )
			{
				// Already initialized
				return;
			}

			// Workspace	
			string			strPath			= System.IO.Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            string			strFileAbs		= string.Format( "{0}\\{1}", strPath, Constant.S_APP_SETTING );

			if( UtilityFile.GetFileExist( strFileAbs ) == true )
			{
				DoWorkspaceOpen( strFileAbs );
			}
			else
			{
				string[]	strTempFiles	= System.IO.Directory.GetFiles( AppSetting.AppDataDir, Constant.S_TEMP_PREFIX + "*.workspace" );

				if( strTempFiles.Length > 0 )
				{
					System.Array.Sort( strTempFiles );
					string		strTempFile		= strTempFiles[ strTempFiles.Length - 1 ];
					string		strTempName		= System.IO.Path.GetFileName( strTempFile );

					MessageBoxResult	mbr		= MessageBox.Show(
						string.Format( "A previous session was found: '{0}'.\nWould you like to restore it?", strTempName ),
						"Restore Session", MessageBoxButton.YesNo, MessageBoxImage.Question );

					if( mbr == MessageBoxResult.Yes )
					{
						DoWorkspaceOpen( strTempFile );

						MessageBoxResult	mbrSave		= MessageBox.Show(
							"Session restored. Would you like to save it as a workspace now?",
							"Save Workspace", MessageBoxButton.YesNo, MessageBoxImage.Question );

						if( mbrSave == MessageBoxResult.Yes )
						{
							DoCommandFileSaveWorkspaceAs();
						}
					}
					else
					{
						foreach( string strTemp in strTempFiles )
						{
							try { System.IO.File.Delete( strTemp ); } catch {}
						}
						m_mgrWorkspace				= new ManagerWorkspace();
					}
				}
				else
				{
					m_mgrWorkspace				= new ManagerWorkspace();
				}
			}
		}

		public ManagerWorkspace GetManagerWorkspace()
		{
			return m_mgrWorkspace;
		}

		private void OnBookmarkItemDoubleClick( object obj, MouseButtonEventArgs ea )
		{
			ListViewItem	lvi0			= obj as ListViewItem;
			string			strBookmark		= lvi0.Content as string;
			string[]		strBookmarkA	= strBookmark.Split( new char[] { ',', '(', ')' } );
			string			strSeqId		= strBookmarkA[ 0 ];
			string			strPosition		= strBookmarkA[ 1 ];					
			string			strZoom			= strBookmarkA[ 3 ];
			int				nPosition		= int.Parse( strPosition );
			double			dZoom			= double.Parse( strZoom );
										
			DocMap			doc				= DoDocumentShow( strSeqId );
					
			doc.DoPanelPositionTo( nPosition );
			doc.DoPanelZoomTo( dZoom );
		}

		private void OnSearchItemDoubleClick( object obj, MouseButtonEventArgs ea )
		{
			ListViewItem	lvi				= obj as ListViewItem;
			
			//string			str				= string.Format( "{0}, {1:N0}-{2:N0}, {3}", strSequenceId, df.Start, df.End, df.Attribute );			
			string			str				= lvi.Content as string;
			string[]		strA			= str.Split( '\t' );
			string			strSeqId		= strA[ 0 ].Trim();
			string			strStart		= strA[ 1 ].Split( '-' )[ 0 ].Trim();
			string			strEnd			= strA[ 1 ].Split( '-' )[ 1 ].Trim();
			int				nStart			= int.Parse( strStart, NumberStyles.Number );
			int				nEnd			= int.Parse( strEnd, NumberStyles.Number );
						
			DocMap			dm				= DoDocumentFind( strSeqId );
			if( dm == null )
			{
				ErrorMessage.ShowErrorSearchNoDocumentOpen( strSeqId );
				return;
			}
						
			if( dm.PanelActive.Zoom == 1 )
			{
				dm.DoPanelZoomSet( 64 );
				DoDocumentMove( strSeqId, nStart, nEnd );
				DoDocumentSelection( strSeqId, nStart, nEnd );
			}
			else
			{
				DoDocumentMove( strSeqId, nStart, nEnd );
				DoDocumentSelection( strSeqId, nStart, nEnd );
			}			
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			DoTitleSet( null );
		}

		//
		//	MRU (Most Recently Used) menu
		//

		public void DoMruMenuUpdate()
		{
			MenuItem		mniFile			= null;

			Menu			menu			= null;
			foreach( object child in m_dpVugmap.Children )
			{
				if( child is Menu )
				{
					menu				= child as Menu;
					break;
				}
			}
			if( menu == null )
				return;

			mniFile							= menu.Items[ 0 ] as MenuItem;
			if( mniFile == null )
				return;

			foreach( MenuItem mni in m_lstMruMenuItem )
			{
				mniFile.Items.Remove( mni );
			}
			m_lstMruMenuItem.Clear();

			List< string >	lstWorkspace	= AppSetting.RecentWorkspaceList;
			List< string >	lstGff			= AppSetting.RecentGffList;

			// Workspace section
			if( lstWorkspace.Count == 0 )
			{
				m_sepMruWorkspace.Visibility	= Visibility.Collapsed;
			}
			else
			{
				m_sepMruWorkspace.Visibility	= Visibility.Visible;

				int			nIndex			= mniFile.Items.IndexOf( m_sepMruWorkspace ) + 1;

				for( int i = 0; i < lstWorkspace.Count; i++ )
				{
					string		strPath		= lstWorkspace[ i ];
					string		strName		= System.IO.Path.GetFileName( strPath );

					MenuItem	mni			= new MenuItem();
					mni.Header				= string.Format( "_{0}  {1}", i + 1, strName );
					mni.ToolTip				= strPath;
					mni.Tag					= strPath;
					mni.Click				+= OnMruItemClick;

					mniFile.Items.Insert( nIndex + i, mni );
					m_lstMruMenuItem.Add( mni );
				}
			}

			// GFF section
			if( lstGff.Count == 0 )
			{
				m_sepMruGff.Visibility		= Visibility.Collapsed;
			}
			else
			{
				m_sepMruGff.Visibility		= Visibility.Visible;

				int			nIndex			= mniFile.Items.IndexOf( m_sepMruGff ) + 1;

				for( int i = 0; i < lstGff.Count; i++ )
				{
					string		strPath		= lstGff[ i ];
					string		strName		= System.IO.Path.GetFileName( strPath );

					MenuItem	mni			= new MenuItem();
					mni.Header				= string.Format( "_{0}  {1}", i + 1, strName );
					mni.ToolTip				= strPath;
					mni.Tag					= strPath;
					mni.Click				+= OnMruItemClick;

					mniFile.Items.Insert( nIndex + i, mni );
					m_lstMruMenuItem.Add( mni );
				}
			}
		}

		private void OnMruItemClick( object obj, RoutedEventArgs ea )
		{
			MenuItem		mni				= obj as MenuItem;
			string			strFile			= mni.Tag as string;

			if( File.Exists( strFile ) == false )
			{
				MessageBox.Show( string.Format( "File not found:\n{0}", strFile ), "Open File", MessageBoxButton.OK, MessageBoxImage.Warning );
				return;
			}

			DoDrop( new string[] { strFile } );
		}

		//
		//	Status Bar
		//

		public void DoStatusBarUpdate()
		{
			DocMap			doc				= m_dckmVugmap.ActiveDocument as DocMap;

			if( doc == null )
			{
				m_tbStatusPosition.Text			= "Ready";
				m_tbStatusZoom.Text				= "";
				m_tbStatusFeatureCount.Text		= "";
				m_tbStatusFileName.Text			= "";
				return;
			}

			PnlMap			pnl				= doc.PanelActive;
			ManagerData		md				= ManagerData.GetManager();

			m_tbStatusPosition.Text				= string.Format( "Position: {0:N0} bp", pnl.Position );
			m_tbStatusZoom.Text					= string.Format( "Zoom: {0}x", pnl.Zoom );
			m_tbStatusFeatureCount.Text			= string.Format( "Features: {0:N0}", md.CachedFeatureCount );
			m_tbStatusFileName.Text				= doc.SequenceId != null ? doc.SequenceId : "";
		}

		private void OnDockingManagerPropertyChanged( object obj, System.ComponentModel.PropertyChangedEventArgs ea )
		{
			if( ea.PropertyName == "ActiveDocument" )
			{
				DoStatusBarUpdate();
			}
		}

		private void OnIntroductionDragOver( object obj, DragEventArgs ea )
		{
			ea.Effects		= DragDropEffects.All;							
			ea.Handled		= true;
		}

		private void OnClosing( object obj, System.ComponentModel.CancelEventArgs ea )
		{
			bool			bIsTemp			= m_mgrWorkspace.File != null
											&& System.IO.Path.GetFileName( m_mgrWorkspace.File ).StartsWith( Constant.S_TEMP_PREFIX );

			if( bIsTemp )
			{
				MessageBoxResult	mbr		= MessageBox.Show(
					"This is a temporary workspace.\nWould you like to save it before closing?",
					"Save Workspace", MessageBoxButton.YesNoCancel, MessageBoxImage.Question );

				if( mbr == MessageBoxResult.Yes )
				{
					string		strTempFile		= m_mgrWorkspace.File;

					DoCommandFileSaveWorkspaceAs();

					if( System.IO.Path.GetFileName( m_mgrWorkspace.File ).StartsWith( Constant.S_TEMP_PREFIX ) )
					{
						ea.Cancel		= true;
						return;
					}

					try { System.IO.File.Delete( strTempFile ); } catch {}
				}
				else if( mbr == MessageBoxResult.No )
				{
					try { System.IO.File.Delete( m_mgrWorkspace.File ); } catch {}
				}
				else
				{
					ea.Cancel		= true;
					return;
				}
			}
			else if( m_mgrWorkspace.IsEdited == true )
			{
				string			strMsg;

				if( System.IO.File.Exists( m_mgrWorkspace.File ) )
					strMsg		= string.Format( "The workspace \"{0}\" has been modified.\nDo you want to save before closing?", m_mgrWorkspace.FileName );
				else
					strMsg		= "No workspace has been saved yet.\nWould you like to save the workspace?";

				MessageBoxResult	mbr			= MessageBox.Show( strMsg, "Save Workspace", MessageBoxButton.YesNoCancel, MessageBoxImage.Question );

				if( mbr == MessageBoxResult.Yes )
				{
					DoCommandFileSaveWorkspaceAs();
				}
				else if( mbr == MessageBoxResult.Cancel )
				{
					ea.Cancel		= true;
					return;
				}
			}

			DoDocumentCloseAll();
		}


		private void OnHyperlinkRequestNavigate( object sender, System.Windows.Navigation.RequestNavigateEventArgs ea )
		{
			System.Diagnostics.Process.Start( ea.Uri.AbsoluteUri );
			ea.Handled			= true;
		}

		private void OnIntroductionNavigating( object obj, NavigatingCancelEventArgs ea )
		{
			if( ea.Uri.IsFile == true )
			{
				ea.Cancel		= true;
			}
			else
			{
				ea.Cancel		= false;
			}
		}

		private void OnUpdateLoaded( object obj, RoutedEventArgs ea )
		{			
		}

		private void OnDocumentationLoaded( object obj, RoutedEventArgs ea )
		{			
		}
	}
}

