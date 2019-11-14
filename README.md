![Logo](.github/logo.png)

[<img src="https://github.com/Guerra24/LRReader/workflows/Continuous%20Delivery/badge.svg">](https://github.com/Guerra24/LRReader/actions?workflow=Continuous+Delivery)
[<img src="https://github.com/Guerra24/LRReader/workflows/Release%20Delivery/badge.svg">](https://github.com/Guerra24/LRReader/actions?workflow=Release+Delivery)

## Features
- Archives list.
- Search, the same one used in the webview.
- Show new archives only.
- Download archives.
- Archive overview (with tags) and reader.
- Configurable reader's base zoom and zoomed factor.
- Right-to-Left and Two pages modes.
- Bookmarks.
- Optional image caching.
- Multiple servers.
- Restart server's background worker.
- Clear "All New" flags.
- Download database.
- Fullscreen.

## Requirements

- Windows 10 1803 (x86, x64, ARM or ARM64)
- LANraragi v0.6.6+

## Installing
Sideload only for now. 

For stable check the [Releases page](https://github.com/Guerra24/LRReader/releases)

For nightly check the [Actions page](https://github.com/Guerra24/LRReader/actions?workflow=Continuous+Delivery)

### First time
Unzip and right click `Install.ps1` > Run with powershell<br>
This will add the self-signed certificate and install the app.

If you're using a local-hosted instance (e.g. LANraragi on Windows), you will need to run this command:<br>
`CheckNetIsolation loopbackexempt -a -n=Guerra24.LRReader_3fr0p4qst6948`<br>
To allow localhost access.

### Upgrade
Unzip and run the `LRReader.UWP_<version>_x86_x64_arm_arm64.appxbundle` file, the app installer should pop-up.

## Screenshots

![Main View](.github/screenshots/01.png)<br>
![Search](.github/screenshots/02.png)<br>
![Bookmarks](.github/screenshots/01_1.png)<br>
![Archive View](.github/screenshots/03.png)<br>
![Reader](.github/screenshots/04.png)<br>
![Reader two pages](.github/screenshots/04_1.png)<br>
![Settings](.github/screenshots/05.png)<br>
