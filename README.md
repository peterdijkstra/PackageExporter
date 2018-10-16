# PackageExporter
Easily export a folder of scripts and assets as a local Unity 2018 package for use with the new Package Manager.

## Features
* Exporting packages in a format that the Unity package manager seems to like.
* Some basic validation for the files inside the package and for fields of the package.json

## Dependencies
* [Odin Inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) (not included)

## How to use
1. Put the PackageExporter script somewhere in your project, ideally in an Editor folder.
2. Gather the assets and scripts you want to package in one folder (make sure you add an assembly definition for scripts). You can add subfolders to that folder, just make sure that the Package Exporter asset is at the root.
3. Add a Package Exporter asset to that folder (right click > Create > Package Exporter).
4. Fill in the fields and click Export Package!

The package(s) can now be found at the root of your project in a newly created folder called ExportedPackages. The tool will automatically create folders for each package, for each version, and also for each export of the same folder. It should not overwrite a previous export!

## Limitations
* AudioClips wouldn't import from a package, so you can't export packages with AudioClips in them.
* Currently the tool only spits out a package for local use. I don't know what is gonna happen if you'd put it on a server or something.
* I have not tested it on an OS other than macOS Mojave.
* I have not tested it with large files or folders.
* I have only tested it on Unity 2018.3, will probably work on an earlier version though. But maybe not.
* I have only tested it with scripts, a few textures and an AudioClip.
* I'm not sure what will happen if you move the Package Exporter asset around.
* I have no idea if this thing is performant, but it seems fast enough.
* Consider this thing untested!!
* Lastly, I'm not sure what will happen if you include this in a project that's meant for building. I made this so I can easily export my tools and extensions to a package, so in my usecase this script will most likely never find its way into an actual project.

## Special thanks
* [Sirenix](https://sirenix.net/) for making my Unity life ever so easier
* [LotteMakesStuff](https://gist.github.com/LotteMakesStuff) for the informative [write up](https://gist.github.com/LotteMakesStuff/6e02e0ea303030517a071a1c81eb016e) about the new package system!
