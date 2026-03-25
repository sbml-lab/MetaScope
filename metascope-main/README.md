# MetaScope

A genome visualization tool for bacterial GFF files, built with C#/WPF.

## Features

- Interactive genomic map with pan, zoom, and feature selection
- GFF / GZ / ZIP file support with automatic read-only mode for large files
- Memory-optimized for datasets with millions of features (~1GB savings on 7M features)
- Single executable deployment (~1MB, no external dependencies)
- Workspace save/restore with crash recovery
- Docking panel layout (AvalonDock)

## Build & Run

- **IDE**: Visual Studio 2022 / Visual Studio 2026
- **Framework**: .NET Framework 4.8
- **Solution**: `VugMap.sln`
- **Output**: `VugMap\bin\Release\MetaScope.exe`

## Download

See [Releases](../../releases) for the latest `MetaScope.exe`.

## Version

Current: v1.1.11

## License

All rights reserved.
