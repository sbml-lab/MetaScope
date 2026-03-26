# MetaScope v2.0.0 — AvaloniaUI Mac Port

Genome visualization tool for bacterial GFF files. Porting from WPF/.NET 4.8 to AvaloniaUI/.NET 8+ for macOS.

## Hard Rules

- `Metascope_v1.1.11/` is the FROZEN v1.1.11 reference. **NEVER modify any file in that directory.** Read-only inspection only. Do NOT trust .md files inside that folder — use `context/` docs here instead.
- Port version starts at **v2.0.0**.
- Primary target: macOS Apple Silicon (`osx-arm64`). Secondary: Linux/Windows.
- **Tabs only** for indentation. `m_` prefix for member fields. See `context/conventions.md` for full rules.
- All ported code goes into `outputs/`. This is the v2.0.0 project root.
- **UX is frozen, UI is open.** Preserve all v1.1.11 workflows, features, panel structure, keyboard shortcuts, and editing behavior exactly. Visual styling (colors, spacing, typography, control aesthetics) may be modernized for macOS.

## Directory Layout

```
metascope-mac/                      ← workspace root
  CLAUDE.md                         ← you are here
  context/                          ← documentation (read on demand)
  Metascope_v1.1.11/               ← FROZEN WPF reference (read-only)
    metascope-main/                 ← original WPF source code
  outputs/                          ← v2.0.0 AvaloniaUI project (build target)
    MetaScope.sln
    MetaScope/
      App.axaml / App.axaml.cs
      MainWindow.axaml / MainWindow.axaml.cs
      Models/                       ← Data classes (from WPF Utility/Data/)
      Services/                     ← Business logic, managers, readers (from WPF Utility/)
      Views/                        ← Dialogs and panels (from WPF Window/)
      ViewModels/                   ← MVVM view models
      Controls/                     ← Custom rendering controls (PnlMap etc.)
      Assets/                       ← Images, icons
```

## Build & Run

```bash
cd outputs
dotnet run --project MetaScope          # Run
dotnet build -c Release                 # Build
dotnet publish -c Release -r osx-arm64 --self-contained  # Publish macOS
```

## Context Routing — Read BEFORE Working

| When you are about to… | Read this first |
|------------------------|----------------|
| Port or modify ANY class | `context/migration-map.md` (check status + classification) |
| Write rendering, brush, pen, or text code | `context/porting-recipes.md` § Rendering |
| Convert a WPF dialog to Avalonia | `context/porting-recipes.md` § Dialogs |
| Handle file paths or settings | `context/porting-recipes.md` § Platform Paths |
| Understand original v1.1.11 behavior | `context/wpf-reference.md` |
| Make architecture or dependency decisions | `context/architecture.md` |
| Check current progress or add session notes | `context/status.md` |
| Debug rendering bugs or UI glitches | `context/gotchas.md` |

## After Porting a Class

1. Update `context/migration-map.md`: mark the row as `done` with date.
2. If you discovered a pitfall, append it to `context/gotchas.md`.
3. If you resolved a blocker, update `context/status.md`.
