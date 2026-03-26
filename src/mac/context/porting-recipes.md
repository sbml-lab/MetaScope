# Porting Recipes: WPF → AvaloniaUI

Reusable conversion patterns. Apply these directly — do not re-derive from scratch.

---

## § Rendering

### Custom Drawing Controls

WPF uses `DrawingVisual` + `RenderOpen()`. Avalonia uses `Control.Render()` override.

```csharp
// WPF pattern
var visual = new DrawingVisual();
using (DrawingContext dc = visual.RenderOpen())
{
    dc.DrawRectangle(brush, pen, rect);
}

// Avalonia equivalent
public class PnlMap : Control
{
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(brush, pen, rect);
    }

    // Force redraw (replaces InvalidateVisual in WPF):
    // Call InvalidateVisual() — same name in Avalonia but triggers Render() override
}
```

### Brush / IBrush

```csharp
// WPF
SolidColorBrush brush = new SolidColorBrush(Colors.Red);
brush.Freeze(); // performance optimization

// Avalonia
IBrush brush = new ImmutableSolidColorBrush(Colors.Red);
// ImmutableSolidColorBrush is already immutable — no Freeze() needed
// Use IBrush as the field/parameter type, not SolidColorBrush
```

Field type change: `Brush m_bshColor` → `IBrush m_bshColor`

### Pen / IPen

```csharp
// WPF
Pen pen = new Pen(brush, 1.0);
pen.Freeze();

// Avalonia
IPen pen = new Pen(brush, 1.0);
// Avalonia Pen is already lightweight, no Freeze() equivalent
// Use IPen as the field/parameter type
```

### FormattedText

```csharp
// WPF
var ft = new FormattedText(
    text,
    CultureInfo.CurrentCulture,
    FlowDirection.LeftToRight,
    new Typeface("Segoe UI"),
    12,
    Brushes.Black);

// Avalonia
var ft = new FormattedText(text, CultureInfo.CurrentCulture,
    FlowDirection.LeftToRight,
    new Typeface("San Francisco", FontStyle.Normal, FontWeight.Normal),
    12,
    Brushes.Black);
// Note: constructor signature is similar but Typeface constructor differs
// macOS default font: "San Francisco" or use Typeface.Default
```

### Color Constants

```csharp
// WPF
Color.FromArgb(255, 100, 150, 200)
// → same in Avalonia
Color.FromArgb(255, 100, 150, 200)

// WPF named colors
System.Windows.Media.Colors.Red
// → Avalonia named colors
Avalonia.Media.Colors.Red
```

### Geometry

```csharp
// WPF StreamGeometry
var geo = new StreamGeometry();
using (var ctx = geo.Open())
{
    ctx.BeginFigure(startPoint, true, true);
    ctx.LineTo(point1, true, false);
}

// Avalonia StreamGeometry — identical API
var geo = new StreamGeometry();
using (var ctx = geo.Open())
{
    ctx.BeginFigure(startPoint, true); // Note: fewer overload parameters
    ctx.LineTo(point1, true);
}
```

---

## § Dialogs

### XAML → AXAML Conversion Template

1. File extension: `.xaml` → `.axaml`
2. Root namespace: `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"` → `xmlns="https://github.com/avaloniaui"`
3. Design namespace: add `xmlns:d="http://schemas.microsoft.com/expression/blend/2008"`
4. Window class: `<Window>` stays `<Window>` (same in Avalonia)

```xml
<!-- WPF -->
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

<!-- Avalonia -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d">
```

### Dialog ShowDialog Pattern

```csharp
// WPF
var dialog = new DialogSearch();
dialog.Owner = this;
bool? result = dialog.ShowDialog();

// Avalonia (async)
var dialog = new DialogSearch();
var result = await dialog.ShowDialog<bool?>(this);
// ShowDialog is async in Avalonia — caller must be async
```

### Common AXAML Property Differences

| WPF | Avalonia | Notes |
|-----|----------|-------|
| `Visibility="Collapsed"` | `IsVisible="False"` | No Visibility enum, use bool |
| `TextBlock.TextTrimming` | `TextBlock.TextTrimming` | Same |
| `ToolTip="text"` | `ToolTip.Tip="text"` | Attached property syntax |
| `MouseLeftButtonDown` | `PointerPressed` | Unified pointer events |
| `MouseMove` | `PointerMoved` | |
| `MouseWheel` | `PointerWheelChanged` | |
| `KeyDown` | `KeyDown` | Same |
| `Focusable="False"` | `Focusable="False"` | Same |

---

## § Platform Paths

### Settings & Workspace Directory

```csharp
// WPF (Windows only)
string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
// → C:\Users\<user>\AppData\Roaming

// Avalonia (cross-platform)
string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
// macOS → /Users/<user>/.config  (XDG) or ~/Library/Application Support depending on runtime
// Recommended: explicit macOS path
string configDir = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Application Support", "MetaScope")
    : Path.Combine(appData, "MetaScope");
```

### Path Separator

Never hardcode `\\`. Always use `Path.Combine()` and `Path.DirectorySeparatorChar`.

### Relative Path Handling

WPF `ManagerWorkspace.GetRelativePath()` has a cross-drive fallback. On macOS there are no drive letters — simplify to `Path.GetRelativePath()` (.NET 5+).

---

## § Commands & Input

### RoutedCommand Replacement

```csharp
// WPF
public static RoutedCommand CmdSave = new RoutedCommand();
// + CommandBinding in XAML

// Avalonia — use ICommand (ReactiveCommand or relay command)
public ICommand CmdSave { get; }
// Initialize in ViewModel:
CmdSave = ReactiveCommand.Create(DoSave);
// Bind in AXAML:
// <Button Command="{Binding CmdSave}" />
```

### Keyboard Shortcuts (macOS modifier mapping)

Windows→macOS: `Ctrl`→`⌘ Cmd`, `Alt`→`⌥ Option`, `Shift`→`Shift`. NumPad keys are dropped (most Mac keyboards lack numpads); use ⌥+Arrow alternatives only.

```csharp
// WPF
InputBindings.Add(new KeyBinding(CmdSave, Key.S, ModifierKeys.Control));

// Avalonia — use KeyBindings in AXAML with macOS-native gestures
// <Window.KeyBindings>
//   <KeyBinding Gesture="Cmd+S" Command="{Binding CmdSave}" />
// </Window.KeyBindings>
//
// Avalonia maps "Cmd" to ⌘ on macOS and to Ctrl on Windows/Linux automatically
// when using KeyModifiers.Meta in code:
// new KeyBinding { Gesture = new KeyGesture(Key.S, KeyModifiers.Meta), Command = CmdSave }
```

---

## § MessageBox

```csharp
// WPF
MessageBox.Show("Error occurred", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

// Avalonia — no built-in MessageBox. Options:
// 1. MessageBox.Avalonia NuGet package
// 2. Custom dialog window
// Recommended: simple custom dialog for consistency
```

---

## § Clipboard

```csharp
// WPF
Clipboard.SetText(text);
string text = Clipboard.GetText();

// Avalonia (async, requires TopLevel)
await TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(text);
string text = await TopLevel.GetTopLevel(this)?.Clipboard?.GetTextAsync();
```
