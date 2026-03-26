# Migration Status

## Current Phase: Initial Port Complete

Version target: v2.0.0 (AvaloniaUI Mac port)

## Completed

- [x] Documentation structure created (CLAUDE.md, context/)
- [x] WPF v1.1.11 reference spec written (context/wpf-reference.md)
- [x] Class-by-class migration map created (context/migration-map.md)
- [x] Porting recipes documented (context/porting-recipes.md)
- [x] Known gotchas extracted (context/gotchas.md)
- [x] Avalonia project scaffolding (MetaScope.sln, MetaScope.csproj, App.axaml, Program.cs)
- [x] Port error classes (Services/Error/): 5 files
- [x] Port Logger + utilities (Services/): Logger, UtilityMath, UtilityString, UtilityFile
- [x] Port data layer (Models/): DataBookmark, DataFeature, DataType, DataFile
- [x] Port readers (Services/): ReaderGff, ReaderSetting, ReaderBookmark
- [x] Port command system (Services/Command/): CommandBase + CommandAdd/Delete/Replace/Edit
- [x] Port managers (Services/): ManagerData, ManagerEdit, ManagerBookmark, ManagerBrush, ManagerPen, ManagerWorkspace, AppSetting, Constant, SvgExporter (stub), UtilityMessage, UtilityWindow, RelayCommand
- [x] Port rendering controls (Controls/): PnlMap, PnlMapLane, PnlMapRuler, ComboBoxPopup
- [x] Port MainWindow with menus, keyboard shortcuts, commands, status bar, tab-based layout
- [x] Port DocMap (Views/): document container with split view, scrollbar
- [x] Port dialogs (Views/): 14 dialog windows
- [x] Port property panels (ViewModels/): PropertyFeature, PropertyFeatureGroup, PropertyFeatureSelected, PropertyGridOrderer, PropertySomething, PropertyVugmap

## Build Status

**82 files (63 .cs + 19 .axaml), ~18,300 lines — builds with 0 errors, 0 warnings**

## Remaining Work (Phase 7: Polish & Integration)

- [ ] Wire dialog invocations into MainWindow command handlers (TODO stubs)
- [ ] Implement MRU (most recently used) file lists in File menu
- [ ] Wire workspace save/restore (ManagerWorkspace ↔ MainWindow)
- [ ] Implement explorer TreeView population
- [ ] Implement edit history ListBox population
- [ ] Implement bookmark ListBox population
- [ ] Implement feature/feature-selected property display
- [ ] Implement temp workspace auto-save and crash recovery
- [ ] Replace ColorPicker with Avalonia equivalent (community control)
- [ ] Reimplement SvgExporter for Avalonia (currently stub)
- [ ] Resolve all remaining TODO comments in code
- [ ] Platform testing: macOS ARM64, verify file paths and rendering
- [ ] Add app icon (Assets/Icon.png)
- [ ] Publish as .app bundle

## Blockers

(none)

## Session Log

### 2026-03-24 — Initial Port Session
- Created documentation structure for Claude Code efficiency
- Analyzed full WPF source tree (22,300 lines, 89 files + 62 AvalonDock files)
- Classified all files: 23 portable, 14 adapt, 28 rewrite, 3 replace
- Installed .NET 8 SDK on macOS
- **Phase 1**: Scaffolded Avalonia project with Dock.Avalonia NuGet
- **Phase 2**: Ported 27 portable/adapt files (errors, logger, utilities, data models, readers, commands)
- **Phase 3**: Ported 12 manager/platform service files (ManagerBrush→IBrush, ManagerPen→IPen, AppSetting→cross-platform paths, etc.)
- **Phase 4**: Ported 3 rendering controls (PnlMap, PnlMapLane, PnlMapRuler) — WPF retained-mode → Avalonia immediate-mode rendering
- **Phase 5**: Ported MainWindow (full menu, 60+ ICommand properties, keyboard shortcuts, status bar) + DocMap
- **Phase 6**: Ported 14 dialogs + 6 property/ViewModel classes
- Final build: **0 errors, 0 warnings**, 82 files, ~18,300 lines
