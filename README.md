No icon for now 🤷‍♂️

## Features
- Archives list
- Archive overview (with tags) & reader
- Configurable reader's base zoom and zoomed factor
- Search by name and/or tag.
- Permanent thumbnail caching
- Optional image caching
- Basic statistics
- Multiple servers

## Requirements

- Windows 10 Build 17763 (x86, x64 or ARM64)
- LANraragi v0.6.0-BETA.2

## Issues
- Thumbnail caching may crash, app should work normally in the next run.
- LANraragi API does not respond correctly when using API Key with No-Fun mode. It will crash when using no key or wrong one, already fixed in LANraragi repo, waiting for proper release.

## Installing
Sideload only for now. 

### First time
Unzip and right click `Add-AppDevPackage.ps1` > Run with powershell
This will add the self-signed certificate and install the app.

### Upgrade
Unzip and run the `LRReader_<version>_x86_x64_arm64.msixbundle` file, the app installer should pop-up.

## Screenshots

[Main View](.github/screenshots/01.png)<br>
[Search](.github/screenshots/02.png)<br>
[Archive View](.github/screenshots/03.png)<br>
[Reader](.github/screenshots/04.png)<br>
[Settings](.github/screenshots/05.png)<br>
