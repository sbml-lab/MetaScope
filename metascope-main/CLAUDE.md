# MetaScope - Project Guide

## Overview
MetaScope is a genome visualization tool built with C#/WPF. It reads GFF (General Feature Format) files and displays genomic features on an interactive map with docking panels.

## Build & Run
- **IDE**: Visual Studio 2022 / Visual Studio 2026
- **Framework**: .NET Framework 4.8 (upgraded from 4.0)
- **Solution**: `VugMap.sln`
- **Output**: `MetaScope.exe` (single exe, AvalonDock.dll embedded)

## Project Structure
```
metascope/
  VugMap.sln                    # VS 2022 solution (upgraded from VS 2010)
  VugMap/                       # Main WPF application (18,590 lines, 89 files)
    VugMap.csproj               # ToolsVersion 15.0, .NET 4.8
    app.config                  # Runtime config (.NET 4.8)
    MainWindow.xaml             # Main window with AvalonDock layout
    Utility/
      Data/
        DataFeature.cs          # Core genomic feature data class (per-feature object)
        DataType.cs             # Per-type feature storage with spatial index
        DataFile.cs             # File operations, feature merging
      AppSetting.cs             # Application settings (read-only threshold, etc.)
      Reader/
        ReaderGff.cs            # GFF file parser (supports file path and Stream)
        ReaderSetting.cs        # Key=Value config file parser
    Window/
      PnlMap.cs                 # Map rendering
      PnlMapLane.cs             # Lane rendering with score-based visualization
      DialogFileOpen.xaml.cs    # Async file loading with read-only detection
      DialogFeatureOperation.xaml.cs  # Feature filtering by score
  AvalonDock/                   # Custom docking library (14,579 lines, 62 files)
    AvalonDock.csproj           # ToolsVersion 15.0, .NET 4.8
```

## Key Data Classes

### DataFeature (memory-optimized)
Core object instantiated per genomic feature (potentially 100K+ objects).
```
Field               Type      Bytes   Notes
m_strSource         string    ref     string.Intern() for deduplication
m_nStart            int       4       Genomic start position
m_nEnd              int       4       Genomic end position
m_usScore           ushort    2       IEEE 754 half-precision float (was float 4B)
m_bStrand           byte      1       '+'/'-'/'.' as byte (was string)
m_bPhase            byte      1       '0'/'1'/'2'/'.' as byte (was string)
m_bAttributeA       byte[]    ref     ASCII-encoded attribute string (null in read-only mode)
m_bshColor          Brush     ref     Cached brush for rendering
```

Helper methods for half-precision: `FloatToHalf()`, `HalfToFloat()`, `HalfIsNaN()`

Static flag `SkipAttributeStorage`: when true, constructors skip `m_bAttributeA` allocation but still parse attributes for color extraction. Set during read-only file loading in `DialogFileOpen`.

### DataType (dual-mode: edit / read-only)
- `N_INDEXSPAN = 1000` (index bucket size in bp)
- `N_INDEXSIZE = 15,000` (index array size, covers 15M bp for bacteria)
- `N_KEYRANGE = 300,000,000` (for BigInteger key generation in SortedDictionary)
- **Edit mode**: LinkedList + dual index arrays (m_lstIndexStart, m_lstIndexEnd)
- **Read-only mode**: sorted `DataFeature[]` array only (LinkedList freed after BuildIndex)
  - `GetFeatureArray()`, `GetFeatureIndexByStart()`, `GetFeatureIndexByEnd()` for array access
  - Binary search on sorted array for O(log n) position lookup
  - Index arrays not allocated (~240KB saved per DataType)
  - LinkedList + LinkedListNode freed (~252MB saved for 7M features)
  - Edit operations (Add/Remove/Adjust/AssignId) are blocked

### DataFile
- `IsReadOnly` property: propagated to DataType instances during creation
- Set based on file size during GFF loading

## Supported File Formats
| Extension | Mode | Method |
|-----------|------|--------|
| `.gff` | Edit or Read-only (by size) | Direct file read |
| `.gz` / `.gzip` | Always Read-only | GZipStream in-memory decompression |
| `.zip` | Always Read-only | ZipArchive, reads all `.gff` entries |

- Compressed files are decompressed in memory (no temp files)
- `ReaderGff` accepts both file path and `Stream` via constructor overload

## Read-Only Mode
- **Threshold**: Files >= 20MB (20,000,000 bytes) default to read-only
- **Compressed files**: `.gz`/`.zip` always read-only
- **Config**: `AppSetting.cs` reads `%AppData%\MetaScope\MetaScope.setting` (Key=Value format via `ReaderSetting`)
  - Key: `ReadOnlyThreshold` (value in bytes)
  - First run: auto-migrates setting file from exe directory to AppData
- **Storage**: sorted `DataFeature[]` array with binary search (LinkedList freed after index build)
- **UI indicator**: Lock icon on AvalonDock document tab (uses existing `IsLocked` + `PART_LockedIcon`)
- **Edit guard**: `DoFeatureAdd`, `DoFeatureRemove`, `DoAdjust`, `DoAssignId`, `SetEdited` return early

## Memory Optimizations Applied
1. **Strand**: `string` -> `byte` (~60B saved per object)
2. **Phase**: `string` -> `byte` (~60B saved per object)
3. **Source**: `byte[]` -> `string.Intern()` (~40B saved per object)
4. **Score**: `float` (4B) -> `ushort` half-precision (2B) (2B saved per object)
5. **Index arrays**: 300K -> 15K entries (~4.6MB saved per DataType)
6. **Read-only mode**: Skips 15K x 2 index arrays (~240KB saved per DataType)
7. **Read-only attribute skip**: `m_bAttributeA` byte[] not allocated (~840MB saved for 7M features)
8. **Read-only LinkedList elimination**: LinkedList + LinkedListNode freed after BuildIndex (~252MB saved for 7M features)

### Read-only mode memory impact (460MB GFF, ~7M features)
| Optimization | Savings |
|-------------|---------|
| Attribute byte[] skip | ~840 MB |
| LinkedList + Node elimination | ~252 MB |
| **Total** | **~1,092 MB** (from ~1,428MB to ~336MB) |

## Version Management & Release Process
- **Current version**: 1.1.11
- **Single source**: `VugMap\Properties\AssemblyInfo.cs` (3 lines: AssemblyVersion, AssemblyFileVersion, AssemblyInformationalVersion)
- **Title bar**: Auto-reads `AssemblyInformationalVersion` via `Assembly.GetExecutingAssembly()` in `DoTitleSet()`
- **Introduction tab**: Version displayed dynamically via `m_runVersion` Run element, set in `OnIntroductionLoaded()`

### Release Steps
1. Release build (`VugMap.sln`, Configuration=Release)
2. Build output: `VugMap\bin\Release\MetaScope.exe`
3. Add new version entry to Recent Changes in Introduction (`MainWindow.xaml`)
4. Git commit and push
5. Attach exe to GitHub Releases

## Single Executable Deployment
- **AvalonDock.dll** embedded as `EmbeddedResource` in `VugMap.csproj` (LogicalName: `AvalonDock.dll`)
- **Runtime loading**: `App.xaml.cs` registers `AppDomain.AssemblyResolve` in App constructor (before XAML startup)
- **Result**: `MetaScope.exe` (~1MB) runs standalone without AvalonDock.dll alongside

## Keyboard Shortcuts

### View Navigation
| Key | Action |
|-----|--------|
| `Left` / `Right` | Scroll left/right (small) |
| `Shift+Left` / `Shift+Right` | Scroll left/right (large) |
| `Ctrl+Scroll` | Zoom in/out |
| `Shift+Scroll` | Scroll left/right |

### Feature Editing (requires editable lane)
| NumPad | Alt+Arrow | Action |
|--------|-----------|--------|
| `NumPad1` | `Alt+Left` | Move feature left |
| `NumPad2` | `Alt+Right` | Move feature right |
| `NumPad4` | `Alt+Up` | Shrink feature start |
| `NumPad5` | `Alt+Down` | Expand feature end |

### Other
| Key | Action |
|-----|--------|
| `Ctrl+Shift+Up/Down` | Move track up/down |
| `Ctrl+Tab` / `Ctrl+Shift+Tab` | Next/prev document tab |
| `F5` | Refresh |

## Feature Selection Highlight
- Clicking a feature in any lane shows a vertical blue semi-transparent overlay (`CLR_LANE_SELECTION`) spanning the feature's genomic start~end across all lanes
- Stored as genomic coordinates (`m_nHighlightStart/End` in `PnlMap.cs`), auto-recalculated on zoom/pan/resize via `DoFeatureHighlightUpdate()` called from `DoUpdateLaneLayout()`
- Clicking empty space clears the highlight
- Key methods: `DoFeatureHighlightSet()`, `DoFeatureHighlightClear()`, `DoFeatureHighlightUpdate()`

## Workspace & AppData
- **Setting file**: `%AppData%\MetaScope\MetaScope.setting` (auto-migrated from exe directory on first run)
- **Temp workspace**: `%AppData%\MetaScope\Temp_*.workspace` (auto-created on GFF open, auto-save/crash recovery)
- **Temp restore**: On startup, if temp workspace found, prompts restore and suggests Save As (converts to permanent workspace, deletes temp)
- **Recent files**: Separate lists for Workspace (5) and GFF (5), displayed in separate File menu sections
  - `AppSetting.RecentWorkspaceList` / `RecentGffList`
  - `AppSetting.DoRecentWorkspaceAdd()` / `DoRecentGffAdd()`
  - Temp workspaces are excluded from Recent list
- **AppSetting.AppDataDir**: Exposes `%AppData%\MetaScope\` path as public property
- **Cross-drive relative path**: `ManagerWorkspace.GetRelativePath()` catches UriFormatException and falls back to absolute path

## Known Fixes

### ScrollViewer Focus Stealing (DocMap.xaml)
- `ScrollViewer` elements set to `Focusable="False"` to prevent them from capturing keyboard focus
- Without this, clicking on the map area causes ScrollViewer to consume arrow key events, blocking MainWindow's RoutedCommand bindings

### Sub-pixel Feature Skip Near Selection (PnlMapLane.cs)
- Rendering optimization skips features within 0.5px of the previous drawn feature
- Bug: when moving a feature right toward a stationary feature, the stationary one gets skipped (drawn second, too close to selected feature drawn first)
- Fix: skip is bypassed when either the current or previous drawn feature is in the selection list (`bCurrSelected || bPrevSelected`)
- Applied to both STACK and POINT/BAR/LINE rendering paths

## Important Notes
- Target domain: **bacteria genomes** (max ~13M bp)
- Score supports **NaN** (represents missing "." in GFF files)
- Score NaN fallback: returns +1.0 for '+' strand, -1.0 for '-' strand
- 4 broken CommandBinding lines in MainWindow.xaml are commented out (were test code with malformed XAML)
- `MSBuildBinPath` replaced with `MSBuildToolsPath` in AvalonDock.csproj

## Coding Style
- **Indentation**: Tabs only (no spaces for indentation)
- **Alignment**: Tabs for column alignment of variable declarations and assignments

