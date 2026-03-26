# Migration Map: WPF v1.1.11 -> AvaloniaUI

WPF source: `Metascope_v1.1.11/metascope-main/`
Avalonia target: `outputs/MetaScope/`

Status legend: `portable` (no/minimal changes), `adapt` (API swap needed), `rewrite` (significant rework), `replace` (use different library/approach), `drop` (not needed), `done` (ported + verified, add date)

## App Entry & Shell

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| App.xaml / App.xaml.cs | App.axaml / App.axaml.cs | adapt | Remove AssemblyResolve (no embedded DLL), Avalonia AppBuilder |
| MainWindow.xaml / .cs | MainWindow.axaml / .cs | rewrite | AvalonDock layout -> Dock.Avalonia, RoutedCommands -> ICommand/ReactiveCommand |
| Settings.cs | Settings.cs | adapt | WPF Settings API -> custom or Avalonia settings |
| Properties/AssemblyInfo.cs | MetaScope.csproj | adapt | Version info moves to csproj in SDK-style projects |

## Data Layer (Utility/Data/) -- mostly portable

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| DataFeature.cs | Models/DataFeature.cs | adapt | `Brush` field -> `IBrush` (or decouple: store color, resolve brush in view layer) |
| DataType.cs | Models/DataType.cs | portable | Pure data structures + algorithms, no UI dependency |
| DataFile.cs | Models/DataFile.cs | portable | File I/O, no UI dependency |
| DataBookmark.cs | Models/DataBookmark.cs | portable | Pure data class |

## Readers (Utility/Reader/) -- portable

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| ReaderGff.cs | Services/ReaderGff.cs | portable | System.IO only, no WPF dependencies |
| ReaderSetting.cs | Services/ReaderSetting.cs | portable | Key=Value parser, pure I/O |
| ReaderBookmark.cs | Services/ReaderBookmark.cs | portable | Pure I/O |

## Command/Undo System (Utility/Command/) -- portable

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| CommandBase.cs | Services/Command/CommandBase.cs | portable | Abstract Undo/Redo, no UI |
| CommandAdd.cs | Services/Command/CommandAdd.cs | portable | |
| CommandDelete.cs | Services/Command/CommandDelete.cs | portable | |
| CommandEdit.cs | Services/Command/CommandEdit.cs | portable | |
| CommandReplace.cs | Services/Command/CommandReplace.cs | portable | |

## Error Handling (Utility/Error/) -- portable

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| ErrorMessage.cs | Services/Error/ErrorMessage.cs | portable | |
| ExceptionAssertion.cs | Services/Error/ExceptionAssertion.cs | portable | |
| ExceptionInvalidArgument.cs | Services/Error/ExceptionInvalidArgument.cs | portable | |
| ExceptionInvalidFormat.cs | Services/Error/ExceptionInvalidFormat.cs | portable | |
| ExceptionVugmap.cs | Services/Error/ExceptionVugmap.cs | portable | |

## Logger -- portable

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| Logger.cs | Services/Logger.cs | portable | File-based logging, no UI |

## Manager Classes (Utility/) -- mixed

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| ManagerData.cs | Services/ManagerData.cs | portable | Data orchestration, no direct UI |
| ManagerWorkspace.cs | Services/ManagerWorkspace.cs | done 2026-03-24 | MainWindow refs replaced with events/delegates; Path.GetRelativePath for macOS |
| ManagerEdit.cs | Services/ManagerEdit.cs | portable | Undo/redo stack management |
| ManagerBookmark.cs | Services/ManagerBookmark.cs | portable | Bookmark data management |
| ManagerBrush.cs | Services/ManagerBrush.cs | rewrite | WPF `SolidColorBrush`/`Brush` -> Avalonia `IBrush`/`ImmutableSolidColorBrush` |
| ManagerPen.cs | Services/ManagerPen.cs | rewrite | WPF `Pen` -> Avalonia `IPen`/`Pen` |
| ManagerLabel.cs | Services/ManagerLabel.cs | rewrite | WPF `FormattedText` -> Avalonia `FormattedText` (different API) |
| ManagerLine.cs | Services/ManagerLine.cs | adapt | WPF geometry -> Avalonia geometry |
| ManagerRectangle.cs | Services/ManagerRectangle.cs | adapt | WPF Rect -> Avalonia Rect |
| AppSetting.cs | Services/AppSetting.cs | adapt | %AppData% -> cross-platform path resolution |
| Constant.cs | Services/Constant.cs | adapt | Color constants: WPF Color -> Avalonia Color |
| SvgExporter.cs | Services/SvgExporter.cs | done 2026-03-24 | Stub with public API; WPF Visual tree walk removed; DoFormat kept; needs Avalonia reimplementation |

## Utility Helpers -- mixed

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| UtilityMath.cs | Services/UtilityMath.cs | portable | Pure math |
| UtilityString.cs | Services/UtilityString.cs | portable | Pure string ops |
| UtilityFile.cs | Services/UtilityFile.cs | adapt | Possible Windows path assumptions |
| UtilityMessage.cs | Services/UtilityMessage.cs | rewrite | WPF MessageBox -> Avalonia MessageBox or custom dialog |
| UtilityWindow.cs | Services/UtilityWindow.cs | rewrite | WPF Window manipulation -> Avalonia Window APIs |

## Rendering Controls (Window/Pnl*) -- rewrite

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| PnlMap.cs | Controls/PnlMap.cs | done 2026-03-24 | Panel + PnlMapOverlay child for selection/drag rendering; events/delegates for MainWindow decoupling; PnlMapLane stub created |
| PnlMapLane.cs | Controls/PnlMapLane.cs | done 2026-03-24 | Immediate-mode rendering; WPF Panel->Control; retained-mode children removed; sub-pixel skip bypass preserved; dialog stubs for Phase 6 |
| PnlMapRuler.cs | Controls/PnlMapRuler.cs | done 2026-03-24 | Updated to use PnlMap constants directly; removed temp PnlMapConstants/PnlMapLaneConstants |

## Dialogs (Window/Dialog*) -- rewrite XAML, adapt code-behind

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| DialogFileOpen.xaml/.cs | Views/DialogFileOpen.axaml/.cs | rewrite | Async file loading, read-only detection |
| DialogBookmarkAdd.xaml/.cs | Views/DialogBookmarkAdd.axaml/.cs | rewrite | |
| DialogChangeType.xaml/.cs | Views/DialogChangeType.axaml/.cs | rewrite | |
| DialogFeatureAdd.xaml/.cs | Views/DialogFeatureAdd.axaml/.cs | rewrite | |
| DialogFeatureAddSize.xaml/.cs | Views/DialogFeatureAddSize.axaml/.cs | rewrite | |
| DialogFeatureEdit.xaml/.cs | Views/DialogFeatureEdit.axaml/.cs | rewrite | |
| DialogFeatureOpacity.xaml/.cs | Views/DialogFeatureOpacity.axaml/.cs | rewrite | |
| DialogFeatureOperation.xaml/.cs | Views/DialogFeatureOperation.axaml/.cs | rewrite | Score filtering |
| DialogIntegrationOperation.xaml/.cs | Views/DialogIntegrationOperation.axaml/.cs | rewrite | |
| DialogLaneOperation.xaml/.cs | Views/DialogLaneOperation.axaml/.cs | rewrite | |
| DialogPositionTo.xaml/.cs | Views/DialogPositionTo.axaml/.cs | rewrite | |
| DialogSearch.xaml/.cs | Views/DialogSearch.axaml/.cs | rewrite | |
| DialogSetHeight.xaml/.cs | Views/DialogSetHeight.axaml/.cs | rewrite | |
| DialogSetScale.xaml/.cs | Views/DialogSetScale.axaml/.cs | rewrite | |
| DialogShortcuts.xaml/.cs | Views/DialogShortcuts.axaml/.cs | rewrite | |
| DialogTest.xaml/.cs | Views/DialogTest.axaml/.cs | rewrite | |
| DialogZoomTo.xaml/.cs | Views/DialogZoomTo.axaml/.cs | rewrite | |

## Document & Property Panels (Window/) -- rewrite

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| DocMap.xaml/.cs | Views/DocMap.axaml/.cs | done 2026-03-24 | UserControl (was DocumentContent); LayoutTransformControl for scale; ScrollBar ValueChanged; PointerMoved for splitter; DragDrop for file drop |
| PropertyFeature.cs | ViewModels/PropertyFeature.cs | adapt | Property grid -> MVVM binding |
| PropertyFeatureGroup.cs | ViewModels/PropertyFeatureGroup.cs | adapt | |
| PropertyFeatureSelected.cs | ViewModels/PropertyFeatureSelected.cs | adapt | |
| PropertyGridOrderer.cs | ViewModels/PropertyGridOrderer.cs | adapt | WPF PropertyGrid -> custom or Avalonia equivalent |
| PropertySomething.cs | ViewModels/PropertySomething.cs | adapt | |
| PropertyVugmap.cs | ViewModels/PropertyVugmap.cs | adapt | |
| ComboBoxPopup.cs | Controls/ComboBoxPopup.cs | rewrite | WPF Popup -> Avalonia Popup |

## ColorPicker (Window/ColorPicker/) -- replace

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| ColorDialog.xaml/.cs | (use Avalonia color picker) | replace | Use existing Avalonia community control |
| ColorPicker.xaml/.cs | (use Avalonia color picker) | replace | |

## AvalonDock Library -- replace entirely

| WPF Source | Avalonia Target | Status | Notes |
|-----------|----------------|--------|-------|
| AvalonDock/ (62 files) | Dock.Avalonia (NuGet) | replace | Do NOT port AvalonDock. Use existing Avalonia docking library. |

## Summary

| Category | Files | Portable | Adapt | Rewrite | Replace | Drop |
|----------|-------|----------|-------|---------|---------|------|
| Data/Models | 4 | 3 | 1 | 0 | 0 | 0 |
| Readers | 3 | 3 | 0 | 0 | 0 | 0 |
| Commands | 5 | 5 | 0 | 0 | 0 | 0 |
| Errors | 5 | 5 | 0 | 0 | 0 | 0 |
| Logger | 1 | 1 | 0 | 0 | 0 | 0 |
| Managers | 12 | 4 | 4 | 3 | 0 | 0 |
| Utilities | 5 | 2 | 1 | 2 | 0 | 0 |
| Rendering | 3 | 0 | 0 | 3 | 0 | 0 |
| Dialogs | 17 | 0 | 0 | 17 | 0 | 0 |
| Panels/Props | 8 | 0 | 6 | 2 | 0 | 0 |
| ColorPicker | 2 | 0 | 0 | 0 | 2 | 0 |
| App/Shell | 4 | 0 | 2 | 1 | 0 | 0 |
| AvalonDock | 62 | 0 | 0 | 0 | 1 | 0 |
| **Total** | **131** | **23** | **14** | **28** | **3** | **0** |
