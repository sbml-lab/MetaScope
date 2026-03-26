using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;

using MetaScope.Controls;
using MetaScope.Models;
using MetaScope.Services;
using MetaScope.Services.Command;
using MetaScope.Services.Error;
using MetaScope.Views;

namespace MetaScope;

public partial class MainWindow : Window
{
	// ================================================================
	// Singleton
	// ================================================================
	private		static List<MainWindow>			s_lstWindows					= new List<MainWindow>();

	// ================================================================
	// Instance fields
	// ================================================================
	private		ManagerWorkspace				m_mgrWorkspace					= null;
	private		string						m_strWorkspaceRealFile			= null;
	private		List<DocMap>					m_lstMap						= new List<DocMap>();
	private		double							m_dDocumentScaleX				= 1.0;
	private		double							m_dDocumentScaleY				= 1.0;
	private		bool							m_bSelectByPosition				= true;
	private		DispatcherTimer					m_tmrAutoSave					= null;
	private		DispatcherTimer					m_tmrWorkspaceSave				= null;
	private		bool							m_bAutoSave						= false;
	private		NativeMenuItem					m_mniAutoSave					= null;
	private		NativeMenuItem					m_mniSelectByPosition			= null;
	private		NativeMenu						m_nmnFile						= null;
	private		int								m_nMruInsertIndex				= 0;
	private		List<NativeMenuItemBase>		m_lstMruItems					= new List<NativeMenuItemBase>();
	private		TabItem							m_tabDragItem					= null;
	private		Point							m_ptDragStart;
	private		bool							m_bTabDragging					= false;

	// ================================================================
	// ICommand properties — bound from AXAML via {Binding Cmd*}
	// ================================================================

	// File
	public		ICommand		CmdFileNewDocument			{ get; private set; }
	public		ICommand		CmdFileOpen					{ get; private set; }
	public		ICommand		CmdFileOpenWorkspace		{ get; private set; }
	public		ICommand		CmdFileOpenLayout			{ get; private set; }
	public		ICommand		CmdFileSaveWorkspace		{ get; private set; }
	public		ICommand		CmdFileSaveWorkspaceAs		{ get; private set; }
	public		ICommand		CmdFileSaveLayoutAs			{ get; private set; }
	public		ICommand		CmdFileSaveAll				{ get; private set; }
	public		ICommand		CmdFileExportImage			{ get; private set; }
	public		ICommand		CmdFileCloseAll				{ get; private set; }
	public		ICommand		CmdFileExit					{ get; private set; }

	// Data — Feature
	public		ICommand		CmdDataSearch				{ get; private set; }
	public		ICommand		CmdDataFeatureUnite			{ get; private set; }
	public		ICommand		CmdDataFeatureMerge			{ get; private set; }
	public		ICommand		CmdDataFeatureMove			{ get; private set; }
	public		ICommand		CmdDataFeatureCopy			{ get; private set; }
	public		ICommand		CmdDataFeatureDelete		{ get; private set; }
	public		ICommand		CmdDataFeatureUndo			{ get; private set; }
	public		ICommand		CmdDataSelectByPosition		{ get; private set; }

	// Data — Track
	public		ICommand		CmdDataTrackSetColor		{ get; private set; }
	public		ICommand		CmdDataTrackSetHeight		{ get; private set; }
	public		ICommand		CmdDataTrackDisplayBar		{ get; private set; }
	public		ICommand		CmdDataTrackDisplayPoint	{ get; private set; }
	public		ICommand		CmdDataTrackDisplayLine		{ get; private set; }
	public		ICommand		CmdDataTrackMoveUp			{ get; private set; }
	public		ICommand		CmdDataTrackMoveDown		{ get; private set; }
	public		ICommand		CmdDataTrackGroup			{ get; private set; }
	public		ICommand		CmdDataTrackUngroup			{ get; private set; }
	public		ICommand		CmdDataTrackSelectToEdit	{ get; private set; }
	public		ICommand		CmdDataTrackSelectAllFeatures { get; private set; }
	public		ICommand		CmdDataTrackManualScale		{ get; private set; }
	public		ICommand		CmdDataTrackChangeType		{ get; private set; }
	public		ICommand		CmdDataTrackHideLane		{ get; private set; }
	public		ICommand		CmdDataTrackCloseFile		{ get; private set; }
	public		ICommand		CmdDataTrackOpAverage		{ get; private set; }
	public		ICommand		CmdDataTrackOpDifference	{ get; private set; }
	public		ICommand		CmdDataTrackOpSummation		{ get; private set; }
	public		ICommand		CmdDataTrackOpMerge			{ get; private set; }
	public		ICommand		CmdDataTrackOpFilter		{ get; private set; }

	// Data — Integration
	public		ICommand		CmdDataIntegrationPorf		{ get; private set; }
	public		ICommand		CmdDataIntegrationRts		{ get; private set; }
	public		ICommand		CmdDataIntegrationTu		{ get; private set; }
	public		ICommand		CmdDataIntegrationTrn		{ get; private set; }

	// View
	public		ICommand		CmdViewZoomIn				{ get; private set; }
	public		ICommand		CmdViewZoomOut				{ get; private set; }
	public		ICommand		CmdViewZoomTo				{ get; private set; }
	public		ICommand		CmdViewScrollLeft			{ get; private set; }
	public		ICommand		CmdViewScrollRight			{ get; private set; }
	public		ICommand		CmdViewPositionTo			{ get; private set; }
	public		ICommand		CmdViewSplit				{ get; private set; }
	public		ICommand		CmdViewFeatureOpacity		{ get; private set; }
	public		ICommand		CmdViewScrollLeftSmall		{ get; private set; }
	public		ICommand		CmdViewScrollRightSmall		{ get; private set; }
	public		ICommand		CmdViewGoHome				{ get; private set; }
	public		ICommand		CmdViewGoEnd				{ get; private set; }
	public		ICommand		CmdViewTabNext				{ get; private set; }
	public		ICommand		CmdViewTabPrev				{ get; private set; }
	public		ICommand		CmdViewRefresh				{ get; private set; }
	public		ICommand		CmdViewScaleUp				{ get; private set; }
	public		ICommand		CmdViewScaleDown			{ get; private set; }

	// Feature Adjust
	public		ICommand		CmdFeatureMoveLeft			{ get; private set; }
	public		ICommand		CmdFeatureMoveRight			{ get; private set; }
	public		ICommand		CmdFeatureShrinkStart		{ get; private set; }
	public		ICommand		CmdFeatureExpandEnd			{ get; private set; }

	// Window
	public		ICommand		CmdWindowIntroduction		{ get; private set; }
	public		ICommand		CmdWindowFileExplorer		{ get; private set; }
	public		ICommand		CmdWindowSetting			{ get; private set; }
	public		ICommand		CmdWindowEdit				{ get; private set; }
	public		ICommand		CmdWindowBookmark			{ get; private set; }
	public		ICommand		CmdWindowFeature			{ get; private set; }
	public		ICommand		CmdWindowFeatureSelected	{ get; private set; }
	public		ICommand		CmdWindowLog				{ get; private set; }
	public		ICommand		CmdWindowSearch				{ get; private set; }

	// Help
	public		ICommand		CmdHelpAbout				{ get; private set; }
	public		ICommand		CmdHelpDocumentation		{ get; private set; }
	public		ICommand		CmdHelpShortcuts			{ get; private set; }
	public		ICommand		CmdHelpAutoSave				{ get; private set; }
	public		ICommand		CmdHelpUpdate				{ get; private set; }
	public		ICommand		CmdHelpGarbageCollection	{ get; private set; }

	// ================================================================
	// Constructor
	// ================================================================
	public MainWindow()
	{
		InitializeComponent();

		s_lstWindows.Add( this );
		DataContext			= this;

		DoCommandInitialize();
		DoMenuInitialize();
		DoKeyBindingsInitialize();
		DoEventSubscribe();
		DoTimerInitialize();

		Opened				+= OnLoaded;
		Closing				+= OnClosing;

		AddHandler( DragDrop.DropEvent, OnDrop );
	}

	// ================================================================
	// Window lookup
	// ================================================================

	/// <summary>
	/// Returns the MainWindow that owns the given visual, or the most recently
	/// focused window as fallback.  Call sites inside controls/dialogs should
	/// prefer the overload that takes a Visual so each control reaches its own
	/// parent window in a multi-window setup.
	/// </summary>
	public static MainWindow GetMainWindow()
	{
		// Fallback: return the currently active window, or the first open one
		foreach( var w in s_lstWindows )
			if( w.IsActive ) return w;
		return s_lstWindows.Count > 0 ? s_lstWindows[0] : null;
	}

	/// <summary>
	/// Finds the MainWindow ancestor of a given visual (control, dialog, etc.).
	/// </summary>
	public static MainWindow GetMainWindow( Avalonia.Visual visual )
	{
		var top = TopLevel.GetTopLevel( visual );
		if( top is MainWindow mw ) return mw;
		// Dialog windows: walk Owner chain
		if( top is Window w && w.Owner is MainWindow owner ) return owner;
		return GetMainWindow();
	}

	public List<DocMap> ListDocument
	{
		get { return m_lstMap; }
	}

	// ================================================================
	// Command initialization
	// ================================================================
	// CanExecute helpers — match WPF reference guard patterns
	private bool CanDoc()
	{
		return GetActiveDocument() != null;
	}

	private bool CanLane()
	{
		var map = GetActiveMap();
		return map != null && map.LaneSelected != null && map.LaneSelected.Count > 0;
	}

	private bool CanEditableLaneWithSelection()
	{
		var doc = GetActiveDocument();
		if( doc == null ) return false;
		var pm = doc.PanelActive;
		if( pm == null ) return false;
		foreach( var pnl in pm.ListLaneEditable )
			if( pnl.GetCountFeatureSelected() > 0 ) return true;
		return false;
	}

	private bool CanUndo()
	{
		return ManagerEdit.GetManager().GetCount() > 0;
	}

	private void DoCommandInitialize()
	{
		// File
		CmdFileNewDocument			= new RelayCommand( OnCommandFileNewDocument );
		CmdFileOpen					= new RelayCommand( OnCommandFileOpen );
		CmdFileOpenWorkspace		= new RelayCommand( OnCommandFileOpenWorkspace );
		CmdFileOpenLayout			= new RelayCommand( OnCommandFileOpenLayout );
		CmdFileSaveWorkspace		= new RelayCommand( OnCommandFileSaveWorkspace );
		CmdFileSaveWorkspaceAs		= new RelayCommand( OnCommandFileSaveWorkspaceAs );
		CmdFileSaveLayoutAs			= new RelayCommand( OnCommandFileSaveLayoutAs );
		CmdFileSaveAll				= new RelayCommand( OnCommandFileSaveAll, CanDoc );
		CmdFileExportImage			= new RelayCommand( OnCommandFileExportImage, CanDoc );
		CmdFileCloseAll				= new RelayCommand( OnCommandFileCloseAll, CanDoc );
		CmdFileExit					= new RelayCommand( OnCommandFileExit );

		// Data — Feature
		CmdDataSearch				= new RelayCommand( OnCommandDataSearch, CanDoc );
		CmdDataFeatureUnite			= new RelayCommand( OnCommandDataFeatureUnite, CanEditableLaneWithSelection );
		CmdDataFeatureMerge			= new RelayCommand( OnCommandDataFeatureMerge, CanEditableLaneWithSelection );
		CmdDataFeatureMove			= new RelayCommand( OnCommandDataFeatureMove, CanEditableLaneWithSelection );
		CmdDataFeatureCopy			= new RelayCommand( OnCommandDataFeatureCopy, CanEditableLaneWithSelection );
		CmdDataFeatureDelete		= new RelayCommand( OnCommandDataFeatureDelete, CanEditableLaneWithSelection );
		CmdDataFeatureUndo			= new RelayCommand( OnCommandDataFeatureUndo );
		CmdDataSelectByPosition		= new RelayCommand( OnCommandDataSelectByPosition );

		// Data — Track
		CmdDataTrackSetColor		= new RelayCommand( OnCommandDataTrackSetColor, CanLane );
		CmdDataTrackSetHeight		= new RelayCommand( OnCommandDataTrackSetHeight, CanLane );
		CmdDataTrackDisplayBar		= new RelayCommand( () => OnCommandDataTrackDisplay( EDataTypeDisplay.BAR ), CanLane );
		CmdDataTrackDisplayPoint	= new RelayCommand( () => OnCommandDataTrackDisplay( EDataTypeDisplay.POINT ), CanLane );
		CmdDataTrackDisplayLine		= new RelayCommand( () => OnCommandDataTrackDisplay( EDataTypeDisplay.LINE ), CanLane );
		CmdDataTrackMoveUp			= new RelayCommand( OnCommandDataTrackMoveUp, CanLane );
		CmdDataTrackMoveDown		= new RelayCommand( OnCommandDataTrackMoveDown, CanLane );
		CmdDataTrackGroup			= new RelayCommand( OnCommandDataTrackGroup, CanLane );
		CmdDataTrackUngroup			= new RelayCommand( OnCommandDataTrackUngroup, CanLane );
		CmdDataTrackSelectToEdit	= new RelayCommand( OnCommandDataTrackSelectToEdit, CanLane );
		CmdDataTrackSelectAllFeatures = new RelayCommand( OnCommandDataTrackSelectAllFeatures, CanLane );
		CmdDataTrackManualScale		= new RelayCommand( OnCommandDataTrackManualScale, CanLane );
		CmdDataTrackChangeType		= new RelayCommand( OnCommandDataTrackChangeType, CanLane );
		CmdDataTrackHideLane		= new RelayCommand( OnCommandDataTrackHideLane, CanLane );
		CmdDataTrackCloseFile		= new RelayCommand( OnCommandDataTrackCloseFile, CanLane );
		CmdDataTrackOpAverage		= new RelayCommand( OnCommandDataTrackOpAverage, CanLane );
		CmdDataTrackOpDifference	= new RelayCommand( OnCommandDataTrackOpDifference, CanLane );
		CmdDataTrackOpSummation		= new RelayCommand( OnCommandDataTrackOpSummation, CanLane );
		CmdDataTrackOpMerge			= new RelayCommand( OnCommandDataTrackOpMerge, CanLane );
		CmdDataTrackOpFilter		= new RelayCommand( OnCommandDataTrackOpFilter, CanLane );

		// Data — Integration
		CmdDataIntegrationPorf		= new RelayCommand( OnCommandDataIntegrationPorf );
		CmdDataIntegrationRts		= new RelayCommand( OnCommandDataIntegrationRts );
		CmdDataIntegrationTu		= new RelayCommand( OnCommandDataIntegrationTu );
		CmdDataIntegrationTrn		= new RelayCommand( OnCommandDataIntegrationTrn );

		// View
		CmdViewZoomIn				= new RelayCommand( OnCommandViewZoomIn, CanDoc );
		CmdViewZoomOut				= new RelayCommand( OnCommandViewZoomOut, CanDoc );
		CmdViewZoomTo				= new RelayCommand( OnCommandViewZoomTo, CanDoc );
		CmdViewScrollLeft			= new RelayCommand( OnCommandViewScrollLeft, CanDoc );
		CmdViewScrollRight			= new RelayCommand( OnCommandViewScrollRight, CanDoc );
		CmdViewPositionTo			= new RelayCommand( OnCommandViewPositionTo, CanDoc );
		CmdViewSplit				= new RelayCommand( OnCommandViewSplit, CanDoc );
		CmdViewFeatureOpacity		= new RelayCommand( OnCommandViewFeatureOpacity, CanDoc );
		CmdViewScrollLeftSmall		= new RelayCommand( OnCommandViewScrollLeftSmall, CanDoc );
		CmdViewScrollRightSmall		= new RelayCommand( OnCommandViewScrollRightSmall, CanDoc );
		CmdViewGoHome				= new RelayCommand( OnCommandViewGoHome, CanDoc );
		CmdViewGoEnd				= new RelayCommand( OnCommandViewGoEnd, CanDoc );
		CmdViewTabNext				= new RelayCommand( OnCommandViewTabNext );
		CmdViewTabPrev				= new RelayCommand( OnCommandViewTabPrev );
		CmdViewRefresh				= new RelayCommand( OnCommandViewRefresh, CanDoc );
		CmdViewScaleUp				= new RelayCommand( OnCommandViewScaleUp, CanDoc );
		CmdViewScaleDown			= new RelayCommand( OnCommandViewScaleDown, CanDoc );

		// Feature Adjust
		CmdFeatureMoveLeft			= new RelayCommand( OnCommandFeatureMoveLeft, CanDoc );
		CmdFeatureMoveRight			= new RelayCommand( OnCommandFeatureMoveRight, CanDoc );
		CmdFeatureShrinkStart		= new RelayCommand( OnCommandFeatureShrinkStart, CanDoc );
		CmdFeatureExpandEnd			= new RelayCommand( OnCommandFeatureExpandEnd, CanDoc );

		// Window
		CmdWindowIntroduction		= new RelayCommand( OnCommandWindowIntroduction );
		CmdWindowFileExplorer		= new RelayCommand( OnCommandWindowFileExplorer );
		CmdWindowSetting			= new RelayCommand( OnCommandWindowSetting );
		CmdWindowEdit				= new RelayCommand( OnCommandWindowEdit );
		CmdWindowBookmark			= new RelayCommand( OnCommandWindowBookmark );
		CmdWindowFeature			= new RelayCommand( OnCommandWindowFeature );
		CmdWindowFeatureSelected	= new RelayCommand( OnCommandWindowFeatureSelected );
		CmdWindowLog				= new RelayCommand( OnCommandWindowLog );
		CmdWindowSearch				= new RelayCommand( OnCommandWindowSearch );

		// Help
		CmdHelpAbout				= new RelayCommand( OnCommandHelpAbout );
		CmdHelpDocumentation		= new RelayCommand( OnCommandHelpDocumentation );
		CmdHelpShortcuts			= new RelayCommand( OnCommandHelpShortcuts );
		CmdHelpAutoSave				= new RelayCommand( OnCommandHelpAutoSave );
		CmdHelpUpdate				= new RelayCommand( OnCommandHelpUpdate );
		CmdHelpGarbageCollection	= new RelayCommand( OnCommandHelpGarbageCollection );
	}

	// ================================================================
	// Key bindings — registered in code-behind because AXAML KeyBindings
	// don't resolve DataContext bindings in Avalonia
	// ================================================================
	// ================================================================
	// Menu — built in code-behind for reliable command wiring
	// ================================================================
	private		List<(NativeMenuItem Item, ICommand Cmd)>	m_lstMenuCmdBindings = new List<(NativeMenuItem, ICommand)>();

	private void DoMenuCanExecuteUpdate()
	{
		foreach( var (item, cmd) in m_lstMenuCmdBindings )
			item.IsEnabled = cmd.CanExecute( null );
	}

	private void DoMenuInitialize()
	{
		NativeMenuItem NMI( string header, ICommand cmd = null, string gesture = null )
		{
			var nmi = new NativeMenuItem( header );
			if( cmd != null )
			{
				nmi.Click += ( s, e ) => { if( cmd.CanExecute( null ) ) cmd.Execute( null ); };
				m_lstMenuCmdBindings.Add( (nmi, cmd) );
			}
			if( gesture != null )
				nmi.Gesture = KeyGesture.Parse( gesture );
			return nmi;
		}

		var menu = new NativeMenu();

		// File
		var nmiFile = new NativeMenuItem( "File" ) { Menu = new NativeMenu() };
		m_nmnFile = nmiFile.Menu;
		m_nmnFile.Add( NMI( "Open", CmdFileOpen, "Cmd+O" ) );
		m_nmnFile.Add( NMI( "Open Workspace", CmdFileOpenWorkspace, "Cmd+Shift+O" ) );
		m_nmnFile.Add( NMI( "Open Layout", CmdFileOpenLayout ) );
		m_nMruInsertIndex = m_nmnFile.Items.Count;
		m_nmnFile.Add( new NativeMenuItemSeparator() );
		m_nmnFile.Add( NMI( "Save Workspace", CmdFileSaveWorkspace, "Cmd+Shift+S" ) );
		m_nmnFile.Add( NMI( "Save Workspace As", CmdFileSaveWorkspaceAs ) );
		m_nmnFile.Add( NMI( "Save Layout As", CmdFileSaveLayoutAs ) );
		m_nmnFile.Add( NMI( "Save All Data", CmdFileSaveAll, "Cmd+S" ) );
		m_nmnFile.Add( NMI( "Export Image...", CmdFileExportImage, "Cmd+Shift+E" ) );
		m_nmnFile.Add( new NativeMenuItemSeparator() );
		m_nmnFile.Add( NMI( "Close All", CmdFileCloseAll ) );
		m_nmnFile.Add( NMI( "Exit", CmdFileExit ) );
		menu.Add( nmiFile );

		// Data
		var nmiData = new NativeMenuItem( "Data" ) { Menu = new NativeMenu() };
		nmiData.Menu.Add( NMI( "Search", CmdDataSearch, "Cmd+F" ) );
		nmiData.Menu.Add( new NativeMenuItemSeparator() );
		var nmiFeature = new NativeMenuItem( "Feature" ) { Menu = new NativeMenu() };
		nmiFeature.Menu.Add( NMI( "Unite", CmdDataFeatureUnite, "Cmd+U" ) );
		nmiFeature.Menu.Add( NMI( "Merge", CmdDataFeatureMerge, "Cmd+M" ) );
		nmiFeature.Menu.Add( NMI( "Move", CmdDataFeatureMove ) );
		nmiFeature.Menu.Add( NMI( "Copy", CmdDataFeatureCopy ) );
		nmiFeature.Menu.Add( NMI( "Delete", CmdDataFeatureDelete, "Cmd+D" ) );
		nmiFeature.Menu.Add( NMI( "Undo", CmdDataFeatureUndo, "Cmd+Z" ) );
		nmiFeature.Menu.Add( new NativeMenuItemSeparator() );
		m_mniSelectByPosition = NMI( "Select by only position", CmdDataSelectByPosition );
		m_mniSelectByPosition.ToggleType = NativeMenuItemToggleType.CheckBox;
		m_mniSelectByPosition.IsChecked = m_bSelectByPosition;
		nmiFeature.Menu.Add( m_mniSelectByPosition );
		nmiData.Menu.Add( nmiFeature );
		var nmiTrack = new NativeMenuItem( "Track" ) { Menu = new NativeMenu() };
		nmiTrack.Menu.Add( NMI( "Set Color", CmdDataTrackSetColor ) );
		nmiTrack.Menu.Add( NMI( "Set Height", CmdDataTrackSetHeight ) );
		var nmiDisplay = new NativeMenuItem( "Display" ) { Menu = new NativeMenu() };
		nmiDisplay.Menu.Add( NMI( "Bar", CmdDataTrackDisplayBar ) );
		nmiDisplay.Menu.Add( NMI( "Point", CmdDataTrackDisplayPoint ) );
		nmiDisplay.Menu.Add( NMI( "Line", CmdDataTrackDisplayLine ) );
		nmiTrack.Menu.Add( nmiDisplay );
		nmiTrack.Menu.Add( NMI( "Move Up", CmdDataTrackMoveUp ) );
		nmiTrack.Menu.Add( NMI( "Move Down", CmdDataTrackMoveDown ) );
		nmiTrack.Menu.Add( NMI( "Group", CmdDataTrackGroup ) );
		nmiTrack.Menu.Add( NMI( "Ungroup", CmdDataTrackUngroup ) );
		nmiTrack.Menu.Add( new NativeMenuItemSeparator() );
		nmiTrack.Menu.Add( NMI( "Select to Edit", CmdDataTrackSelectToEdit ) );
		nmiTrack.Menu.Add( NMI( "Select All Features", CmdDataTrackSelectAllFeatures ) );
		nmiTrack.Menu.Add( NMI( "Manual Scale", CmdDataTrackManualScale ) );
		nmiTrack.Menu.Add( NMI( "Change Type", CmdDataTrackChangeType ) );
		nmiTrack.Menu.Add( new NativeMenuItemSeparator() );
		nmiTrack.Menu.Add( NMI( "Hide Lane", CmdDataTrackHideLane ) );
		nmiTrack.Menu.Add( NMI( "Close File", CmdDataTrackCloseFile ) );
		nmiTrack.Menu.Add( new NativeMenuItemSeparator() );
		nmiTrack.Menu.Add( NMI( "Average", CmdDataTrackOpAverage ) );
		nmiTrack.Menu.Add( NMI( "Difference", CmdDataTrackOpDifference ) );
		nmiTrack.Menu.Add( NMI( "Summation", CmdDataTrackOpSummation ) );
		nmiTrack.Menu.Add( NMI( "Merge", CmdDataTrackOpMerge ) );
		nmiTrack.Menu.Add( NMI( "Filter", CmdDataTrackOpFilter ) );
		nmiData.Menu.Add( nmiTrack );
		var nmiIntegration = new NativeMenuItem( "Integration" ) { Menu = new NativeMenu() };
		nmiIntegration.Menu.Add( NMI( "pORF from Proteomics and ORF", CmdDataIntegrationPorf ) );
		nmiIntegration.Menu.Add( NMI( "RTS from TD and RBR", CmdDataIntegrationRts ) );
		nmiIntegration.Menu.Add( NMI( "TU from TSS and RTS", CmdDataIntegrationTu ) );
		nmiData.Menu.Add( nmiIntegration );
		menu.Add( nmiData );

		// View
		var nmiView = new NativeMenuItem( "View" ) { Menu = new NativeMenu() };
		nmiView.Menu.Add( NMI( "Zoom In", CmdViewZoomIn, "Cmd+OemPlus" ) );
		nmiView.Menu.Add( NMI( "Zoom Out", CmdViewZoomOut, "Cmd+OemMinus" ) );
		nmiView.Menu.Add( NMI( "Zoom To", CmdViewZoomTo ) );
		nmiView.Menu.Add( NMI( "Scroll Left", CmdViewScrollLeft ) );
		nmiView.Menu.Add( NMI( "Scroll Right", CmdViewScrollRight ) );
		nmiView.Menu.Add( NMI( "Position To", CmdViewPositionTo, "Cmd+G" ) );
		nmiView.Menu.Add( NMI( "Split", CmdViewSplit, "Cmd+T" ) );
		nmiView.Menu.Add( NMI( "Feature Opacity", CmdViewFeatureOpacity ) );
		nmiView.Menu.Add( new NativeMenuItemSeparator() );
		nmiView.Menu.Add( NMI( "Scale Up", CmdViewScaleUp ) );
		nmiView.Menu.Add( NMI( "Scale Down", CmdViewScaleDown ) );
		menu.Add( nmiView );

		// Window
		var nmiWindow = new NativeMenuItem( "Window" ) { Menu = new NativeMenu() };
		var nmiWindows = new NativeMenuItem( "Windows" ) { Menu = new NativeMenu() };
		nmiWindows.Menu.Add( NMI( "Introduction", CmdWindowIntroduction ) );
		nmiWindows.Menu.Add( NMI( "Explorer", CmdWindowFileExplorer ) );
		nmiWindows.Menu.Add( NMI( "Edit", CmdWindowEdit ) );
		nmiWindows.Menu.Add( NMI( "Bookmark", CmdWindowBookmark ) );
		nmiWindows.Menu.Add( NMI( "Feature", CmdWindowFeature ) );
		nmiWindows.Menu.Add( NMI( "Selected Feature", CmdWindowFeatureSelected ) );
		nmiWindows.Menu.Add( NMI( "Search", CmdWindowSearch ) );
		nmiWindow.Menu.Add( nmiWindows );
		menu.Add( nmiWindow );

		// Help
		var nmiHelp = new NativeMenuItem( "Help" ) { Menu = new NativeMenu() };
		nmiHelp.Menu.Add( NMI( "About MetaScope", CmdHelpAbout ) );
		nmiHelp.Menu.Add( NMI( "Documentation", CmdHelpDocumentation ) );
		nmiHelp.Menu.Add( NMI( "Keyboard Shortcuts", CmdHelpShortcuts ) );
		nmiHelp.Menu.Add( new NativeMenuItemSeparator() );
		m_mniAutoSave = NMI( "Auto Save", CmdHelpAutoSave );
		m_mniAutoSave.ToggleType = NativeMenuItemToggleType.CheckBox;
		nmiHelp.Menu.Add( m_mniAutoSave );
		nmiHelp.Menu.Add( new NativeMenuItemSeparator() );
		nmiHelp.Menu.Add( NMI( "Check Updates", CmdHelpUpdate ) );
		nmiHelp.Menu.Add( NMI( "Garbage Collection", CmdHelpGarbageCollection ) );
		menu.Add( nmiHelp );

		menu.NeedsUpdate += ( s, e ) => DoMenuCanExecuteUpdate();

		NativeMenu.SetMenu( this, menu );
		m_mnuMain.IsVisible = false;
	}

	private void DoKeyBindingsInitialize()
	{
		// macOS: use ⌘ (Meta) instead of Ctrl
		KeyModifiers Cmd = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
		KeyModifiers CmdShift = Cmd | KeyModifiers.Shift;

		void Bind( KeyGesture g, ICommand cmd )
		{
			KeyBindings.Add( new KeyBinding { Gesture = g, Command = cmd } );
		}

		// File
		Bind( new KeyGesture( Key.O, Cmd ),					CmdFileOpen );
		Bind( new KeyGesture( Key.O, CmdShift ),				CmdFileOpenWorkspace );
		Bind( new KeyGesture( Key.S, CmdShift ),				CmdFileSaveWorkspace );
		Bind( new KeyGesture( Key.S, Cmd ),					CmdFileSaveAll );
		Bind( new KeyGesture( Key.E, CmdShift ),				CmdFileExportImage );
		Bind( new KeyGesture( Key.W, CmdShift ),				CmdFileCloseAll );
		Bind( new KeyGesture( Key.Q, Cmd ),					CmdFileExit );

		// Data — Feature
		Bind( new KeyGesture( Key.F, Cmd ),					CmdDataSearch );
		Bind( new KeyGesture( Key.U, Cmd ),					CmdDataFeatureUnite );
		Bind( new KeyGesture( Key.M, Cmd ),					CmdDataFeatureMerge );
		Bind( new KeyGesture( Key.V, Cmd ),					CmdDataFeatureMove );
		Bind( new KeyGesture( Key.C, Cmd ),					CmdDataFeatureCopy );
		Bind( new KeyGesture( Key.D, Cmd ),					CmdDataFeatureDelete );
		Bind( new KeyGesture( Key.Z, Cmd ),					CmdDataFeatureUndo );

		// Data — Track
		Bind( new KeyGesture( Key.C, CmdShift ),				CmdDataTrackSetColor );
		Bind( new KeyGesture( Key.H, CmdShift ),				CmdDataTrackSetHeight );
		Bind( new KeyGesture( Key.B, CmdShift ),				CmdDataTrackDisplayBar );
		Bind( new KeyGesture( Key.P, CmdShift ),				CmdDataTrackDisplayPoint );
		Bind( new KeyGesture( Key.L, CmdShift ),				CmdDataTrackDisplayLine );
		Bind( new KeyGesture( Key.Up, CmdShift ),				CmdDataTrackMoveUp );
		Bind( new KeyGesture( Key.Down, CmdShift ),			CmdDataTrackMoveDown );
		Bind( new KeyGesture( Key.G, CmdShift ),				CmdDataTrackGroup );
		Bind( new KeyGesture( Key.U, CmdShift ),				CmdDataTrackUngroup );
		Bind( new KeyGesture( Key.A, CmdShift ),				CmdDataTrackSelectAllFeatures );
		Bind( new KeyGesture( Key.T, CmdShift ),				CmdDataTrackChangeType );
		Bind( new KeyGesture( Key.D, CmdShift ),				CmdDataTrackHideLane );
		Bind( new KeyGesture( Key.X, CmdShift ),				CmdDataTrackCloseFile );

		// View
		Bind( new KeyGesture( Key.OemPlus, Cmd ),				CmdViewZoomIn );
		Bind( new KeyGesture( Key.OemMinus, Cmd ),				CmdViewZoomOut );
		Bind( new KeyGesture( Key.D0, Cmd ),					CmdViewZoomTo );
		Bind( new KeyGesture( Key.Left, KeyModifiers.Shift ),	CmdViewScrollLeft );
		Bind( new KeyGesture( Key.Right, KeyModifiers.Shift ),	CmdViewScrollRight );
		Bind( new KeyGesture( Key.G, Cmd ),					CmdViewPositionTo );
		Bind( new KeyGesture( Key.T, Cmd ),					CmdViewSplit );
		Bind( new KeyGesture( Key.Left, KeyModifiers.None ),	CmdViewScrollLeftSmall );
		Bind( new KeyGesture( Key.Right, KeyModifiers.None ),	CmdViewScrollRightSmall );
		Bind( new KeyGesture( Key.Home, KeyModifiers.None ),	CmdViewGoHome );
		Bind( new KeyGesture( Key.End, KeyModifiers.None ),	CmdViewGoEnd );
		Bind( new KeyGesture( Key.Tab, Cmd ),					CmdViewTabNext );
		Bind( new KeyGesture( Key.Tab, CmdShift ),				CmdViewTabPrev );
		Bind( new KeyGesture( Key.F5, KeyModifiers.None ),		CmdViewRefresh );
		Bind( new KeyGesture( Key.OemPlus, CmdShift ),			CmdViewScaleUp );
		Bind( new KeyGesture( Key.OemMinus, CmdShift ),		CmdViewScaleDown );

		// Feature Adjust (Option ⌥ on macOS)
		Bind( new KeyGesture( Key.Left, KeyModifiers.Alt ),			CmdFeatureMoveLeft );
		Bind( new KeyGesture( Key.Right, KeyModifiers.Alt ),		CmdFeatureMoveRight );
		Bind( new KeyGesture( Key.Down, KeyModifiers.Alt ),			CmdFeatureShrinkStart );
		Bind( new KeyGesture( Key.Up, KeyModifiers.Alt ),			CmdFeatureExpandEnd );

		// Help
		Bind( new KeyGesture( Key.H, Cmd ),					CmdHelpDocumentation );
	}

	// ================================================================
	// Event subscriptions — connect to decoupled service events
	// ================================================================
	private void DoEventSubscribe()
	{
		ErrorMessage.OnError			+= strMsg => Dispatcher.UIThread.Post( () => DoShowError( strMsg ) );
		UtilityMessage.OnMessage		+= strMsg => Dispatcher.UIThread.Post( () => DoShowMessage( strMsg ) );

		ManagerEdit.OnEditUpdated		+= () => Dispatcher.UIThread.Post( DoEditUpdate );
		ManagerBookmark.OnBookmarkUpdated += () => Dispatcher.UIThread.Post( DoBookmarkUpdate );

		ManagerData.ConfirmSaveFunc		= DoConfirmSave;
		ManagerData.SaveFileDialogFunc	= DoSaveFileDialog;

		PnlMap.OnStatusBarUpdateRequested += () => Dispatcher.UIThread.Post( DoStatusBarUpdate );
		PnlMap.IsSelectByPositionFunc = () => m_bSelectByPosition;

		m_ltvSearch.DoubleTapped		+= OnSearchItemDoubleClick;
		m_ltvBookmark.DoubleTapped		+= OnBookmarkItemDoubleClick;

		// Workspace save event handlers
		ManagerWorkspace.OnFillSetting	+= DoWorkspaceFillSetting;
		ManagerWorkspace.OnFillFile		+= DoWorkspaceFillFile;
		ManagerWorkspace.OnFillLane		+= DoWorkspaceFillLane;

		// PnlMap fires this on every view update — triggers debounced workspace save
		PnlMap.OnWorkspaceSaveDebounceRequested += DoWorkspaceSaveDebounce;

		// PnlMap fires this after unite/delete — triggers immediate GFF autosave
		PnlMap.OnAutoSaveImmediateRequested += DoAutoSaveImmediate;
	}

	// ================================================================
	// Workspace fill handlers — populate workspace data during save
	// ================================================================
	private void DoWorkspaceFillSetting( ManagerWorkspace mw )
	{
		// Only respond if this workspace belongs to this window
		if( mw != m_mgrWorkspace )		return;

		var ltc = m_dpMain.Children[0] as LayoutTransformControl;
		var st = ltc?.LayoutTransform as ScaleTransform;

		mw.LayoutApplicationScaleX		= st?.ScaleX ?? 1.0;
		mw.LayoutApplicationScaleY		= st?.ScaleY ?? 1.0;
		mw.LayoutDocumentScaleX			= m_dDocumentScaleX;
		mw.LayoutDocumentScaleY			= m_dDocumentScaleY;

		// Check if introduction tab is visible
		bool bIntroVisible = false;
		foreach( TabItem tab in m_tabDocuments.Items )
		{
			if( tab.Header is string strH && strH == "Introduction" )	{ bIntroVisible = true; break; }
			if( tab.Header is Grid grd )
			{
				foreach( var child in grd.Children )
					if( child is TextBlock tb && tb.Text == "Introduction" )	{ bIntroVisible = true; break; }
			}
		}
		mw.SetSetting( "Startup.ShowIntroduction", bIntroVisible ? "true" : "false" );

		// Bookmarks
		mw.Bookmark.Clear();
		var mb = ManagerBookmark.GetManager();
		foreach( var db in mb.ListBookmark )
			mw.Bookmark.Add( db );
	}

	private void DoWorkspaceFillFile( ManagerWorkspace mw )
	{
		if( mw != m_mgrWorkspace )		return;

		mw.Data.Clear();

		var md = ManagerData.GetManager();
		for( int i = 0; i < md.GetCountDataFile(); i++ )
		{
			var df = md.GetDataFile( i );
			string strRelative = mw.GetRelativePath( df.File );

			var wd = new WorkspaceData();
			wd.File = strRelative ?? df.File;
			mw.Data.Add( wd );
		}
	}

	private void DoWorkspaceFillLane( ManagerWorkspace mw )
	{
		if( mw != m_mgrWorkspace )		return;

		mw.Map.Clear();

		foreach( var doc in m_lstMap )
		{
			var pnl = doc.PanelActive;
			if( pnl == null )		continue;

			var wm = new WorkspaceMap();
			wm.SequenceId				= doc.SequenceId;
			wm.Position					= pnl.Position;
			wm.Zoom						= pnl.Zoom;

			for( int i = 0; i < pnl.GetCountLane(); i++ )
			{
				var pnlLane = pnl.GetLane( i );
				var wl = new WorkspaceLane();
				wl.Height					= pnlLane.LaneHeight;

				for( int j = 0; j < pnlLane.GetCountDataType(); j++ )
				{
					var dt = pnlLane.DoDataTypeGet( j );
					var wt = new WorkspaceType( dt.Type, dt.GetColorString() );
					wt.Display				= dt.Display.ToString();
					wt.ScaleMax				= dt.Scale ? dt.ScaleMax : double.NaN;
					wt.ScaleMin				= dt.Scale ? dt.ScaleMin : double.NaN;
					wl.ListType.Add( wt );
				}

				wm.ListLane.Add( wl );
			}

			mw.Map.Add( wm );
		}
	}

	private void OnSearchItemDoubleClick( object obj, Avalonia.Interactivity.RoutedEventArgs ea )
	{
		string str = m_ltvSearch.SelectedItem as string;
		if( string.IsNullOrEmpty( str ) )		return;

		string[] strA = str.Split( '\t' );
		if( strA.Length < 2 )		return;

		string strSeqId = strA[0].Trim();
		string[] strRange = strA[1].Split( '-' );
		if( strRange.Length < 2 )		return;

		if( !int.TryParse( strRange[0].Trim(), System.Globalization.NumberStyles.Number,
			System.Globalization.CultureInfo.InvariantCulture, out int nStart ) )		return;
		if( !int.TryParse( strRange[1].Trim(), System.Globalization.NumberStyles.Number,
			System.Globalization.CultureInfo.InvariantCulture, out int nEnd ) )		return;

		DocMap dm = DoDocumentShow( strSeqId );
		if( dm == null )
		{
			ErrorMessage.ShowErrorSearchNoDocumentOpen( strSeqId );
			return;
		}

		if( dm.PanelActive.Zoom == 1 )
			dm.DoPanelZoomSet( 64 );

		DoDocumentMove( strSeqId, nStart, nEnd );
		DoDocumentSelection( strSeqId, nStart, nEnd );
	}

	private void OnBookmarkItemDoubleClick( object obj, Avalonia.Interactivity.RoutedEventArgs ea )
	{
		string strBookmark = m_ltvBookmark.SelectedItem as string;
		if( string.IsNullOrEmpty( strBookmark ) )		return;

		string[] strBookmarkA = strBookmark.Split( new char[] { ',', '(', ')' } );
		if( strBookmarkA.Length < 4 )		return;

		string strSeqId = strBookmarkA[0].Trim();
		if( !int.TryParse( strBookmarkA[1].Trim(), out int nPosition ) )		return;
		if( !double.TryParse( strBookmarkA[3].Trim(), out double dZoom ) )		return;

		DocMap doc = DoDocumentShow( strSeqId );
		if( doc == null )		return;

		doc.DoPanelPositionTo( nPosition );
		doc.DoPanelZoomTo( dZoom );
	}

	// ================================================================
	// Timer initialization
	// ================================================================
	private void DoTimerInitialize()
	{
		m_tmrAutoSave = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 5 ) };
		m_tmrAutoSave.Tick += ( s, e ) => OnAutoSaveTick();

		m_tmrWorkspaceSave = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 5 ) };
		m_tmrWorkspaceSave.Tick += ( s, e ) => OnWorkspaceSaveTick();
	}

	// ================================================================
	// Window lifecycle
	// ================================================================
	private void OnLoaded( object sender, EventArgs ea )
	{
		DoTitleSet();
		DoMruUpdate();
		DoIntroductionAdd();
		DoTabDragSetup();

		m_bAutoSave = AppSetting.AutoSave;
		m_mniAutoSave.IsChecked = m_bAutoSave;

		// Check for temp workspace recovery
		DoTempWorkspaceRestore();

		// Timers are one-shot: started by DoAutoSaveDebounce / DoWorkspaceSaveDebounce
		// when edits occur, not free-running on startup.
	}

	private bool m_bClosingConfirmed = false;

	private async void OnClosing( object sender, WindowClosingEventArgs ea )
	{
		if( m_bClosingConfirmed )
		{
			DoDocumentCloseAll();
			return;
		}

		m_tmrAutoSave.Stop();
		m_tmrWorkspaceSave.Stop();

		if( m_mgrWorkspace == null || m_lstMap.Count == 0 )
		{
			DoCleanupTempFile();
			DoDocumentCloseAll();
			return;
		}

		// Always ask to save on close
		ea.Cancel = true;

		string strMsg = m_strWorkspaceRealFile != null
			? string.Format( "Save workspace \"{0}\" before closing?",
							 Path.GetFileName( m_strWorkspaceRealFile ) )
			: "Would you like to save the workspace before closing?";

		var result = await DoShowSavePrompt( strMsg );

		if( result == SavePromptResult.Yes )
		{
			// Save GFF data to real files
			DoGffSaveToReal();

			if( m_strWorkspaceRealFile != null )
			{
				m_mgrWorkspace.DoSave( m_strWorkspaceRealFile );
			}
			else
			{
				OnCommandFileSaveWorkspaceAs();
				await Task.Delay( 500 );
				if( m_strWorkspaceRealFile == null )
					return;
			}
		}
		else if( result == SavePromptResult.Cancel )
		{
			return;
		}
		// SavePromptResult.No → don't save, revert to last manual save

		// Clean up ALL temp files — no temp files remain after normal close
		DoGffCleanupTemp();
		DoCleanupTempFile();

		m_bClosingConfirmed = true;
		Close();
	}

	private void DoCleanupTempFile()
	{
		string strTemp = GetTempFilePath();
		if( strTemp != null && File.Exists( strTemp ) )
			try { File.Delete( strTemp ); } catch {}
	}

	private enum SavePromptResult { Yes, No, Cancel }

	private async Task<SavePromptResult> DoShowSavePrompt( string strMessage )
	{
		var tcs = new TaskCompletionSource<SavePromptResult>();

		var dlg = new Window
		{
			Title				= "Save Workspace",
			Width				= 340,
			SizeToContent		= SizeToContent.Height,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			CanResize			= false,
			ShowInTaskbar		= false,
		};

		var txtMsg = new TextBlock
		{
			Text				= strMessage,
			TextWrapping		= Avalonia.Media.TextWrapping.Wrap,
			Margin				= new Avalonia.Thickness( 20, 20, 20, 13 ),
			TextAlignment		= Avalonia.Media.TextAlignment.Left,
			FontSize			= 13,
			FontWeight			= Avalonia.Media.FontWeight.Bold,
		};

		var btnSave = new Avalonia.Controls.Button
		{
			Content = "Save", Width = 200, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness( 0, 3 ),
		};
		btnSave.Classes.Add( "accent" );
		var btnDontSave = new Avalonia.Controls.Button
		{
			Content = "Don't Save", Width = 200, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness( 0, 3 ),
		};
		var btnCancel = new Avalonia.Controls.Button
		{
			Content = "Cancel", Width = 200, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness( 0, 3 ),
		};

		btnSave.Click     += ( s, e ) => { tcs.TrySetResult( SavePromptResult.Yes );    dlg.Close(); };
		btnDontSave.Click += ( s, e ) => { tcs.TrySetResult( SavePromptResult.No );     dlg.Close(); };
		btnCancel.Click   += ( s, e ) => { tcs.TrySetResult( SavePromptResult.Cancel );  dlg.Close(); };

		var pnlButtons = new StackPanel
		{
			Orientation			= Avalonia.Layout.Orientation.Vertical,
			HorizontalAlignment	= Avalonia.Layout.HorizontalAlignment.Center,
			Margin				= new Avalonia.Thickness( 0, 0, 0, 16 ),
		};
		pnlButtons.Children.Add( btnSave );
		pnlButtons.Children.Add( btnDontSave );
		pnlButtons.Children.Add( btnCancel );

		var pnlRoot = new DockPanel();
		DockPanel.SetDock( pnlButtons, Avalonia.Controls.Dock.Bottom );
		pnlRoot.Children.Add( pnlButtons );
		pnlRoot.Children.Add( txtMsg );

		dlg.Content = pnlRoot;

		dlg.Closing += ( s, e ) => tcs.TrySetResult( SavePromptResult.Cancel );

		await dlg.ShowDialog( this );
		return await tcs.Task;
	}

	private void DoDocumentCloseAll()
	{
		foreach( var doc in m_lstMap.ToList() )
			doc.DoClose();

		m_lstMap.Clear();
		m_tabDocuments.Items.Clear();

		s_lstWindows.Remove( this );
	}

	// ================================================================
	// Active document helpers
	// ================================================================
	private DocMap GetActiveDocument()
	{
		if( m_tabDocuments == null )		return null;

		var tab = m_tabDocuments.SelectedItem as TabItem;
		if( tab == null )				return null;

		return tab.Content as DocMap;
	}

	private PnlMap GetActiveMap()
	{
		var doc = GetActiveDocument();
		if( doc == null )			return null;
		return doc.PanelActive;
	}

	private PnlMapLane GetActiveLane()
	{
		var map = GetActiveMap();
		if( map == null )							return null;
		if( map.LaneSelected == null )				return null;
		if( map.LaneSelected.Count == 0 )			return null;
		return map.LaneSelected[0];
	}

	// ================================================================
	// File commands
	// ================================================================
	private void OnCommandFileNewDocument()
	{
		// v1.1.11: New Document is hidden (IsVisible="False" in menu)
		// No-op — documents are created when GFF files are opened
	}

	private async void OnCommandFileOpen()
	{
		var dlg = await StorageProvider.OpenFilePickerAsync( new FilePickerOpenOptions
		{
			Title				= "Open GFF File",
			AllowMultiple		= true,
			FileTypeFilter		= new[]
			{
				new FilePickerFileType( "GFF Files" )	{ Patterns = new[] { "*.gff", "*.gz", "*.gzip", "*.zip" } },
				new FilePickerFileType( "All Files" )	{ Patterns = new[] { "*.*" } }
			}
		} );

		if( dlg == null || dlg.Count == 0 )		return;

		var strFileA = dlg.Select( f => f.Path.LocalPath ).ToArray();
		DoFileOpen( strFileA );
	}

	private void DoFileOpen( string[] strFileA )
	{
		foreach( var strFile in strFileA )
		{
			if( !UtilityFile.GetFileExist( strFile ) )
			{
				ErrorMessage.ShowErrorFileNotFound( strFile );
				continue;
			}

			string strExt = Path.GetExtension( strFile ).ToLowerInvariant();

			if( strExt == ".workspace" )
			{
				DoWorkspaceOpen( strFile );
				continue;
			}

			if( strExt != ".gff" && strExt != ".gz" && strExt != ".gzip" && strExt != ".zip" )
			{
				ErrorMessage.ShowErrorFileNotSupported( strExt );
				continue;
			}

			if( ManagerData.GetManager().IsContainingFile( strFile ) )
			{
				// File already loaded in ManagerData — reuse it in this window
				DoGffFileReuse( strFile );
				continue;
			}

			DoGffFileOpen( strFile );
		}
	}

	private void DoGffFileOpen( string strFile )
	{
		// If already loaded in ManagerData (e.g. by another window), reuse it
		if( ManagerData.GetManager().IsContainingFile( strFile ) )
		{
			DoGffFileReuse( strFile );
			return;
		}

		// Synchronous load (v1.1.11 uses async DialogFileOpen with progress bar — to be added later)
		try
		{
			string strExt = Path.GetExtension( strFile ).ToLowerInvariant();
			bool bReadOnly = new FileInfo( strFile ).Length >= AppSetting.ReadOnlyThresholdBytes
							|| strExt == ".gz" || strExt == ".gzip" || strExt == ".zip";

			var reader = new ReaderGff( strFile );
			if( !reader.DoReadFile() )		return;

			var df = reader.DataFile;
			df.IsReadOnly = bReadOnly;
			ManagerData.GetManager().DoDataFileAdd( df );

			// Assign a unique random color for this file's lanes
			var bshFile = ManagerBrush.GetManager().GetBrushRandom();
			for( int i = 0; i < df.GetCountDataType(); i++ )
			{
				var dt = df.GetDataType( i );
				if( dt.DoBrushGet() == null )
					dt.DoBrushSet( bshFile );
			}

			// Add lanes to active or new document
			foreach( var strSeqId in Enumerable.Range( 0, df.GetCountSequenceId() ).Select( i => df.GetSequenceId( i ) ) )
			{
				var doc = GetDocumentBySequenceId( strSeqId );
				if( doc == null )
				{
					doc = DoDocumentAdd( strSeqId );
				}
				doc.DoPanelLaneAdd( df );
				// Defer view update until after layout pass
				var docCapture = doc;
				Dispatcher.UIThread.Post( () =>
				{
					docCapture.DoUpdateView();
					docCapture.DoScrollSet();
				} );
			}

			AppSetting.DoRecentGffAdd( strFile );
			DoMruUpdate();
			DoExplorerUpdate();
			DoStatusBarUpdate();

			// Create a workspace if none exists — autosave will write to temp file
			if( m_mgrWorkspace == null )
			{
				m_mgrWorkspace = new ManagerWorkspace();
				// No real file yet — m_strWorkspaceRealFile stays null
			}

			m_mgrWorkspace.IsEdited = true;
		}
		catch( Exception e )
		{
			Logger.PrintLine( "# ERROR DoGffFileOpen: {0}", e.Message );
			ErrorMessage.ShowError( e.Message );
		}
	}

	/// <summary>
	/// Adds an already-loaded DataFile to this window's view without re-reading.
	/// Used when the same GFF is needed in multiple workspace windows.
	/// </summary>
	private void DoGffFileReuse( string strFile )
	{
		var md = ManagerData.GetManager();
		var df = md.GetDataFile( strFile );
		if( df == null )		return;

		foreach( var strSeqId in Enumerable.Range( 0, df.GetCountSequenceId() ).Select( i => df.GetSequenceId( i ) ) )
		{
			var doc = GetDocumentBySequenceId( strSeqId );
			if( doc == null )
			{
				doc = DoDocumentAdd( strSeqId );
			}
			doc.DoPanelLaneAdd( df );
			var docCapture = doc;
			Dispatcher.UIThread.Post( () =>
			{
				docCapture.DoUpdateView();
				docCapture.DoScrollSet();
			} );
		}

		DoExplorerUpdate();
		DoStatusBarUpdate();
	}

	private async void OnCommandFileOpenWorkspace()
	{
		var dlg = await StorageProvider.OpenFilePickerAsync( new FilePickerOpenOptions
		{
			Title				= "Open Workspace",
			AllowMultiple		= false,
			FileTypeFilter		= new[]
			{
				new FilePickerFileType( "Workspace" )	{ Patterns = new[] { "*.workspace" } },
				new FilePickerFileType( "All Files" )	{ Patterns = new[] { "*.*" } }
			}
		} );

		if( dlg == null || dlg.Count == 0 )		return;
		DoWorkspaceOpen( dlg[0].Path.LocalPath );
	}

	private void DoWorkspaceOpen( string strFile )
	{
		// If this window already has documents, open workspace in a new window
		if( m_lstMap.Count > 0 )
		{
			var win = new MainWindow();
			win.Show();
			win.DoWorkspaceOpen( strFile );
			return;
		}

		try
		{
			m_mgrWorkspace = ManagerWorkspace.MakeFromFile( strFile );
			if( m_mgrWorkspace == null )		return;

			// Track the real workspace file (not a temp autosave)
			bool bIsTemp = Path.GetFileName( strFile ).StartsWith( Constant.S_TEMP_PREFIX );
			m_strWorkspaceRealFile = bIsTemp ? null : strFile;

			var strFileA = m_mgrWorkspace.GetFileArray();
			if( strFileA != null )
			{
				foreach( var strGff in strFileA )
				{
					if( !string.IsNullOrEmpty( strGff ) && UtilityFile.GetFileExist( strGff ) )
					{
						DoGffFileOpen( strGff );
					}
				}
			}

			// Restore lane arrangement, colors, display modes, scales, zoom, position
			DoDropEndWork();

			// Restore introduction tab visibility
			string strShowIntro = m_mgrWorkspace[ ManagerWorkspace.STR_STARTUP_SHOWINTRODUCTION ];
			if( strShowIntro != null )
			{
				bool bWantIntro = strShowIntro.ToLower() == "true";
				TabItem tabIntro = null;
				foreach( TabItem tab in m_tabDocuments.Items )
				{
					if( tab.Header is string strH && strH == "Introduction" )	{ tabIntro = tab; break; }
					if( tab.Header is Grid grd )
					{
						foreach( var child in grd.Children )
							if( child is TextBlock tb && tb.Text == "Introduction" )	{ tabIntro = tab; break; }
					}
				}

				if( bWantIntro && tabIntro == null )
					DoIntroductionAdd();
				else if( !bWantIntro && tabIntro != null )
					m_tabDocuments.Items.Remove( tabIntro );
			}

			// Restore application scale
			string strAppScaleX = m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_APPLICAIONSCALEX ];
			string strAppScaleY = m_mgrWorkspace[ ManagerWorkspace.STR_LAYOUT_APPLICAIONSCALEY ];
			if( strAppScaleX != null || strAppScaleY != null )
			{
				var ltc = m_dpMain.Children[0] as LayoutTransformControl;
				var st = ltc?.LayoutTransform as ScaleTransform;
				if( st != null )
				{
					if( strAppScaleX != null )		st.ScaleX = double.Parse( strAppScaleX );
					if( strAppScaleY != null )		st.ScaleY = double.Parse( strAppScaleY );
				}
			}

			// Restore bookmarks
			if( m_mgrWorkspace.Bookmark != null )
			{
				ManagerBookmark.GetManager().ListBookmark.Clear();
				ManagerBookmark.GetManager().ListBookmark.AddRange( m_mgrWorkspace.Bookmark );
			}

			m_mgrWorkspace.IsEdited = false;

			string strFileName = Path.GetFileName( strFile );
			if( strFileName != null && !strFileName.StartsWith( Constant.S_TEMP_PREFIX ) )
				AppSetting.DoRecentWorkspaceAdd( strFile );

			DoMruUpdate();
			DoTitleSet();
		}
		catch( Exception e )
		{
			Logger.PrintLine( "# ERROR DoWorkspaceOpen: {0}", e.Message );
			ErrorMessage.ShowError( e.Message );
		}
	}

	/// <summary>
	/// Restores lane arrangement, colors, display modes, scale settings,
	/// zoom level, and scroll position from the current workspace.
	/// Equivalent to WPF v1.1.11 DoDropEndWork().
	/// </summary>
	private void DoDropEndWork()
	{
		if( m_mgrWorkspace == null )		return;

		var md = ManagerData.GetManager();

		foreach( WorkspaceMap wm in m_mgrWorkspace.Map )
		{
			var dm = DoDocumentFind( wm.SequenceId );
			if( dm == null )		continue;

			var lstLane = new List< PnlMapLane >();

			foreach( WorkspaceLane wl in wm.ListLane )
			{
				if( wl.ListType.Count == 0 )		continue;

				// Find the lane that already contains the first type
				var pnl = dm.PanelActive.GetLane( wl.ListType[0].Type );
				if( pnl == null )		continue;

				// Apply each type's saved state
				foreach( WorkspaceType wt in wl.ListType )
				{
					var dt = md.GetDataType( wm.SequenceId, wt.Type );
					if( dt == null )		continue;

					dt.DoColorSet( wt.Color );
					dt.Display		= wt.GetDisplay();
					dt.ScaleMax		= wt.ScaleMax;
					dt.ScaleMin		= wt.ScaleMin;
					if( !double.IsNaN( wt.ScaleMax ) && !double.IsNaN( wt.ScaleMin ) )
						dt.Scale	= true;

					// If this type isn't in the lane yet, add it (grouped lanes)
					if( !pnl.IsContainingDataType( dt ) )
						pnl.DoDataTypeAdd( dt );
				}

				pnl.LaneHeight	= wl.Height;
				lstLane.Add( pnl );
			}

			// Replace default lane order with saved order
			dm.PanelActive.DoLaneRemoveAll();
			foreach( var pnl in lstLane )
				dm.PanelActive.DoLaneAdd( pnl );

			// Restore zoom and position
			dm.PanelActive.Zoom = wm.Zoom;
			dm.PanelActive.SetPosition( wm.Position );

			// Update the view
			dm.DoUpdateView();
			dm.DoScrollSet();
		}
	}

	private async void OnCommandFileOpenLayout()
	{
		var dlg = await StorageProvider.OpenFilePickerAsync( new FilePickerOpenOptions
		{
			Title				= "Open Layout",
			AllowMultiple		= false,
			FileTypeFilter		= new[]
			{
				new FilePickerFileType( "Layout" )		{ Patterns = new[] { "*.layout" } },
				new FilePickerFileType( "All Files" )	{ Patterns = new[] { "*.*" } }
			}
		} );

		if( dlg == null || dlg.Count == 0 )		return;
		// Layout restore is a minor feature — workspace loading handles most state
	}

	private void OnCommandFileSaveWorkspace()
	{
		if( m_strWorkspaceRealFile == null )
		{
			OnCommandFileSaveWorkspaceAs();
			return;
		}

		// Save GFF data to real files and workspace to real file
		DoGffSaveToReal();
		m_mgrWorkspace.DoSave( m_strWorkspaceRealFile );
		m_mgrWorkspace.IsEdited = false;

		// Delete temp files since we just saved the real ones
		DoGffCleanupTemp();
		DoCleanupTempFile();

		UtilityMessage.ShowMessageFileSaveWorkspace( m_strWorkspaceRealFile );
	}

	private async void OnCommandFileSaveWorkspaceAs()
	{
		var dlg = await StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
		{
			Title				= "Save Workspace As",
			DefaultExtension	= "workspace",
			FileTypeChoices		= new[]
			{
				new FilePickerFileType( "Workspace" )	{ Patterns = new[] { "*.workspace" } }
			}
		} );

		if( dlg == null )		return;

		if( m_mgrWorkspace == null )
			m_mgrWorkspace = new ManagerWorkspace();

		m_strWorkspaceRealFile = dlg.Path.LocalPath;
		m_mgrWorkspace.DoSave( dlg.Path.LocalPath );
		AppSetting.DoRecentWorkspaceAdd( dlg.Path.LocalPath );
		DoMruUpdate();
		DoTitleSet();
		UtilityMessage.ShowMessageFileSaveWorkspace( dlg.Path.LocalPath );
	}

	private async void OnCommandFileSaveLayoutAs()
	{
		var dlg = await StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
		{
			Title				= "Save Layout As",
			DefaultExtension	= "layout",
			FileTypeChoices		= new[]
			{
				new FilePickerFileType( "Layout" )		{ Patterns = new[] { "*.layout" } }
			}
		} );

		if( dlg == null )		return;
		// Layout save is handled via workspace system
	}

	private void OnCommandFileSaveAll()
	{
		// Explicit Save All — save GFF to real files and clean up temps
		DoGffSaveToReal();
		var dm = ManagerData.GetManager();
		var lst = new List<string>();
		for( int i = 0; i < dm.GetCountDataFile(); i++ )
			lst.Add( dm.GetDataFile( i ).FileName );
		if( lst.Count > 0 )
			UtilityMessage.ShowMessageFilesSave( string.Join( "\r\n", lst ) );
	}

	private async void OnCommandFileExportImage()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		var dlg = await StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
		{
			Title				= "Export Image",
			DefaultExtension	= "png",
			SuggestedFileName	= DateTime.Now.ToString( "yyyy-MM-dd_HH'hr'_mm'm'" ),
			FileTypeChoices		= new[]
			{
				new FilePickerFileType( "PNG Image" )	{ Patterns = new[] { "*.png" } },
				new FilePickerFileType( "SVG Image" )	{ Patterns = new[] { "*.svg" } }
			}
		} );

		if( dlg == null )		return;

		string strFile = dlg.Path.LocalPath;
		string strExt = Path.GetExtension( strFile ).ToLowerInvariant();

		Cursor = new Avalonia.Input.Cursor( Avalonia.Input.StandardCursorType.Wait );

		if( strExt == ".svg" )
			doc.PanelActive.DoExportSvg( strFile );
		else
			doc.PanelActive.DoExportPng( strFile, 300.0 );

		Cursor = Avalonia.Input.Cursor.Default;
	}

	private void OnCommandFileCloseAll()
	{
		ManagerData.GetManager().DoFileCloseAll();
		m_lstMap.Clear();
		m_tabDocuments.Items.Clear();
		DoExplorerUpdate();
		DoStatusBarUpdate();
	}

	private void OnCommandFileExit()
	{
		Close();
	}

	// ================================================================
	// Data — Feature commands
	// ================================================================
	private async void OnCommandDataSearch()
	{
		try
		{
			DoBottomPaneShow();
			if( m_tabBottom != null )
				m_tabBottom.SelectedItem = m_tabSearch;

			Logger.PrintLine( "# Search: creating dialog" );
			var dlg = new DialogSearch();
			Logger.PrintLine( "# Search: showing dialog" );
			await dlg.ShowDialog<bool?>( this );
			Logger.PrintLine( "# Search: dialog closed" );
		}
		catch( Exception ex )
		{
			Logger.PrintLine( "# ERROR OnCommandDataSearch: {0}\n{1}", ex.Message, ex.StackTrace );
			await new Window
			{
				Title = "Search Error",
				Width = 500, Height = 300,
				Content = new TextBox { Text = ex.ToString(), IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap }
			}.ShowDialog( this );
		}
	}

	private void OnCommandDataFeatureUnite()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		if( doc.PanelActive.ListLaneEditable.Count == 0 )
			ErrorMessage.ShowErrorSelectLaneFirst();
		else
			doc.PanelActive.DoLaneFeatureUniteSelected();
	}

	private void OnCommandDataFeatureMerge()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		if( doc.PanelActive.ListLaneEditable.Count == 0 )
			ErrorMessage.ShowErrorSelectLaneFirst();
		else
			doc.PanelActive.ListLaneEditable[0].DoHeadFeatOpMergeClick();
	}

	private void OnCommandDataFeatureMove()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		if( doc.PanelActive.ListLaneEditable.Count == 0 )
			ErrorMessage.ShowErrorSelectLaneFirst();
		else
			doc.PanelActive.ListLaneEditable[0].DoHeadFeatOpMoveClick();
	}

	private void OnCommandDataFeatureCopy()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		if( doc.PanelActive.ListLaneEditable.Count == 0 )
			ErrorMessage.ShowErrorSelectLaneFirst();
		else
			doc.PanelActive.ListLaneEditable[0].DoHeadFeatOpCopyClick();
	}

	private void OnCommandDataFeatureDelete()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		if( doc.PanelActive.ListLaneEditable.Count == 0 )
			ErrorMessage.ShowErrorSelectLaneFirst();
		else
			doc.PanelActive.DoLaneFeatureDeleteSelected();
	}

	private void OnCommandDataFeatureUndo()
	{
		var mgr = ManagerEdit.GetManager();
		if( mgr.GetCount() == 0 )		return;

		Cursor = new Avalonia.Input.Cursor( Avalonia.Input.StandardCursorType.Wait );

		var cmd = mgr.RemoveCommandLast();
		cmd.DoUndo();
		cmd.DoLaneUpdate();

		Cursor = Avalonia.Input.Cursor.Default;

		// Refresh the active document view to show undo changes
		var doc = GetActiveDocument();
		if( doc != null )
		{
			doc.DoUpdateView();
			doc.DoScrollSet();
		}

		DoEditUpdate();
		DoExplorerUpdate();
		DoAutoSaveImmediate();
	}

	private void OnCommandDataSelectByPosition()
	{
		m_bSelectByPosition = !m_bSelectByPosition;
		m_mniSelectByPosition.IsChecked = m_bSelectByPosition;
	}

	// ================================================================
	// Data — Track commands
	// ================================================================
	private void OnCommandDataTrackSetColor()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadSetColorClick();
	}

	private void OnCommandDataTrackSetHeight()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadSetHeightClick();
	}

	private void OnCommandDataTrackDisplay( EDataTypeDisplay eDisplay )
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;

		switch( eDisplay )
		{
			case EDataTypeDisplay.BAR:		pnl.DoHeadDisplayBox();		break;
			case EDataTypeDisplay.POINT:	pnl.DoHeadDisplayPoint();	break;
			case EDataTypeDisplay.LINE:		pnl.DoHeadDisplayLine();	break;
			case EDataTypeDisplay.STACK:	pnl.DoHeadDisplayStack();	break;
		}
	}

	private void OnCommandDataTrackMoveUp()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		doc.PanelActive.DoLaneMoveUp( pnl );
	}

	private void OnCommandDataTrackMoveDown()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		doc.PanelActive.DoLaneMoveDown( pnl );
	}

	private void OnCommandDataTrackGroup()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;
		doc.PanelActive.DoLaneGroup();
	}

	private void OnCommandDataTrackUngroup()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;
		doc.PanelActive.DoLaneUngroup();
	}

	private void OnCommandDataTrackSelectToEdit()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadSelectToEditClick();
	}

	private void OnCommandDataTrackSelectAllFeatures()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadSelectAllClick();
	}

	private void OnCommandDataTrackManualScale()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadManualScaleClick();
	}

	private void OnCommandDataTrackChangeType()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadChangeTypeClick();
	}

	private void OnCommandDataTrackHideLane()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadHideClick();
	}

	private void OnCommandDataTrackCloseFile()
	{
		var pnl = GetActiveLane();
		if( pnl == null )		return;
		pnl.DoHeadCloseClick();
	}

	private void OnCommandDataTrackOpAverage()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadOpeartionAverageClick();
	}

	private void OnCommandDataTrackOpDifference()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadOpeartionDiffClick();
	}

	private void OnCommandDataTrackOpSummation()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadOpeartionSumClick();
	}

	private void OnCommandDataTrackOpMerge()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadOpeartionMergeClick();
	}

	private void OnCommandDataTrackOpFilter()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadOpeartionFilterClick();
	}

	// ================================================================
	// Data — Integration commands
	// ================================================================
	private void OnCommandDataIntegrationPorf()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadIntegrationPorfClick();
	}

	private void OnCommandDataIntegrationRts()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadIntegrationRtsClick();
	}

	private void OnCommandDataIntegrationTu()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadIntegrationTuClick();
	}

	private void OnCommandDataIntegrationTrn()
	{
		var pnl = GetActiveLane();
		if( pnl != null )		pnl.DoHeadIntegrationTrnClick();
	}

	// ================================================================
	// View commands
	// ================================================================
	private void OnCommandViewZoomIn()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelZoomIn();
	}

	private void OnCommandViewZoomOut()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelZoomOut();
	}

	private async void OnCommandViewZoomTo()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		var dlg = new DialogZoomTo( doc.PanelActive );
		dlg.SetElementValue();
		var b = await dlg.ShowDialog<bool?>( this );
		if( b == true )
		{
			double dZoom = dlg.DoZoomGet();
			doc.DoPanelZoomTo( dZoom );
		}
	}

	private void OnCommandViewScrollLeft()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelScrollLeft();
	}

	private void OnCommandViewScrollRight()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelScrollRight();
	}

	private async void OnCommandViewPositionTo()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		var dlg = new DialogPositionTo( doc.PanelActive );
		dlg.SetElementValue();
		var b = await dlg.ShowDialog<bool?>( this );
		if( b == true )
		{
			int nPosition = dlg.DoPositionGet();
			doc.DoPanelPositionTo( nPosition );
		}
	}

	private void OnCommandViewSplit()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoSplitSet( !doc.IsSplitted );
	}

	private async void OnCommandViewFeatureOpacity()
	{
		var dlg = new DialogFeatureOpacity();
		dlg.SetElementValue();
		var b = await dlg.ShowDialog<bool?>( this );
		if( b == true )
		{
			string strOpacity = dlg.DoOpacityGet();
			ManagerBrush.DoOpacitySet( strOpacity );

			foreach( var dm in m_lstMap )
			{
				foreach( PnlMapLane pml in dm.PanelMap.LaneList )
				{
					foreach( DataType dt in pml.DataTypeList )
					{
						var bshOld = dt.DoBrushGet() as Avalonia.Media.ISolidColorBrush;
						if( bshOld != null )
						{
							var bshNew = ManagerBrush.GetManager().GetBrush( bshOld );
							dt.DoBrushSet( bshNew );
						}
					}
				}
				dm.DoUpdateView();
			}
		}
	}

	private void OnCommandViewScrollLeftSmall()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelScrollLeftSmall();
	}

	private void OnCommandViewScrollRightSmall()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelScrollRightSmall();
	}

	private void OnCommandViewGoHome()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;
		doc.DoPanelPositionTo( doc.PanelActive.PositionMin );
	}

	private void OnCommandViewGoEnd()
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;
		var map = doc.PanelActive;
		int nPage = UtilityMath.DoRound( (double)( map.PositionMax - map.PositionMin ) / map.Zoom );
		doc.DoPanelPositionTo( map.PositionMax - nPage );
	}

	private void OnCommandViewTabNext()
	{
		if( m_tabDocuments == null || m_tabDocuments.ItemCount <= 1 )	return;
		int nIdx = m_tabDocuments.SelectedIndex + 1;
		if( nIdx >= m_tabDocuments.ItemCount )		nIdx = 0;
		m_tabDocuments.SelectedIndex = nIdx;
	}

	private void OnCommandViewTabPrev()
	{
		if( m_tabDocuments == null || m_tabDocuments.ItemCount <= 1 )	return;
		int nIdx = m_tabDocuments.SelectedIndex - 1;
		if( nIdx < 0 )		nIdx = m_tabDocuments.ItemCount - 1;
		m_tabDocuments.SelectedIndex = nIdx;
	}

	private void OnCommandViewRefresh()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoUpdateView();
	}

	private void OnCommandViewScaleUp()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelScaleUp();
	}

	private void OnCommandViewScaleDown()
	{
		var doc = GetActiveDocument();
		if( doc != null )		doc.DoPanelScaleDown();
	}

	// ================================================================
	// Feature adjust commands
	// ================================================================
	private void OnCommandFeatureMoveLeft()		{ DoFeatureAdjust( -1, -1 ); }	// ⌥← move left (keep length)
	private void OnCommandFeatureMoveRight()	{ DoFeatureAdjust( 1, 1 ); }	// ⌥→ move right (keep length)
	private void OnCommandFeatureShrinkStart()	{ DoFeatureAdjust( 1, 0 ); }	// ⌥↑ increase start
	private void OnCommandFeatureExpandEnd()	{ DoFeatureAdjust( 0, 1 ); }	// ⌥↓ increase end

	private void DoFeatureAdjust( int nStartDelta, int nEndDelta )
	{
		var doc = GetActiveDocument();
		if( doc == null )		return;

		var pm = doc.PanelActive;
		if( pm == null )		return;

		// Collect selected features across all lanes (matching WPF reference)
		var lstSelected = new List<DataFeature>();
		PnlMapLane pnlOwner = null;

		foreach( PnlMapLane pnl in pm.LaneList )
		{
			var lst = pnl.ListFeatureSelected;
			if( lst != null && lst.Count > 0 )
			{
				lstSelected.AddRange( lst );
				pnlOwner = pnl;
			}
		}

		if( lstSelected.Count == 0 )		return;

		if( lstSelected.Count >= 2 )
		{
			ErrorMessage.ShowError( "Please select only one feature." );
			return;
		}

		if( pnlOwner.DataTypeSelected == null || pnlOwner.DataTypeSelected.IsReadOnly )
			return;

		// Single feature — create adjusted copy
		DataFeature dfOld = lstSelected[0];
		DataFeature dfNew = new DataFeature( dfOld );
		if( dfNew.ColorBrush == null )
			dfNew.ColorBrush = dfOld.ColorBrush;

		dfNew.Start += nStartDelta;
		dfNew.End   += nEndDelta;

		// Undo support — consolidate consecutive adjustments on the same feature
		var me = ManagerEdit.GetManager();
		Services.Command.CommandEdit cmd = null;

		if( me.GetCount() > 0 )
		{
			var cbLast = me.GetCommandLast();
			if( cbLast is Services.Command.CommandEdit ceLast )
			{
				if( ceLast.LaneOwner == pnlOwner && ceLast.FeatureCurrent == dfOld )
				{
					cmd = ceLast;
					cmd.UpdateAdjust( dfOld, dfNew );
				}
			}
		}

		if( cmd == null )
		{
			cmd = me.MakeCommandEdit();
			cmd.DoFeatureAdd( pnlOwner, dfOld, dfNew );
			cmd.SetAdjustInfo( pnlOwner, dfOld, dfNew );
		}

		// Apply — remove old, add new (maintains DataType sorted order)
		DataType dt = pnlOwner.DataTypeSelected;
		dt.DoFeatureRemove( dfOld );
		dt.DoFeatureAdd( dfNew );

		// Re-select the new feature
		pnlOwner.DoFeatureSelect( dfNew );

		// Update highlight to follow adjusted feature
		pm.DoFeatureHighlightSet( dfNew.Start, dfNew.End );

		DoEditUpdate();
		pnlOwner.DoLayoutUpdate();

		DoAutoSaveDebounce();
	}

	// ================================================================
	// Window commands
	// ================================================================
	private void OnCommandWindowIntroduction()
	{
		if( m_tabDocuments != null && m_tabDocuments.ItemCount > 0 )
			m_tabDocuments.SelectedIndex = 0;
	}

	private void OnCommandWindowFileExplorer()
	{
		if( m_tabTools != null )
			m_tabTools.SelectedItem = m_tabExplorer;
	}

	private void OnCommandWindowSetting()
	{
		// v1.1.11: Settings panel uses WinForms PropertyGrid — not ported (IsVisible="False" in menu)
	}

	private void OnCommandWindowEdit()
	{
		if( m_tabTools != null )
			m_tabTools.SelectedItem = m_tabEdit;
	}

	private void OnCommandWindowBookmark()
	{
		if( m_tabTools != null )
			m_tabTools.SelectedItem = m_tabBookmark;
	}

	private void OnCommandWindowFeature()
	{
		if( m_tabTools != null )
			m_tabTools.SelectedItem = m_tabFeature;
	}

	private void OnCommandWindowFeatureSelected()
	{
		if( m_tabTools != null )
			m_tabTools.SelectedItem = m_tabFeatureSelected;
	}

	private void OnCommandWindowLog()
	{
		DoBottomPaneShow();
		if( m_tabBottom != null )
			m_tabBottom.SelectedItem = m_tabLog;
	}

	private void OnCommandWindowSearch()
	{
		if( m_grdContent.RowDefinitions[2].Height.Value > 0 )
			DoBottomPaneHide();
		else
			DoBottomPaneShow();
	}

	private void DoBottomPaneShow()
	{
		m_grdContent.RowDefinitions[1].Height = new GridLength( 5 );
		m_grdContent.RowDefinitions[2].Height = new GridLength( 200 );
		m_grdContent.RowDefinitions[2].MinHeight = 80;
	}

	private void DoBottomPaneHide()
	{
		m_grdContent.RowDefinitions[1].Height = new GridLength( 0 );
		m_grdContent.RowDefinitions[2].MinHeight = 0;
		m_grdContent.RowDefinitions[2].Height = new GridLength( 0 );
	}

	// ================================================================
	// Help commands
	// ================================================================
	private void OnCommandHelpAbout()
	{
		var launcher = TopLevel.GetTopLevel( this )?.Launcher;
		launcher?.LaunchUriAsync( new Uri( "https://sbml-lab.ai/software" ) );
	}

	private void OnCommandHelpDocumentation()
	{
		var launcher = TopLevel.GetTopLevel( this )?.Launcher;
		launcher?.LaunchUriAsync( new Uri( "https://github.com/sbml-lab/MetaScope" ) );
	}

	private async void OnCommandHelpShortcuts()
	{
		var dlg = new DialogShortcuts();
		await dlg.ShowDialog<bool?>( this );
	}

	private void OnCommandHelpAutoSave()
	{
		m_bAutoSave = !m_bAutoSave;
		m_mniAutoSave.IsChecked = m_bAutoSave;
		AppSetting.AutoSave = m_bAutoSave;

		if( !m_bAutoSave )
		{
			m_tmrAutoSave.Stop();
			m_tmrWorkspaceSave.Stop();
		}

		string strMsg = m_bAutoSave ? "Auto Save enabled." : "Auto Save disabled.";
		UtilityMessage.ShowMessage( strMsg );
	}

	private void OnCommandHelpUpdate()
	{
		var launcher = TopLevel.GetTopLevel( this )?.Launcher;
		launcher?.LaunchUriAsync( new Uri( "https://github.com/sbml-lab/MetaScope" ) );
	}

	private void OnCommandHelpGarbageCollection()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	// ================================================================
	// Document management
	// ================================================================
	private DocMap GetDocumentBySequenceId( string strSeqId )
	{
		return m_lstMap.FirstOrDefault( d => d.SequenceId == strSeqId );
	}

	private DocMap DoDocumentAdd( string strSequenceId )
	{
		var doc = new DocMap();
		doc.SequenceId = strSequenceId;
		doc.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
		doc.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
		m_lstMap.Add( doc );

		var tab = new TabItem
		{
			Content					= doc,
			HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
			VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
		};

		tab.Header = MakeClosableTabHeader( strSequenceId, () => DoDocumentClose( tab, doc ) );

		m_tabDocuments.Items.Add( tab );
		m_tabDocuments.SelectedItem = tab;

		return doc;
	}

	private void DoDocumentClose( TabItem tab, DocMap doc )
	{
		doc.DoClose();
		m_lstMap.Remove( doc );
		m_tabDocuments.Items.Remove( tab );

		// If no other document uses a file, remove it from ManagerData
		var md = ManagerData.GetManager();
		for( int i = md.GetCountDataFile() - 1; i >= 0; i-- )
		{
			var df = md.GetDataFile( i );
			bool bUsed = false;
			foreach( var dm in m_lstMap )
			{
				if( dm.PanelMap.LaneList.Any( l => l.DataTypeList.Any( dt => dt.DataFile == df ) ) )
				{
					bUsed = true;
					break;
				}
			}
			if( !bUsed )
				md.DoDataFileRemove( df );
		}

		DoExplorerUpdate();
		DoStatusBarUpdate();
	}

	private Grid MakeClosableTabHeader( string text, Action onClose )
	{
		var grid = new Grid { MinWidth = 90 };
		grid.ColumnDefinitions.Add( new ColumnDefinition( 18, GridUnitType.Pixel ) );
		grid.ColumnDefinitions.Add( new ColumnDefinition( GridLength.Star ) );
		grid.ColumnDefinitions.Add( new ColumnDefinition( 18, GridUnitType.Pixel ) );

		var btn = new Button
		{
			Content		= "\u00D7",
			FontSize	= 9,
			Padding		= new Thickness( 0 ),
			MinWidth	= 0,
			MinHeight	= 0,
			Width		= 18,
			Height		= 18,
			CornerRadius = new CornerRadius( 9 ),
			BorderThickness = new Thickness( 0 ),
			VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
			HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
			Cursor		= new Avalonia.Input.Cursor( Avalonia.Input.StandardCursorType.Hand ),
			Margin		= new Thickness( -17, 0, 0, 0 ),
			Classes		= { "tab-close" }
		};
		btn.Click += ( s, e ) => { e.Handled = true; onClose(); };
		Grid.SetColumn( btn, 0 );

		var tb = new TextBlock
		{
			Text = text,
			VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			FontSize = 13,
			MaxWidth = 120,
			TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
		};
		Grid.SetColumn( tb, 1 );

		grid.Children.Add( btn );
		grid.Children.Add( tb );

		return grid;
	}

	private void DoTabDragSetup()
	{
		m_tabDocuments.AddHandler( Avalonia.Input.InputElement.PointerPressedEvent, OnTabStripPointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel );
		m_tabDocuments.AddHandler( Avalonia.Input.InputElement.PointerMovedEvent, OnTabStripPointerMoved, Avalonia.Interactivity.RoutingStrategies.Tunnel );
		m_tabDocuments.AddHandler( Avalonia.Input.InputElement.PointerReleasedEvent, OnTabStripPointerReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel );
	}

	private TabItem FindTabItemFromVisual( Visual v )
	{
		while( v != null )
		{
			if( v is TabItem ti && m_tabDocuments.Items.Contains( ti ) )
				return ti;
			v = v.GetVisualParent() as Visual;
		}
		return null;
	}

	private TabItem FindTabItemAtX( double dX, TabItem skipTab = null )
	{
		foreach( TabItem tab in m_tabDocuments.Items )
		{
			if( tab == skipTab )	continue;
			// Use Bounds (layout position) not TranslatePoint (which includes RenderTransform)
			double dLeft = tab.Bounds.X;
			double dRight = dLeft + tab.Bounds.Width;
			double dMid = ( dLeft + dRight ) / 2.0;
			// Only match if pointer crosses the midpoint of the target tab
			if( dX >= dLeft && dX <= dRight )
			{
				// For swap: require crossing past the midpoint
				if( skipTab != null )
				{
					int nFrom = m_tabDocuments.Items.IndexOf( skipTab );
					int nTo = m_tabDocuments.Items.IndexOf( tab );
					if( nFrom < nTo && dX < dMid )		continue;	// dragging right: need to pass midpoint
					if( nFrom > nTo && dX > dMid )		continue;	// dragging left: need to pass midpoint
				}
				return tab;
			}
		}
		return null;
	}

	private void OnTabStripPointerPressed( object sender, Avalonia.Input.PointerPressedEventArgs ea )
	{
		if( !ea.GetCurrentPoint( m_tabDocuments ).Properties.IsLeftButtonPressed )	return;

		var source = ea.Source as Visual;
		if( source is Button || ( source?.Parent is Button ) )	return;

		// Only allow drag from the tab strip area, not the content area
		var pt = ea.GetPosition( m_tabDocuments );
		var firstTab = m_tabDocuments.ItemCount > 0 ? m_tabDocuments.Items[0] as TabItem : null;
		if( firstTab != null )
		{
			var tabBottom = firstTab.TranslatePoint( new Point( 0, firstTab.Bounds.Height ), m_tabDocuments );
			Console.WriteLine( $"[DRAG-DEBUG] click Y={pt.Y:F1}, tab.Bounds={firstTab.Bounds}, TranslatePoint={tabBottom}, source={ea.Source?.GetType().Name}" );
			if( tabBottom.HasValue && pt.Y > tabBottom.Value.Y + 4 )	return;
		}

		var tab = FindTabItemFromVisual( source );
		if( tab == null )	return;

		m_tabDragItem = tab;
		m_ptDragStart = ea.GetPosition( m_tabDocuments );
		m_bTabDragging = false;
	}

	private void OnTabStripPointerMoved( object sender, Avalonia.Input.PointerEventArgs ea )
	{
		if( m_tabDragItem == null )		return;

		var pt = ea.GetPosition( m_tabDocuments );
		if( !m_bTabDragging )
		{
			if( Math.Abs( pt.X - m_ptDragStart.X ) < 8 )	return;
			m_bTabDragging = true;
			m_tabDragItem.ZIndex = 100;
			m_tabDragItem.Opacity = 0.8;
			m_tabDragItem.RenderTransform = new TranslateTransform();
		}

		// Move dragged tab with pointer
		if( m_tabDragItem.RenderTransform is TranslateTransform tt )
			tt.X = pt.X - m_ptDragStart.X;

		// Swap when crossing another tab's midpoint
		var target = FindTabItemAtX( pt.X, m_tabDragItem );
		if( target != null )
		{
			int nFrom = m_tabDocuments.Items.IndexOf( m_tabDragItem );
			int nTo = m_tabDocuments.Items.IndexOf( target );
			if( nFrom >= 0 && nTo >= 0 )
			{
				// Calculate layout shift: the dragged tab's position will change by the target's width
				double dShift = nTo > nFrom ? target.Bounds.Width : -target.Bounds.Width;

				m_tabDocuments.Items.RemoveAt( nFrom );
				m_tabDocuments.Items.Insert( nTo, m_tabDragItem );
				m_tabDocuments.SelectedItem = m_tabDragItem;

				// Adjust drag start immediately so transform stays correct
				m_ptDragStart = new Point( m_ptDragStart.X + dShift, m_ptDragStart.Y );
				if( m_tabDragItem.RenderTransform is TranslateTransform tt2 )
					tt2.X = pt.X - m_ptDragStart.X;
			}
		}
	}

	private void OnTabStripPointerReleased( object sender, Avalonia.Input.PointerReleasedEventArgs ea )
	{
		if( m_tabDragItem != null )
		{
			m_tabDragItem.ZIndex = 0;
			m_tabDragItem.Opacity = 1.0;
			m_tabDragItem.RenderTransform = null;
			m_tabDragItem = null;
			m_bTabDragging = false;
		}
	}

	private static TextBlock IntroHeader( string text )
	{
		return new TextBlock
		{
			Text = text,
			FontSize = 16,
			FontWeight = FontWeight.SemiBold,
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 40, 40, 40 ) ),
			Margin = new Thickness( 0, 8, 0, 0 )
		};
	}

	private static Border IntroSeparator()
	{
		return new Border { Height = 1, Background = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 210, 210, 210 ) ), Margin = new Thickness( 0, 4 ) };
	}

	private static TextBlock IntroBullet( string text, bool bBold = false )
	{
		return new TextBlock
		{
			Text = "\u2022  " + text,
			TextWrapping = TextWrapping.Wrap,
			FontSize = 14,
			FontWeight = bBold ? FontWeight.SemiBold : FontWeight.Normal,
			LineHeight = 22,
			Margin = new Thickness( 16, 2, 0, 0 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 80, 80, 80 ) )
		};
	}

	private static TextBlock IntroVersion( string ver )
	{
		return new TextBlock
		{
			Text = ver,
			FontSize = 13,
			FontWeight = FontWeight.SemiBold,
			Margin = new Thickness( 16, 6, 0, 0 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) )
		};
	}

	private static TextBlock IntroShortcut( string key, string action )
	{
		return new TextBlock
		{
			Text = $"{key,-28} {action}",
			FontSize = 12,
			FontFamily = new Avalonia.Media.FontFamily( "Menlo, 'SF Mono', Consolas, monospace" ),
			Margin = new Thickness( 16, 2, 0, 0 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 80, 80, 80 ) )
		};
	}

	private void DoIntroductionAdd()
	{
		var body = new StackPanel { Spacing = 4 };

		// Title
		body.Children.Add( new TextBlock
		{
			Text = "MetaScope",
			FontSize = 32,
			FontWeight = FontWeight.Light,
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 40, 40, 40 ) ),
			FontFamily = new Avalonia.Media.FontFamily( "SF Pro Display, -apple-system, sans-serif" )
		} );

		// What is MetaScope?
		body.Children.Add( IntroSeparator() );
		body.Children.Add( IntroHeader( "What is MetaScope?" ) );
		body.Children.Add( new TextBlock
		{
			Text = "MetaScope is a genome browser with integrative functions, highly flexible and interactive user interface, " +
				"by which molecular biologists with minimal computational skills can visualize their genome-scale datasets " +
				"along with canonical genomic annotations, and analyze, curate and integrate with data operation functions of MetaScope. " +
				"The datasets MetaScope can handle include tiling array data (ChIP-chip and expression profiling), " +
				"calculated peak data, transcription start site data, and genomic annotation in GFF format.",
			TextWrapping = TextWrapping.Wrap,
			FontSize = 14,
			LineHeight = 22,
			Margin = new Thickness( 16, 4, 0, 0 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 80, 80, 80 ) )
		} );

		// Screenshot
		try
		{
			var imgBorder = new Border
			{
				Background = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 248, 248, 250 ) ),
				BorderBrush = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 220, 220, 225 ) ),
				BorderThickness = new Thickness( 1 ),
				CornerRadius = new CornerRadius( 4 ),
				Padding = new Thickness( 12 ),
				Margin = new Thickness( 16, 8, 0, 0 ),
				Child = new StackPanel
				{
					Spacing = 6,
					Children =
					{
						new Avalonia.Controls.Image
						{
							Source = new Avalonia.Media.Imaging.Bitmap(
								System.IO.Path.Combine( System.IO.Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Assets", "Introduction.MetaScope.png" ) ),
							MaxWidth = 640,
							Stretch = Avalonia.Media.Stretch.Uniform
						},
						new TextBlock
						{
							Text = "Figure 1. MetaScope showing genomic annotation, expression profiling data, and RNA polymerase ChIP-chip data\n" +
								"of E. coli K12 MG1655 (upper side) and K. pneumoniae MGH 78578 (lower side).",
							TextWrapping = TextWrapping.Wrap,
							FontSize = 11,
							FontStyle = Avalonia.Media.FontStyle.Italic,
							TextAlignment = Avalonia.Media.TextAlignment.Center,
							Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 120, 120, 120 ) )
						}
					}
				}
			};
			body.Children.Add( imgBorder );
		}
		catch { /* image not found — skip silently */ }

		// Use MetaScope in order to
		body.Children.Add( IntroSeparator() );
		body.Children.Add( IntroHeader( "Use MetaScope in order to" ) );
		body.Children.Add( IntroBullet( "Visualize datasets including ChIP-chip data, expression profiling data and transcription start site data", true ) );
		body.Children.Add( IntroBullet( "Analyze, validate, curate and integrate datasets by cross-referencing multiple -omic data", true ) );
		body.Children.Add( IntroBullet( "Build and share genome annotations", true ) );
		body.Children.Add( IntroBullet( "Analyze and compare multiple -omics data from two or more species", true ) );

		// Version
		body.Children.Add( IntroSeparator() );
		body.Children.Add( new TextBlock
		{
			Text = $"Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.0.0"}",
			FontSize = 13,
			FontWeight = FontWeight.SemiBold,
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) )
		} );

		// Recent Changes
		body.Children.Add( IntroSeparator() );
		body.Children.Add( IntroHeader( "Recent Changes" ) );

		// macOS
		body.Children.Add( new TextBlock
		{
			Text = "\uF8FF  macOS",
			FontSize = 13,
			FontWeight = FontWeight.Bold,
			Margin = new Thickness( 0, 10, 0, 2 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 40, 40, 40 ) )
		} );
		body.Children.Add( IntroVersion( "v1.2.0" ) );
		body.Children.Add( IntroBullet( "macOS Apple Silicon native port using AvaloniaUI" ) );
		body.Children.Add( IntroBullet( "Drag-to-reorder document tabs with close button" ) );
		body.Children.Add( IntroBullet( "macOS-native UI redesign" ) );

		// Windows
		body.Children.Add( new TextBlock
		{
			Text = "\u229E  Windows",
			FontSize = 13,
			FontWeight = FontWeight.Bold,
			Margin = new Thickness( 0, 14, 0, 2 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 40, 40, 40 ) )
		} );
		body.Children.Add( IntroVersion( "v1.1.11" ) );
		body.Children.Add( IntroBullet( "Workspace: auto-creates a temporary workspace when opening GFF files, enabling auto-save and crash recovery" ) );
		body.Children.Add( IntroVersion( "v1.1.10" ) );
		body.Children.Add( IntroBullet( "Auto Save: automatic saving after feature edits (persists across sessions)" ) );
		body.Children.Add( IntroBullet( "Alt+Arrow keys for feature editing (move, expand, shrink)" ) );
		body.Children.Add( IntroVersion( "v1.1.9" ) );
		body.Children.Add( IntroBullet( "Export Image: PNG (300dpi) and SVG vector export (\u2318\u21E7E)" ) );
		body.Children.Add( IntroVersion( "v1.1.8" ) );
		body.Children.Add( IntroBullet( "Feature tooltip on hover (position, score, strand, name)" ) );
		body.Children.Add( IntroVersion( "v1.1.7" ) );
		body.Children.Add( IntroBullet( "NumPad feature adjustment and read-only track selection blocked" ) );
		body.Children.Add( IntroVersion( "v1.1.6" ) );
		body.Children.Add( IntroBullet( "Recent Files list, status bar, read-only threshold 20MB" ) );
		body.Children.Add( IntroVersion( "v1.1.5" ) );
		body.Children.Add( IntroBullet( "Arrow key scroll, Home/End navigation, \u2318Tab switching, F5 refresh" ) );

		// Keyboard Shortcuts
		body.Children.Add( IntroSeparator() );
		body.Children.Add( IntroHeader( "Keyboard Shortcuts" ) );
		body.Children.Add( new TextBlock { Text = "Navigation", FontSize = 12, FontWeight = FontWeight.SemiBold, Margin = new Thickness( 16, 6, 0, 0 ), Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) ) } );
		body.Children.Add( IntroShortcut( "\u2190 / \u2192", "Scroll left / right" ) );
		body.Children.Add( IntroShortcut( "\u21E7+\u2190 / \u21E7+\u2192", "Scroll left / right (large)" ) );
		body.Children.Add( IntroShortcut( "Home / End", "Go to genome start / end" ) );
		body.Children.Add( IntroShortcut( "\u2318G", "Go to position" ) );
		body.Children.Add( new TextBlock { Text = "Zoom", FontSize = 12, FontWeight = FontWeight.SemiBold, Margin = new Thickness( 16, 6, 0, 0 ), Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) ) } );
		body.Children.Add( IntroShortcut( "\u2318+ / \u2318-", "Zoom in / out" ) );
		body.Children.Add( IntroShortcut( "\u2318 0", "Zoom to custom level" ) );
		body.Children.Add( IntroShortcut( "\u2318 Scroll", "Zoom in / out (mouse)" ) );
		body.Children.Add( new TextBlock { Text = "File and View", FontSize = 12, FontWeight = FontWeight.SemiBold, Margin = new Thickness( 16, 6, 0, 0 ), Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) ) } );
		body.Children.Add( IntroShortcut( "\u2318O", "Open file" ) );
		body.Children.Add( IntroShortcut( "\u2318Tab / \u2318\u21E7Tab", "Next / previous tab" ) );
		body.Children.Add( IntroShortcut( "\u2318T", "Split view" ) );
		body.Children.Add( IntroShortcut( "\u2318F", "Search" ) );
		body.Children.Add( IntroShortcut( "F5", "Refresh view" ) );
		body.Children.Add( new TextBlock { Text = "Track", FontSize = 12, FontWeight = FontWeight.SemiBold, Margin = new Thickness( 16, 6, 0, 0 ), Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) ) } );
		body.Children.Add( IntroShortcut( "\u2318\u21E7\u2191 / \u2318\u21E7\u2193", "Move track up / down" ) );
		body.Children.Add( IntroShortcut( "\u2318\u21E7C", "Set track color" ) );
		body.Children.Add( IntroShortcut( "\u2318\u21E7H", "Set track height" ) );
		body.Children.Add( IntroShortcut( "\u2318\u21E7B / P / L", "Display as bar / point / line" ) );
		body.Children.Add( new TextBlock { Text = "Feature Editing", FontSize = 12, FontWeight = FontWeight.SemiBold, Margin = new Thickness( 16, 6, 0, 0 ), Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 60, 60, 60 ) ) } );
		body.Children.Add( IntroShortcut( "\u2325\u2190 / \u2325\u2192", "Move feature left / right" ) );
		body.Children.Add( IntroShortcut( "\u2325\u2191 / \u2325\u2193", "Expand end / shrink start" ) );

		// Open file hint
		body.Children.Add( IntroSeparator() );
		body.Children.Add( new TextBlock
		{
			Text = "Open a GFF file to get started:  File \u2192 Open  (\u2318O)",
			FontSize = 12,
			Margin = new Thickness( 0, 8, 0, 0 ),
			Foreground = new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.FromRgb( 140, 140, 140 ) )
		} );

		var outer = new Border
		{
			Background = Avalonia.Media.Brushes.White,
			Child = new ScrollViewer
			{
				Content = new Border
				{
					Child = body,
					Margin = new Thickness( 40, 30 ),
					MaxWidth = 680
				}
			}
		};

		var tab = new TabItem { Content = outer };
		tab.Header = MakeClosableTabHeader( "Introduction", () => m_tabDocuments.Items.Remove( tab ) );
		m_tabDocuments.Items.Add( tab );
		m_tabDocuments.SelectedItem = tab;
	}

	// ================================================================
	// UI update helpers
	// ================================================================
	public void DoExplorerUpdate()
	{
		DoExplorerUpdateFile();
		DoExplorerUpdateSequenceId();
		DoExplorerUpdateType();
	}

	private void DoExplorerUpdateFile()
	{
		ManagerData		md				= ManagerData.GetManager();

		m_tviFile.Items.Clear();
		m_tviFile.IsExpanded			= true;

		for( int i = 0; i < md.GetCountDataFile(); i++ )
		{
			DataFile		df				= md.GetDataFile( i );

			TreeViewItem	tviFile			= new TreeViewItem();
			tviFile.Header					= ( df.IsReadOnly ? "[R] " : "" )
												+ ( df.IsEdited == false
													? string.Format( "{0}", df.FileName )
													: string.Format( "{0} (*)", df.FileName ) );
			tviFile.IsExpanded				= true;

			for( int j = 0; j < df.GetCountDataType(); j++ )
			{
				DataType		dt				= df.GetDataType( j );

				TreeViewItem	tviType			= new TreeViewItem();
				tviType.Header					= dt.IsEdited == false
													? string.Format( "{0}:{1}", dt.SequenceId, dt.Type )
													: string.Format( "{0}:{1} (*)", dt.SequenceId, dt.Type );
				tviType.IsExpanded				= false;

				var dtCapture = dt;
				tviType.DoubleTapped			+= ( obj, ea ) =>
				{
					DocMap doc = DoDocumentShow( dtCapture.SequenceId );
					if( doc != null )
						doc.DoPanelLaneShow( dtCapture.Type );
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

		var lst = new List<string>();

		for( int i = 0; i < md.GetCountDataFile(); i++ )
		{
			DataFile df = md.GetDataFile( i );
			for( int j = 0; j < df.GetCountSequenceId(); j++ )
			{
				string strSeqId = df.GetSequenceId( j );
				if( !lst.Contains( strSeqId ) )
					lst.Add( strSeqId );
			}
		}

		foreach( string str in lst )
		{
			TreeViewItem	tvi				= new TreeViewItem();
			tvi.Header						= str;
			tvi.IsExpanded					= true;

			var strCapture = str;
			tvi.DoubleTapped				+= ( obj, ea ) => DoDocumentShow( strCapture );

			m_tviSequenceId.Items.Add( tvi );
		}
	}

	private void DoExplorerUpdateType()
	{
		ManagerData		md				= ManagerData.GetManager();

		m_tviType.Items.Clear();
		m_tviType.IsExpanded			= true;

		var lst = new List<string>();

		for( int i = 0; i < md.GetCountDataFile(); i++ )
		{
			DataFile df = md.GetDataFile( i );
			for( int j = 0; j < df.GetCountType(); j++ )
			{
				string strType = df.GetType( j );
				if( !lst.Contains( strType ) )
					lst.Add( strType );
			}
		}

		foreach( string str in lst )
		{
			TreeViewItem	tvi				= new TreeViewItem();
			tvi.Header						= str;
			tvi.IsExpanded					= true;

			m_tviType.Items.Add( tvi );
		}
	}

	private DocMap DoDocumentShow( string strSequenceId )
	{
		var doc = GetDocumentBySequenceId( strSequenceId );
		if( doc == null )		return null;

		// Select the tab
		foreach( TabItem tab in m_tabDocuments.Items )
		{
			if( tab.Content is DocMap dm && dm.SequenceId == strSequenceId )
			{
				m_tabDocuments.SelectedItem = tab;
				break;
			}
		}
		return doc;
	}

	private void DoDocumentMove( string strSequenceId, int nStart, int nEnd )
	{
		var doc = GetDocumentBySequenceId( strSequenceId );
		if( doc == null )		return;

		int nCenter = ( nStart + nEnd ) / 2;
		doc.DoPanelPositionTo( nCenter );
	}

	private void DoDocumentSelection( string strSequenceId, int nStart, int nEnd )
	{
		var doc = GetDocumentBySequenceId( strSequenceId );
		if( doc == null )		return;

		doc.PanelActive.DoFeatureHighlightSet( nStart, nEnd );
	}

	public void DoEditUpdate()
	{
		m_ltvEdit.Items.Clear();
		var me = ManagerEdit.GetManager();
		foreach( var cb in me.GetCommand() )
		{
			m_ltvEdit.Items.Add( cb.GetString() );
		}
	}

	// v1.1.11 PropertyGrid format: "Name                    Value"
	public void DoFeatureDisplay( DataFeature df )
	{
		m_ltvFeature.Items.Clear();
		if( df == null )		return;

		// Feature category — matches v1.1.11 PropertyFeature exactly
		m_ltvFeature.Items.Add( "── Feature ──" );
		m_ltvFeature.Items.Add( string.Format( "Source          {0}", df.Source ) );
		m_ltvFeature.Items.Add( string.Format( "Start           {0:N0}", df.Start ) );
		m_ltvFeature.Items.Add( string.Format( "End             {0:N0}", df.End ) );
		m_ltvFeature.Items.Add( string.Format( "Score           {0}", df.ScoreString ) );
		m_ltvFeature.Items.Add( string.Format( "Strand          {0}", df.Strand ) );
		m_ltvFeature.Items.Add( string.Format( "Phase           {0}", df.Phase ) );
		m_ltvFeature.Items.Add( string.Format( "Attribute       {0}", df.Attribute ) );

		// Attribute category — individual GFF attributes
		var dic = df.DoAttributeParse();
		if( dic != null && dic.Count > 0 )
		{
			m_ltvFeature.Items.Add( "" );
			m_ltvFeature.Items.Add( "── Attribute ──" );
			foreach( var kv in dic )
			{
				if( !string.IsNullOrEmpty( kv.Key ) )
					m_ltvFeature.Items.Add( string.Format( "{0,-16}{1}", kv.Key, kv.Value ) );
			}
		}

	}

	// v1.1.11 PropertyFeatureSelected format
	public void DoFeatureSelectedDisplay( List<DataFeature> lst )
	{
		m_ltvFeatureSelected.Items.Clear();
		if( lst == null || lst.Count == 0 )		return;

		DataFeature		dfFirst			= lst.First();
		DataFeature		dfLast			= lst.Last();

		// Statistics category
		m_ltvFeatureSelected.Items.Add( "── Statistics ──" );
		m_ltvFeatureSelected.Items.Add( string.Format( "Count           {0:N0}", lst.Count ) );

		// Feature category
		m_ltvFeatureSelected.Items.Add( "" );
		m_ltvFeatureSelected.Items.Add( "── Feature ──" );
		m_ltvFeatureSelected.Items.Add( string.Format( "Start           {0:N0}", dfFirst.Start ) );
		m_ltvFeatureSelected.Items.Add( string.Format( "End             {0:N0}", dfLast.End ) );
		m_ltvFeatureSelected.Items.Add( string.Format( "Score           {0:N0}", dfFirst.Score ) );
		m_ltvFeatureSelected.Items.Add( string.Format( "Strand          {0}", dfFirst.Strand ) );
		m_ltvFeatureSelected.Items.Add( string.Format( "Phase           {0}", dfFirst.Phase ) );
		m_ltvFeatureSelected.Items.Add( string.Format( "Attribute       {0}", dfFirst.Attribute ) );

	}

	public void DoFeatureHoverDisplay( DataFeature df )
	{
		// Update Feature panel content (like WPF DoFeatureSet) — no tab switch
		DoFeatureDisplay( df );

		// Update status bar
		if( df == null )
		{
			var doc = GetActiveDocument();
			m_tbStatusFileName.Text = doc != null ? doc.SequenceId : "";
			return;
		}
		m_tbStatusFileName.Text = string.Format( "{0}  [{1:N0}..{2:N0}]  {3}  {4}",
			df.Source, df.Start, df.End, df.Strand, df.ScoreString );
	}

	public void DoBookmarkUpdate()
	{
		m_ltvBookmark.Items.Clear();
		var mb = ManagerBookmark.GetManager();
		foreach( var db in mb.ListBookmark )
		{
			m_ltvBookmark.Items.Add( db.GetString() );
		}
	}

	public void DoStatusBarUpdate()
	{
		var doc = GetActiveDocument();
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

		if( pnl != null )
		{
			m_tbStatusPosition.Text			= string.Format( "Position: {0:N0} bp", pnl.Position );
			m_tbStatusZoom.Text				= string.Format( "Zoom: {0}x", pnl.Zoom );
		}

		m_tbStatusFeatureCount.Text			= string.Format( "Features: {0:N0}", md.CachedFeatureCount );
		m_tbStatusFileName.Text				= doc.SequenceId != null ? doc.SequenceId : "";
	}

	// ================================================================
	// Float / Dock tool panel
	// ================================================================
	private		Window							m_wndFloatingTools				= null;

	public void OnFloatToolsClick( object sender, RoutedEventArgs ea )
	{
		if( m_wndFloatingTools != null )
		{
			m_wndFloatingTools.Close();
			return;
		}

		// Hide tools and splitter, make document fill 100%
		m_bdrTools.IsVisible = false;
		foreach( var child in m_grdContent.Children.OfType<GridSplitter>().Where( g => Grid.GetColumn( g ) == 1 ).ToList() )
			child.IsVisible = false;
		m_grdContent.ColumnDefinitions[1].Width = new GridLength( 0 );
		m_grdContent.ColumnDefinitions[2].Width = new GridLength( 0 );
		m_grdContent.ColumnDefinitions[0].Width = new GridLength( 1, GridUnitType.Star );

		// Detach content for floating window
		var content = m_bdrTools.Child;
		m_bdrTools.Child = null;

		m_wndFloatingTools = new Window
		{
			Title		= "Inspector",
			Width		= 320,
			Height		= 550,
			Content		= content,
			Background	= new Avalonia.Media.SolidColorBrush( Avalonia.Media.Color.Parse( "#F2F2F7" ) ),
			FontFamily	= new Avalonia.Media.FontFamily( "-apple-system, 'Helvetica Neue', sans-serif" ),
			WindowStartupLocation = WindowStartupLocation.CenterScreen
		};

		m_wndFloatingTools.Closed += ( s, e ) =>
		{
			// Re-dock
			var floatContent = m_wndFloatingTools.Content as Avalonia.Controls.Control;
			m_wndFloatingTools.Content = null;

			m_bdrTools.Child = floatContent as DockPanel;
			m_bdrTools.IsVisible = true;
			m_grdContent.ColumnDefinitions[1].Width = new GridLength( 1 );
			m_grdContent.ColumnDefinitions[2].MinWidth = 150;
			m_grdContent.ColumnDefinitions[2].Width = new GridLength( 280 );

			foreach( var child in m_grdContent.Children.OfType<GridSplitter>().Where( g => Grid.GetColumn( g ) == 1 ) )
				child.IsVisible = true;

			m_wndFloatingTools = null;
		};

		m_wndFloatingTools.Show();
	}

	public void DoTitleSet()
	{
		// v1.1.11 format: "MetaScope {version}  - {filename}"
		string strVersion = Assembly.GetExecutingAssembly()
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
			?.InformationalVersion ?? "2.0.0";

		string strFile = m_mgrWorkspace?.FileName;

		Title = string.Format( "MetaScope {0}{1}",
			strVersion,
			strFile == null ? "" : string.Format( "  - {0}", strFile ) );
	}

	public DocMap DoDocumentActive()
	{
		return GetActiveDocument();
	}



	public void DoDrop( string[] strFileA )
	{
		DoFileOpen( strFileA );
	}

	private void OnDrop( object sender, DragEventArgs ea )
	{
		var files = ea.Data.GetFiles();
		if( files == null )		return;

		var strFileA = files.Select( f => f.Path.LocalPath ).ToArray();
		if( strFileA.Length > 0 )
			DoDrop( strFileA );
	}

	// ================================================================
	// Dialog helpers
	// ================================================================
	private async void DoShowError( string strMsg )
	{
		// Simple error dialog
		var dlg = new Window
		{
			Title		= "Error",
			Width		= 400,
			SizeToContent = SizeToContent.Height,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			CanResize	= false,
			Content		= new StackPanel
			{
				Margin		= new Thickness( 20 ),
				Spacing		= 11,
				Children	=
				{
					new TextBlock { Text = strMsg, TextWrapping = TextWrapping.Wrap, FontSize = 13, FontWeight = Avalonia.Media.FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Left },
					new Button { Content = "OK", Width = 120, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center }
				}
			}
		};

		( (StackPanel) dlg.Content ).Children.OfType<Button>().First().Click += ( s, e ) => dlg.Close();
		await dlg.ShowDialog( this );
	}

	private async void DoShowMessage( string strMsg )
	{
		var dlg = new Window
		{
			Title		= "Message",
			Width		= 400,
			SizeToContent = SizeToContent.Height,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			CanResize	= false,
			Content		= new StackPanel
			{
				Margin		= new Thickness( 20 ),
				Spacing		= 11,
				Children	=
				{
					new TextBlock { Text = strMsg, TextWrapping = TextWrapping.Wrap, FontSize = 13, FontWeight = Avalonia.Media.FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Left },
					new Button { Content = "OK", Width = 120, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center }
				}
			}
		};

		( (StackPanel) dlg.Content ).Children.OfType<Button>().First().Click += ( s, e ) => dlg.Close();
		await dlg.ShowDialog( this );
	}

	private bool DoConfirmSave( string strTitle, string strMessage )
	{
		// Synchronous wrapper — ManagerData calls this from non-async context
		// For now, auto-save without prompting (matches auto-save behavior)
		return true;
	}

	private string DoSaveFileDialog( string strDefaultName )
	{
		// Synchronous wrapper — ManagerData calls this from non-async context
		// Returns null to skip (caller handles null as "cancelled")
		return null;
	}

	// ================================================================
	// MRU (Most Recently Used)
	// ================================================================
	private void DoMruUpdate()
	{
		if( m_nmnFile == null )		return;

		foreach( var item in m_lstMruItems )
			m_nmnFile.Items.Remove( item );
		m_lstMruItems.Clear();

		var lstWorkspace = AppSetting.RecentWorkspaceList;
		var lstGff = AppSetting.RecentGffList;
		int insertAt = m_nMruInsertIndex;

		if( lstWorkspace.Count > 0 )
		{
			var sep = new NativeMenuItemSeparator();
			m_nmnFile.Items.Insert( insertAt, sep );
			m_lstMruItems.Add( sep );
			insertAt++;
			for( int i = 0; i < lstWorkspace.Count; i++ )
			{
				string strPath = lstWorkspace[i];
				string strName = Path.GetFileName( strPath );
				var nmi = new NativeMenuItem( $"{i + 1}  {strName}" );
				string capturedPath = strPath;
				nmi.Click += ( s, e ) => DoDrop( new[] { capturedPath } );
				m_nmnFile.Items.Insert( insertAt, nmi );
				m_lstMruItems.Add( nmi );
				insertAt++;
			}
		}

		if( lstGff.Count > 0 )
		{
			var sep = new NativeMenuItemSeparator();
			m_nmnFile.Items.Insert( insertAt, sep );
			m_lstMruItems.Add( sep );
			insertAt++;
			for( int i = 0; i < lstGff.Count; i++ )
			{
				string strPath = lstGff[i];
				string strName = Path.GetFileName( strPath );
				var nmi = new NativeMenuItem( $"{i + 1}  {strName}" );
				string capturedPath = strPath;
				nmi.Click += ( s, e ) => DoDrop( new[] { capturedPath } );
				m_nmnFile.Items.Insert( insertAt, nmi );
				m_lstMruItems.Add( nmi );
				insertAt++;
			}
		}
	}

	// ================================================================
	// Auto-save and temp workspace
	// ================================================================
	private void OnAutoSaveTick()
	{
		m_tmrAutoSave.Stop();
		if( !m_bAutoSave )		return;

		var dm = ManagerData.GetManager();
		if( dm == null || !dm.IsEdited )
			return;

		// Autosave GFF to temp files, not originals
		DoGffSaveToTemp();
	}

	private void OnWorkspaceSaveTick()
	{
		m_tmrWorkspaceSave.Stop();
		if( !m_bAutoSave )		return;
		if( m_mgrWorkspace == null )		return;

		// Autosave writes to a temp file, never the real workspace
		string strTemp = GetTempFilePath();
		if( strTemp == null )		return;

		try { m_mgrWorkspace.DoSave( strTemp ); } catch {}
		// Restore the real file path so manual Save still targets the original
		// (DoSave sets m_strFile to strTemp, so reset it)
		m_mgrWorkspace.File = m_strWorkspaceRealFile ?? m_mgrWorkspace.File;
	}

	/// <summary>
	/// Returns the temp autosave file path for the current workspace.
	/// Stored in AppData alongside other temp workspaces.
	/// </summary>
	private string GetTempFilePath()
	{
		if( m_mgrWorkspace == null )		return null;

		if( m_strWorkspaceRealFile != null )
		{
			string strBaseName = Path.GetFileNameWithoutExtension( m_strWorkspaceRealFile );
			return Path.Combine( AppSetting.AppDataDir,
				Constant.S_TEMP_PREFIX + strBaseName + ".workspace" );
		}

		// No real file — use a session-based temp name
		if( m_strSessionTempFile == null )
		{
			m_strSessionTempFile = Path.Combine( AppSetting.AppDataDir,
				string.Format( "{0}{1}.workspace",
					Constant.S_TEMP_PREFIX,
					DateTime.Now.ToString( "yyyy-MM-dd_HH'hr'_mm'm'" ) ) );
		}

		return m_strSessionTempFile;
	}

	private		string						m_strSessionTempFile			= null;

	public void DoAutoSaveImmediate()
	{
		if( !m_bAutoSave )		return;
		// Autosave GFF to temp files, not originals
		DoGffSaveToTemp();
	}

	/// <summary>
	/// Saves all edited GFF files to temp files (*.gff.tmp) alongside originals.
	/// The real files are only overwritten on explicit Save.
	/// </summary>
	private void DoGffSaveToTemp()
	{
		var dm = ManagerData.GetManager();
		for( int i = 0; i < dm.GetCountDataFile(); i++ )
		{
			var df = dm.GetDataFile( i );
			if( df.IsEdited && !df.IsReadOnly )
			{
				string strTemp = df.File + ".tmp";
				try { df.DoSave( strTemp ); } catch {}
				// DoSave(string) does NOT change IsEdited or File path
			}
		}
	}

	/// <summary>
	/// Saves all edited GFF files to their real paths (explicit Save).
	/// Deletes temp files afterwards.
	/// </summary>
	private void DoGffSaveToReal()
	{
		var dm = ManagerData.GetManager();
		dm.DoFileSaveAll();

		// Clean up temp GFF files
		for( int i = 0; i < dm.GetCountDataFile(); i++ )
		{
			var df = dm.GetDataFile( i );
			string strTemp = df.File + ".tmp";
			if( File.Exists( strTemp ) )
				try { File.Delete( strTemp ); } catch {}
		}
	}

	/// <summary>
	/// Deletes all temp GFF files (*.gff.tmp) — called on close without save.
	/// </summary>
	private void DoGffCleanupTemp()
	{
		var dm = ManagerData.GetManager();
		for( int i = 0; i < dm.GetCountDataFile(); i++ )
		{
			var df = dm.GetDataFile( i );
			string strTemp = df.File + ".tmp";
			if( File.Exists( strTemp ) )
				try { File.Delete( strTemp ); } catch {}
		}
	}

	public void DoAutoSaveDebounce()
	{
		if( !m_bAutoSave )		return;
		m_tmrAutoSave.Stop();
		m_tmrAutoSave.Start();
	}

	public void DoWorkspaceSaveDebounce()
	{
		if( !m_bAutoSave )		return;
		if( m_mgrWorkspace == null )		return;

		m_tmrWorkspaceSave.Stop();
		m_tmrWorkspaceSave.Start();
	}

	private async void DoTempWorkspaceRestore()
	{
		string strAppData = AppSetting.AppDataDir;
		if( !Directory.Exists( strAppData ) )		return;

		var files = Directory.GetFiles( strAppData, Constant.S_TEMP_PREFIX + "*.workspace" );
		if( files.Length == 0 )		return;

		foreach( var strTempFile in files )
		{
			string strName = Path.GetFileName( strTempFile );
			var result = await DoShowSavePrompt(
				string.Format( "A previous session was found: \"{0}\".\nWould you like to restore it?", strName ) );

			if( result == SavePromptResult.Yes )
			{
				// Also restore any .gff.tmp files by replacing originals
				DoGffTempRestore();
				DoWorkspaceOpen( strTempFile );
			}
			else
			{
				// User declined — delete temp workspace and temp GFF files
				try { File.Delete( strTempFile ); } catch {}
				DoGffTempCleanupAll();
			}
		}
	}

	/// <summary>
	/// On crash recovery: replace original GFF files with their .tmp versions.
	/// </summary>
	private void DoGffTempRestore()
	{
		var dm = ManagerData.GetManager();
		for( int i = 0; i < dm.GetCountDataFile(); i++ )
		{
			var df = dm.GetDataFile( i );
			string strTemp = df.File + ".tmp";
			if( File.Exists( strTemp ) )
			{
				try
				{
					File.Copy( strTemp, df.File, true );
					File.Delete( strTemp );
				}
				catch {}
			}
		}
	}

	/// <summary>
	/// Delete all .gff.tmp files found alongside loaded GFF files.
	/// </summary>
	private void DoGffTempCleanupAll()
	{
		var dm = ManagerData.GetManager();
		for( int i = 0; i < dm.GetCountDataFile(); i++ )
		{
			var df = dm.GetDataFile( i );
			string strTemp = df.File + ".tmp";
			if( File.Exists( strTemp ) )
				try { File.Delete( strTemp ); } catch {}
		}
	}

	// ================================================================
	// Search results helpers (used by DialogSearch)
	// ================================================================
	public DocMap GetActiveDocMap()
	{
		return GetActiveDocument();
	}

	public DocMap DoDocumentFind( string strSequenceId )
	{
		foreach( DocMap doc in m_lstMap )
		{
			if( doc.SequenceId == strSequenceId )
				return doc;
		}

		return null;
	}

	public void DoSearchResultsClear()
	{
		m_ltvSearch.Items.Clear();
	}

	public void DoSearchResultAdd( string strItem )
	{
		m_ltvSearch.Items.Add( strItem );
	}

	public void DoSearchResultsActivate()
	{
		m_tabBottom.SelectedItem		= m_tabSearch;
	}
}
