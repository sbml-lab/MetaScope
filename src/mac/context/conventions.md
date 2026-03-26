# Coding Conventions

## Indentation & Formatting

- **Tabs only** for indentation (no spaces)
- **Tabs** for column alignment of variable declarations and assignments
- No trailing whitespace
- UTF-8 encoding, LF line endings (macOS/Unix standard)

## Naming

- **Fields**: `m_` prefix for member variables (e.g., `m_nStart`, `m_strSource`)
- **Type prefixes** (Hungarian-lite, preserved from WPF codebase):
  - `n` = int (`m_nStart`)
  - `str` = string (`m_strSource`)
  - `b` = byte or bool (`m_bStrand`)
  - `us` = ushort (`m_usScore`)
  - `bsh` = Brush/IBrush (`m_bshColor`)
  - `lst` = List/LinkedList (`m_lstFeature`)
- **Methods**: PascalCase, `Do` prefix for action methods (e.g., `DoFeatureAdd`, `DoTitleSet`)
- **Constants**: `UPPER_SNAKE_CASE` (e.g., `CLR_LANE_SELECTION`, `N_INDEXSPAN`)
- **Classes**: PascalCase, category prefix (e.g., `DataFeature`, `ManagerBrush`, `ReaderGff`)
- **XAML/AXAML files**: PascalCase matching class name

## File Organization

- One primary class per file
- File name matches class name
- Namespace matches folder path: `MetaScope.Models`, `MetaScope.Services`, `MetaScope.Views`

## MVVM Pattern

- Views (`.axaml`): UI layout only, minimal code-behind
- ViewModels: business logic, expose properties via INotifyPropertyChanged or ReactiveUI
- Models: pure data classes, no UI dependencies
- Services: stateless or singleton managers

## Language

- Code comments: English
- Docs and status notes: Korean is acceptable (bilingual project)
- Commit messages: English

## Git

- Branch naming: `feature/`, `fix/`, `refactor/` prefixes
- Commit messages: imperative mood, concise first line
- Do not commit build artifacts (`bin/`, `obj/`)
