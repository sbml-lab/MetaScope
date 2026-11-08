#!/bin/zsh
# Build a distributable MetaScope.app (+ zip) for macOS. Usage: macos/publish-mac.sh [osx-arm64|osx-x64]
set -euo pipefail
RID=${1:-osx-arm64}
export PATH="/opt/homebrew/opt/dotnet@8/bin:$PATH" DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
cd "$(dirname "$0")/.."
VER=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' MetaScope.csproj)
OUT=bin/Release/$RID; APP=$OUT/MetaScope.app; PUB=$OUT/publish
[ -d "$OUT" ] && trash "$OUT"	# previous build output only; sources are never touched
dotnet publish -c Release -r "$RID" --self-contained -o "$PUB"
mkdir -p "$APP/Contents/Resources"
mv -n "$PUB" "$APP/Contents/MacOS"
cp macos/Info.plist "$APP/Contents/"
cp macos/MetaScope.icns "$APP/Contents/Resources/"
# Sign in a temp dir: inside iCloud Drive codesign refuses with "detritus not allowed" (live Finder xattrs).
SIGN=$(mktemp -d)
ditto --norsrc --noextattr "$APP" "$SIGN/MetaScope.app"
codesign --force --deep --sign - "$SIGN/MetaScope.app"	# ad-hoc: no Developer ID available
codesign --verify --deep --strict "$SIGN/MetaScope.app"
ditto -c -k --keepParent "$SIGN/MetaScope.app" "$OUT/MetaScope-$VER-$RID.zip"
trash "$APP"; mv -n "$SIGN/MetaScope.app" "$APP"; trash "$SIGN"
echo "→ $APP"; echo "→ $OUT/MetaScope-$VER-$RID.zip"
