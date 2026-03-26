# WPF v1.1.11 Reference Spec

Condensed behavioral specification of the original MetaScope WPF application.
Source: `Metascope_v1.1.11/metascope-main/` (read-only, never modify).

## Application Overview

- Genome visualization tool for bacterial GFF files
- C#/WPF, .NET Framework 4.8, Visual Studio solution
- Single executable deployment (~1MB, AvalonDock.dll embedded)
- Target domain: bacteria genomes (max ~13M bp)
- Total codebase: ~22,300 lines C# across 89 files

## Core Data Model

### DataFeature (per-feature object, potentially 100K+)
```
Field               Type      Bytes   Purpose
m_strSource         string    ref     string.Intern() deduplication
m_nStart            int       4       Genomic start position
m_nEnd              int       4       Genomic end position
m_usScore           ushort    2       IEEE 754 half-precision (FloatToHalf/HalfToFloat/HalfIsNaN)
m_bStrand           byte      1       '+'/'-'/'.' as byte
m_bPhase            byte      1       '0'/'1'/'2'/'.' as byte
m_bAttributeA       byte[]    ref     ASCII-encoded attributes (null in read-only mode)
m_bshColor          Brush     ref     Cached rendering brush
```
- Score NaN fallback: +1.0 for '+' strand, -1.0 for '-' strand
- Static flag `SkipAttributeStorage`: skips m_bAttributeA allocation but still parses for color

### DataType (dual-mode: edit / read-only)
- Index constants: N_INDEXSPAN=1000, N_INDEXSIZE=15000, N_KEYRANGE=300000000
- Edit mode: LinkedList + dual index arrays (m_lstIndexStart, m_lstIndexEnd)
- Read-only mode: sorted DataFeature[] with binary search, LinkedList freed after BuildIndex
- Edit operations blocked in read-only: Add/Remove/Adjust/AssignId

### DataFile
- IsReadOnly property propagated to DataType instances during creation

## File Format Support

| Extension | Mode | Method |
|-----------|------|--------|
| `.gff` | Edit or Read-only (by size) | Direct file read |
| `.gz`/`.gzip` | Always read-only | GZipStream in-memory |
| `.zip` | Always read-only | ZipArchive, reads all .gff entries |

- Read-only threshold: >= 20MB (configurable via AppSetting)
- Compressed files decompressed in memory (no temp files)
- ReaderGff accepts both file path and Stream

## Read-Only Mode

- Storage: sorted DataFeature[] with binary search (LinkedList freed)
- UI: Lock icon on AvalonDock document tab (IsLocked + PART_LockedIcon)
- Edit guard: DoFeatureAdd, DoFeatureRemove, DoAdjust, DoAssignId, SetEdited return early
- Memory savings on 7M features: ~1,092 MB (attribute skip ~840MB + LinkedList elimination ~252MB)

## Workspace & Settings

- Setting file: `%AppData%\MetaScope\MetaScope.setting` (Key=Value via ReaderSetting)
- Auto-migrates from exe directory on first run
- Temp workspace: `%AppData%\MetaScope\Temp_*.workspace` (auto-save, crash recovery)
- Temp restore: prompts Save As on startup (converts to permanent, deletes temp)
- Recent files: separate lists for Workspace (5) and GFF (5) in File menu

## Rendering Pipeline

- PnlMap.cs: main map rendering (WPF DrawingContext)
- PnlMapLane.cs: per-lane rendering with score-based visualization
- PnlMapRuler.cs: genomic coordinate ruler
- Feature highlight: vertical blue overlay (CLR_LANE_SELECTION) spanning start~end across all lanes
- Sub-pixel skip optimization: bypassed when current or previous feature is selected

## Docking System

- AvalonDock library (custom build, embedded as resource)
- DockingManager in MainWindow.xaml
- DocumentContent for map tabs, DockableContent for panels
- IsLocked property on DocumentContent for read-only indicator

## Keyboard Shortcuts (macOS)

### Navigation
- Left/Right: scroll small, Shift+Left/Right: scroll large
- Home/End: go to genome start/end
- ⌘G: go to position
- ⌘+Scroll: zoom, Shift+Scroll: horizontal scroll

### Zoom
- ⌘+/⌘−: zoom in/out
- ⌘0: zoom to custom level

### File and View
- ⌘O: open file, ⌘S: save
- ⌘Tab/Shift+⌘Tab: next/previous tab
- ⌘T: split view, ⌘F: search
- F5: refresh view
- ⌘Shift+E: export image (PNG/SVG)

### Track
- ⌘Shift+Up/Down: move track up/down
- ⌘Shift+C: set track color
- ⌘Shift+H: set track height
- ⌘Shift+B/P/L: display as bar/point/line

### Feature Editing (editable lanes only)
- ⌥Left: move feature left, ⌥Right: move feature right
- ⌥Down: shrink start, ⌥Up: expand end

## Undo/Redo Command System

- CommandBase: abstract base with Undo()/Redo()
- CommandAdd, CommandDelete, CommandEdit, CommandReplace
- Managed through ManagerEdit

## Version Info

- Single source: AssemblyInfo.cs (AssemblyVersion, FileVersion, InformationalVersion)
- Title bar: reads AssemblyInformationalVersion via reflection
- Introduction tab: version via m_runVersion Run element

## Known Fixes to Preserve

1. ScrollViewer Focusable="False" in DocMap.xaml (prevents keyboard focus stealing)
2. Sub-pixel feature skip bypass when selected (PnlMapLane.cs)
3. Cross-drive relative path fallback in ManagerWorkspace.GetRelativePath()
4. 4 broken CommandBinding lines in MainWindow.xaml commented out (test code)
