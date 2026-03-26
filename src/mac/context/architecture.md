# Architecture Decisions

## Platform & Framework

- **Runtime**: .NET 8 LTS (or latest LTS at time of development)
- **UI Framework**: AvaloniaUI 11.x
- **Primary target**: macOS (Apple Silicon / osx-arm64)
- **Secondary targets**: Linux (x64), Windows (x64) -- validate but not primary
- **Rendering backend**: Skia (Avalonia default, cross-platform)

## Project Type

- SDK-style csproj (`net8.0`)
- Single project (not split into class libraries unless complexity demands it)
- MVVM pattern using ReactiveUI or CommunityToolkit.Mvvm (TBD)
- All ported code lives in `outputs/` (solution root: `outputs/MetaScope.sln`)

## Docking Library

**Decision needed**: Replace AvalonDock with an Avalonia-compatible docking library.

Options:
1. **Dock.Avalonia** (wieslaw-soltes) -- most mature, MIT license
2. **Custom TabControl** -- simpler, fewer features, no external dependency
3. **Avalonia.Controls.PanAndZoom** + custom layout -- if docking is less critical

The WPF version uses AvalonDock for: document tabs (map files), dockable property panels, floating windows. Evaluate which features are actually needed for the Mac version.

## Rendering Strategy

The WPF version uses `DrawingVisual` + `DrawingContext` for the genomic map.

In Avalonia, the equivalent is overriding `Render(DrawingContext context)` on a custom `Control`. Key API mappings:

| WPF | Avalonia |
|-----|----------|
| `DrawingVisual.RenderOpen()` | `override Render(DrawingContext)` |
| `DrawingContext.DrawRectangle()` | `DrawingContext.DrawRectangle()` (same name) |
| `DrawingContext.DrawLine()` | `DrawingContext.DrawLine()` |
| `DrawingContext.DrawText(FormattedText)` | `DrawingContext.DrawText(FormattedText)` (different constructor) |
| `DrawingGroup` | Not needed (direct render) |
| `VisualBrush` | `VisualBrush` (limited support) |

Performance note: Avalonia's Skia backend should handle bacterial genome rendering (max ~13M bp, typically <100K visible features) without issues. If performance problems arise, consider `WriteableBitmap` for tile-based rendering.

## File Path Strategy (cross-platform)

| WPF (Windows) | Avalonia (macOS) |
|---------------|-----------------|
| `%AppData%\MetaScope\` | `~/Library/Application Support/MetaScope/` |
| `Path.Combine()` with `\` | `Path.Combine()` (works cross-platform) |
| Drive letters (C:, D:) | Unix paths (no drive letters) |

Use `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` which resolves correctly per platform.

## Deployment

- macOS: `.app` bundle via `dotnet-msbuild` or `Avalonia.BuildTasks`
- Consider: DMG packaging, code signing, notarization for distribution
- Single-file publish: `cd outputs && dotnet publish -r osx-arm64 --self-contained -p:PublishSingleFile=true`

## Dependencies (planned)

| Package | Purpose |
|---------|---------|
| Avalonia | UI framework |
| Avalonia.Desktop | Desktop platform support |
| Avalonia.Themes.Fluent | Default theme |
| Dock.Avalonia (TBD) | Docking panel layout |
| ReactiveUI.Avalonia (TBD) | MVVM framework |

## UI vs UX Boundary

**UX (frozen — match v1.1.11 exactly):**
- All workflows: file open, edit, save, workspace restore
- Panel structure: docking layout, document tabs, property panels
- Keyboard shortcuts and navigation (arrow keys, ⌘+Scroll zoom, ⌥+Arrow feature editing)
- Feature selection, undo/redo, search, bookmarks
- Menu structure and dialog flow
- Read-only mode behavior (lock icon, edit guards)

**UI (open for modernization):**
- Theme and color palette (Fluent theme is the default)
- Typography (use macOS-native San Francisco where appropriate)
- Control styling: buttons, scrollbars, combo boxes, sliders
- Spacing, padding, border radius
- Icon set (replace Windows-style icons with macOS-appropriate ones)
- Dialog chrome (title bar, close/minimize buttons follow macOS conventions)
- High-DPI / Retina rendering improvements

When in doubt, preserve the v1.1.11 behavior and only change the visual surface.

## Not Porting

- AvalonDock source (62 files) -- use NuGet package instead
- `System.Windows.Forms.Integration` (WinForms host) -- not applicable
- `WindowInteropWrapper` / Win32 interop -- not applicable on macOS
- Embedded DLL loading (`AssemblyResolve`) -- use normal NuGet references
