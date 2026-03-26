# Gotchas & Pitfalls

Append-only list of pitfalls discovered during porting. Read this before debugging rendering or UI issues.

---

## Rendering

### 1. ScrollViewer Focusable must be False
**Source**: DocMap.xaml (WPF v1.1.11)
**Problem**: ScrollViewer steals keyboard focus from the map control, breaking arrow-key navigation.
**Fix**: Set `Focusable="False"` on any ScrollViewer wrapping PnlMap.

### 2. Sub-pixel feature skip bypass when selected
**Source**: PnlMapLane.cs (WPF v1.1.11)
**Problem**: The rendering optimization that skips sub-pixel-width features must be disabled when the current OR previous feature is selected. Otherwise, selected features can disappear at certain zoom levels.
**Fix**: Check selection state before applying the sub-pixel skip. See original PnlMapLane.cs rendering loop.

### 3. Feature highlight spans all lanes
**Source**: PnlMap.cs (WPF v1.1.11)
**Problem**: The blue vertical selection overlay (`CLR_LANE_SELECTION`) must span start~end across ALL lanes, not just the lane containing the selected feature.
**Fix**: Draw the highlight at the PnlMap level, not PnlMapLane level.

---

## File Paths

### 4. Cross-drive relative path fallback is unnecessary on macOS
**Source**: ManagerWorkspace.GetRelativePath()
**Problem**: WPF version has fallback logic for cross-drive paths (C: vs D:). macOS has no drive letters.
**Fix**: Use `Path.GetRelativePath()` (.NET 5+) directly. Remove drive-letter logic.

---

## WPF Legacy

### 5. Four broken CommandBinding lines in MainWindow
**Source**: MainWindow.xaml (WPF v1.1.11), lines commented out
**Problem**: Test code that was never removed. These are `CommandBinding` entries that reference nonexistent commands.
**Fix**: Do NOT port these. They are dead code.

### 6. AssemblyResolve handler for embedded AvalonDock DLL
**Source**: App.xaml.cs (WPF v1.1.11)
**Problem**: WPF version embeds AvalonDock.dll as a resource and loads it via `AppDomain.AssemblyResolve`. This pattern is unnecessary with NuGet packages.
**Fix**: Remove the AssemblyResolve handler entirely. Reference Dock.Avalonia via NuGet.

---

## Data Model

### 7. Score NaN fallback behavior
**Source**: DataFeature.cs
**Problem**: When score is NaN (half-precision), the rendering falls back to +1.0 for '+' strand and -1.0 for '-' strand. This affects lane height calculations.
**Fix**: Preserve this fallback exactly. It determines default vertical positioning.

### 8. string.Intern() for m_strSource deduplication
**Source**: DataFeature.cs
**Problem**: GFF files can have 100K+ features with repeated source strings. Without `string.Intern()`, each copy consumes separate memory.
**Fix**: Keep `string.Intern()` calls — it works identically in .NET 8.

---

### 9. Avalonia 11 DrawingContext uses disposable pattern, not Pop()
**Source**: PnlMapRuler.cs port
**Problem**: WPF uses `dc.PushClip(geometry)` + `dc.Pop()` and `dc.PushTransform(transform)` + `dc.Pop()`. Avalonia 11 returns an `IDisposable` from `PushClip`/`PushTransform` and has no `Pop()` method.
**Fix**: Use `using( dc.PushClip(...) )` and `using( dc.PushTransform(...) )` blocks. Nesting `using` blocks replaces nested `Push`/`Pop` pairs.

### 10. Avalonia 11 PushClip accepts Rect, not RectangleGeometry
**Source**: PnlMapRuler.cs port
**Problem**: WPF `dc.PushClip(new RectangleGeometry(rect))` does not compile in Avalonia 11. The Avalonia `PushClip` overload accepts `Rect` or `RoundedRect`, not `Geometry` objects.
**Fix**: Pass `new Rect(...)` directly to `dc.PushClip()`.

### 11. Avalonia 11 PushTransform accepts Matrix, not Transform objects
**Source**: PnlMapRuler.cs port
**Problem**: WPF uses `dc.PushTransform(new TranslateTransform(...))` and `dc.PushTransform(new RotateTransform(...))`. Avalonia 11's `PushTransform` accepts a `Matrix` struct.
**Fix**: Use `dc.PushTransform(Matrix.CreateTranslation(x, y))` and `dc.PushTransform(Matrix.CreateRotation(radians))`. Convert degrees to radians with `degrees * Math.PI / 180.0`.

### 12. Avalonia Panel.Render() is sealed — use overlay Control child
**Source**: PnlMap.cs port
**Problem**: WPF `Panel` allows overriding `OnRender(DrawingContext)`. In Avalonia, `Panel.Render()` is sealed and cannot be overridden. Attempting `public override void Render(DrawingContext dc)` on a Panel subclass causes CS0239.
**Fix**: Create an internal `Control` subclass (e.g., `PnlMapOverlay`) that overrides `Render()`, add it as a child of the Panel with `IsHitTestVisible = false`. The overlay draws on top of all other children. The Panel calls `InvalidateVisual()` on the overlay child instead of on itself. This is particularly important for PnlMap's blue selection highlight which must span all lanes (gotcha #3).

<!-- APPEND NEW GOTCHAS BELOW THIS LINE -->
