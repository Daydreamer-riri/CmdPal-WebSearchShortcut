## Prerequisites

### System Requirements
- Windows 11 or Windows 10 version 2004 (build 19041) or newer
- x64 or ARM64 processor

### Required Software
1. **PowerToys with Command Palette**
   - Install from Microsoft Store: https://apps.microsoft.com/detail/xp89dcgq3k6vld
   - Includes Microsoft Edge WebView2 Runtime
   - Available for both Windows 11 and Windows 10

## Installation Steps

### Step 1: Install PowerToys
1. Open Microsoft Store
2. Search for "PowerToys" or use the direct link above
3. Click "Get" or "Install"
4. Wait for installation to complete

### Step 2: Install Extension
1. Download the MSIX package `WebSearchShortcut_x.x.x.0_x64(arm64).msix`
2. Right-click the MSIX file
3. Select "Install" from the context menu
4. Follow the installation prompts
5. The extension will automatically register with Command Palette

### Step 3: Activate Command Palette
1. Open PowerToys
2. Navigate to Command Palette tool
3. Enable Command Palette if not already active
4. Set your preferred hotkey

## Testing the Extension

### Basic Functionality Test
1. Press your Command Palette hotkey
2. In the search box, type "Reload"
3. Select "Reload, Reload Command Palette extensions" and press Enter
4. Type "google"(or bing, wiki, npm ...) in the search box
5. Select the google item and press Enter
6. Type anything what you want to search
7. Press Enter to open browser and search your keyword

### Expected Behavior
- Extension should fetch a random riddle from the internet
- Default shortcuts(Google, bing, npm, YouTube...) should display clearly in the Command Palette
- Pressing Enter should show the answer in a popup dialog
- Each new activation should fetch a different riddle

## Troubleshooting

### Extension Not Appearing
- Restart PowerToys
- Reload Command Palette extensions (Step 3 in testing)
- Verify the MSIX package installed successfully in Windows Settings > Apps

### Installation Issues
- Run as Administrator when installing MSIX
- Ensure Windows is up to date
- Check Windows Event Viewer for detailed error messages

## What to Test

1. **Installation Process**: Smooth MSIX installation
2. **Extension Discovery**: Appears in Command Palette after reload

## Reporting Issues

If you encounter problems, please report:
- Windows version and build number
- PowerToys version
- Exact error messages
- Steps to reproduce the issue
- Screenshots if applicable
