![Logo](https://s3.guerra24.net/projects/lrr/logo.png)

[<img src="https://github.com/Guerra24/LRReader/workflows/Nightly/badge.svg">](https://github.com/Guerra24/LRReader/actions?query=workflow:Nightly)
[<img src="https://github.com/Guerra24/LRReader/workflows/Release%20Delivery/badge.svg">](https://github.com/Guerra24/LRReader/actions?query=workflow:"Release+Delivery")

## Features
- Archives list.
- Search.
- Archive overview and reader.
- Bookmarks.
- Multiple servers/profiles.
- Manage your server from within the app.

## Requirements

- Windows 10 1803 (x86, x64, ARM or ARM64)
- LANraragi v0.6.8+

## Installing
Sideload only for now. 

For stable check the [Releases page](https://github.com/Guerra24/LRReader/releases)

Unzip and right click `Install App.ps1` > Run with powershell<br>
This will add the self-signed certificate and install the app.

For nightly check the [Nightly page](https://s3.guerra24.net/projects/lrr/nightly/index.html)

Nightly is testing the AppInstaller format and uses auto-updates, if testing goes right it will replace the Install script.
You will need to install the cert first, [instructions here](https://github.com/Guerra24/LRReader/wiki/Certificate).

If you're using a local-hosted instance (e.g. LANraragi on Windows), you will need to run this command:<br>
`CheckNetIsolation loopbackexempt -a -n=Guerra24.LRReader_3fr0p4qst6948`<br>
To allow localhost access.

## Reader

The reader can be used with the keyboard and/or mouse.

Use the UP/DOWN keys to scroll vertically and RIGHT/LEFT to switch pages.<br>
Use SPACE to scroll by a distance and if at the bottom, changes to the next page.<br>
Use ESC to close the reader.<br>

Click on the left/right of the window to change pages.<br>
Scroll by clicking and dragging the page.<br>

## Screenshots

![Main View](https://s3.guerra24.net/projects/lrr/screenshots/01.png)<br>
![Search](https://s3.guerra24.net/projects/lrr/screenshots/02.png)<br>
![Bookmarks](https://s3.guerra24.net/projects/lrr/screenshots/01_1.png)<br>
![Archive View](https://s3.guerra24.net/projects/lrr/screenshots/03.png)<br>
![Reader](https://s3.guerra24.net/projects/lrr/screenshots/04.png)<br>
![Reader two pages](https://s3.guerra24.net/projects/lrr/screenshots/04_1.png)<br>
![Settings](https://s3.guerra24.net/projects/lrr/screenshots/05.png)<br>
