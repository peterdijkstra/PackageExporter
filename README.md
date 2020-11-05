# PackageExporter
Easily export a folder of scripts and assets as a local Unity 2018 package for use with the new Package Manager.

## Deprecated! Archived!
Since I do not intend to work on this package anymore, I'm putting it in archived mode. I made this back when git support wasn't available for the Unity Package Manager (or at the very least it was an iffy process), when it made more sense to upload your packages to an external package repository. Now, you can do that far easier through git or OpenUPM. For a good tutorial, read [this](https://www.patreon.com/posts/25070968).

I suppose there's use for a Unity extensions that automatically does most of this work, and maybe I'll remake this then using UIToolkit instead of Odin.

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

In Unity 2018.3 you can add a package to another project by using the + button on the Package Manager window.

<img src="docs/import.gif" width="600">

This will add the package path to the manifest file in the Packages folder in your project root

<img src="docs/path.jpg" width="800">

As you can see it points to a local full path. This is not ideal if you're not the only developer on the project. [It does seem](https://forum.unity.com/threads/other-registries-than-unitys-own-already-work-nice.533691/) that you can use your own npm registry. However that's beyond the scope of this tool (at least for now).

## Limitations
* AudioClips wouldn't import from a package, so you can't export packages with AudioClips in them.
* Currently the tool only spits out a package for local use. I don't know what is gonna happen if you'd put it on a server or something. The generated package.json is easy enough to edit by hand, though.
* I have not tested it on an OS other than macOS Mojave.
* I have not tested it with large files or folders.
* I have only tested it on Unity 2018.3, will probably work on an earlier version though. But maybe not?
* I have only tested it with scripts, a few textures and an AudioClip.
* I'm not sure what will happen if you move the Package Exporter asset around.
* I have no idea if this thing is performant, but it seems fast enough.
* Consider this thing untested!!
* Lastly, I'm not sure what will happen if you include this in a project that's meant for deployment. I made this so I can easily export my tools and extensions to a package, so I can use those things in other projects. In my usecase this script will never find its way into an actual project.

## Todo
* Make the validation more intelligent and expandable.
* Test some more
* Maybe add dependencies to the package.json generation
* Unity editor version field validation?

## Screenshots
<img src="docs/valid.jpg" width="400"><img src="docs/invalid.jpg" width="400">

## Special thanks
* [Sirenix](https://sirenix.net/) for making my Unity life ever so easier
* [LotteMakesStuff](https://gist.github.com/LotteMakesStuff) for the informative [write up](https://gist.github.com/LotteMakesStuff/6e02e0ea303030517a071a1c81eb016e) about the new package system!
