using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Text.RegularExpressions;

/// <summary>
/// Validates your Unity Package Manager package and exports it for you!
/// </summary>
[CreateAssetMenu, HideMonoScript]
public class PackageExporter : SerializedScriptableObject
{
    // make it look like it is part of the scriptable object, sneaky
    [InlineProperty, HideLabel, HideReferenceObjectPicker, SerializeField, ShowInInspector]
    private PackageJsonStub stub;

    // gets called when the scriptable object gets created
    private void Reset() 
    {
        stub = new PackageJsonStub();
    }

    #region Directories
    /// <summary>
    /// Please do not touch
    /// </summary>
    private DirectoryInfo _workingDirectory;
    /// <summary>
    /// Get the working directory, i.e. the directory where this asset lives in
    /// </summary>
    private DirectoryInfo GetWorkingDirectory()
    {
        if (_workingDirectory != null)
            return _workingDirectory;

        var directoryPath = AssetDatabase.GetAssetPath(this);
        if (directoryPath.IsNullOrWhitespace())
            return null;
        directoryPath = directoryPath.Remove(directoryPath.LastIndexOf('/'));
        directoryPath = Application.dataPath.Replace("Assets", directoryPath);

        _workingDirectory = new DirectoryInfo(directoryPath);
        return _workingDirectory;
    }

    /// <summary>
    /// Get the directory where we want to put our exported packages in
    /// </summary>
    private DirectoryInfo GetPackageExportFolder()
    {
        var exportFolderPath = Application.dataPath.Replace("Assets", "ExportedPackages");

        var exportFolder = new DirectoryInfo(exportFolderPath);
        if (exportFolder.Exists == false)
            exportFolder.Create();

        return exportFolder;
    }
    #endregion
    #region Validation
    // The following should also be pretty self explanatory
    // There's probably a more intelligent way of doing these things but for now this is alright
    private bool HasFiles()
    {
        if (GetWorkingDirectory() == null)
            return false;

        return GetWorkingDirectory()
                .EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Where(x => x.Name != $"{this.name}.asset" || x.Name != $"{this.name}.asset.meta")
                .Count() > 0;
    }

    private bool HasScripts()
    {
        if (GetWorkingDirectory() == null)
            return false;

        return GetWorkingDirectory().EnumerateFiles("*.cs", SearchOption.AllDirectories).Count() > 0;
    }

    private bool HasAsmDef()
    {
        if (GetWorkingDirectory() == null)
            return false;
        // If there are no scripts, we don't need an assembly definition
        // so just return true
        if (HasScripts() == false)
            return true;

        return GetWorkingDirectory().EnumerateFiles("*.asmdef", SearchOption.AllDirectories).Count() > 0;
    }

    private bool HasAudioClips()
    {
        if (GetWorkingDirectory() == null)
            return false;

        var directoryPath = AssetDatabase.GetAssetPath(this);
        directoryPath = directoryPath.Remove(directoryPath.LastIndexOf('/'));
        return AssetDatabase.FindAssets("t:AudioClip", new string[] { directoryPath }).Length > 0;
    }

    private bool ValidPackage()
    {
        if (HasFiles() == false)
            return false;
        if (stub.ValidName == false)
            return false;
        if (stub.ValidDisplayName == false)
            return false;
        if (stub.ValidSemanticVersion == false)
            return false;
        if (HasAudioClips())
            return false;
        if (HasScripts() && !HasAsmDef())
            return false;

        return true;
    }

    private Color ValidColor => ValidPackage() ? Color.green : Color.red;

    /// <summary>
    /// This draws some validation message boxes
    /// </summary>
    [OnInspectorGUI, PropertySpace(32)]
    private void ValidationGUI()
    {
        if (GetWorkingDirectory() == null)
            return;

        if (HasFiles() == false)
            SirenixEditorGUI.ErrorMessageBox("Package is empty!", true);
        if (stub.ValidName == false)
            SirenixEditorGUI.ErrorMessageBox("Empy name is not valid.", true);
        if (stub.ValidDisplayName == false)
            SirenixEditorGUI.ErrorMessageBox("Empy display name is not valid.", true);
        if (stub.ValidSemanticVersion == false)
            SirenixEditorGUI.ErrorMessageBox("Semantic version is not valid. Expected is MAJOR.MINOR.PATCH[-something].\nFor example: 1.2.3 or 0.2.5-prerelease.", true);
        if (HasAudioClips())
            SirenixEditorGUI.ErrorMessageBox("Package contains AudioClips, those won't import unfortunately.", true);
        if (HasScripts() && !HasAsmDef())
            SirenixEditorGUI.ErrorMessageBox("This package has scripts but is missing an Assembly Definition.", true);

        // yay
        if (ValidPackage())
            SirenixEditorGUI.InfoMessageBox("This package is ready for packaging!", true);
    }
    #endregion
    #region Package exporting
    /// <summary>
    /// Shorthand for a pretty Json from the JsonUtility
    /// </summary>
    private string Json => JsonUtility.ToJson(stub, true);

    /// <summary>
    /// Log json for preview
    /// </summary>
    [Button(ButtonSizes.Large),
    LabelText("Preview package.json in Console"),
    HorizontalGroup("Buttons", 0f, 10, 10),
    PropertySpace(32)]
    private void LogJson() => Debug.Log(Json);

    /// <summary>
    /// Exports the package!
    /// </summary>
    [Button(ButtonSizes.Large),
    GUIColor(nameof(ValidColor)),
    EnableIf(nameof(ValidPackage)),
    HorizontalGroup("Buttons", 0f, 10, 10),
    PropertySpace(32)]
    private void ExportPackage()
    {
        // the button gets disabled if package is not valid, but putting this check in just in case
        if (ValidPackage() == false)
        {
            Debug.LogError($"Package {stub.name} is not valid.", this);
            return;
        }

        // get the export folder path
        var exportFolderPath = GetPackageExportFolder().FullName;
        // find or create a folder for our package with its display name
        var packageFolder = new DirectoryInfo($"{exportFolderPath}/{stub.displayName}");
        if (packageFolder.Exists == false)
            packageFolder.Create();
        // find or create a folder for our semantic version of the package
        var versionsFolder = new DirectoryInfo($"{packageFolder.FullName}/{stub.version}");
        if (versionsFolder.Exists == false)
            versionsFolder.Create();

        // time to figure out how we're gonna call the build folder
        DirectoryInfo pkgExportFolder = null;
        // check if folders exist that just have a number
        var exportVersions = versionsFolder.GetDirectories().Select(x =>
            {
                // there might be a more intelligent way of doing this but hey it works
                int ver = 0;
                if (int.TryParse(x.Name, out ver))
                    return ver;
                else
                    return -1;
            }).Where(x => x >= 0);
        // default build number is 1
        var newExportVersion = 1;
        // If there is any build folder in there though, we'll grab the highest number and add 1
        if (exportVersions.Count() > 0)
            newExportVersion = exportVersions.Max() + 1;
        // make the build folder!
        pkgExportFolder = versionsFolder.CreateSubdirectory(newExportVersion.ToString("0000"));
        // and also make a folder with the name of our package
        // this will be actual package
        pkgExportFolder = pkgExportFolder.CreateSubdirectory(stub.name);
        // copy everything!
        CopyDirectoryAndFilesTo(GetWorkingDirectory(), pkgExportFolder);
        // this asset also got copied over, so delete it and its meta file
        // NOTE: ForEach is an extension provided by Odin
        pkgExportFolder.GetFiles().Where(x => x.Name == $"{this.name}.asset" || x.Name == $"{this.name}.asset.meta").ForEach(x => x.Delete());
        // finally, create the package.json file
        File.WriteAllText(pkgExportFolder.FullName + "/package.json", Json);

        // celebrate!!!
        Debug.Log($"Finished creating package <b>{stub.displayName}</b> at {pkgExportFolder.FullName}!", this);
    }

    /// <summary>
    /// Recursive function to copy files and directories from one directory to another directory
    /// </summary>
    private void CopyDirectoryAndFilesTo(DirectoryInfo fromDir, DirectoryInfo toDir)
    {
        // Copy files
        foreach (var file in fromDir.GetFiles())
            file.CopyTo($"{toDir.FullName}/{file.Name}");

        // Copy directories
        foreach (var dir in fromDir.GetDirectories())
        {
            var sub = toDir.CreateSubdirectory(dir.Name);
            // yay recursion
            CopyDirectoryAndFilesTo(dir, sub);
        }
    }
    #endregion
    /// <summary>
    /// This class serves as a stub for the package.json and has some validation features
    /// Also if we use the JsonUtility on the scriptableobject we get loads of stuff we don't want
    /// </summary>
    private class PackageJsonStub
    {
        #region package.json
        // the following things should be pretty self explanatory
        [HideLabel,
        Title("Name", null, TitleAlignments.Left, false, true),
        OnValueChanged(nameof(FixName)), // Can only use lowercase for package names
        GUIColor(nameof(NameFieldColor)),
        SuffixLabel("Required")]
        public string name = "com.companyname.packagename";

        [HideLabel,
        Title("Display name", null, TitleAlignments.Left, false, true),
        GUIColor(nameof(DisplayNameFieldColor)),
        SuffixLabel("Required")]
        public string displayName = "Package Name";

        [HideLabel,
        Title("Semantic version", null, TitleAlignments.Left, false, true),
        GUIColor(nameof(VersionFieldColor)),
        SuffixLabel("Required")]
        public string version = "0.0.1-prerelease";

        [HideLabel,
        Title("Unity version", null, TitleAlignments.Left, false, true)]
        public string unity = "2018.3";

        [HideLabel,
        Title("Description", null, TitleAlignments.Left, false, true),
        TextArea]
        public string description = "This is a sweet package!";

        [ListDrawerSettings(Expanded = true),
        HideLabel,
        Title("Keywords", null, TitleAlignments.Left, false, true)]
        public string[] keywords = { "Nice" };

        [HideLabel,
        Title("Category", null, TitleAlignments.Left, false, true)]
        public string category = "My packages";

        // TODO: add dependencies? Would require this stub to be replaced with a more proper package.json generator..
        #endregion
        #region Stub validation
        // better than red!
        internal Color Pink => new Color32(255, 182, 193, 255);

        // Name
        internal bool ValidName => !name.IsNullOrWhitespace();
        internal Color NameFieldColor => ValidName ? Color.white : Pink;

        private void FixName()
        {
            name = name.ToLower();
        }

        // Display name
        internal bool ValidDisplayName => !displayName.IsNullOrWhitespace();
        internal Color DisplayNameFieldColor => ValidDisplayName ? Color.white : Pink;

        // Semantic version
        internal bool ValidSemanticVersion => Regex.IsMatch(version, SemverRegex);
        internal Color VersionFieldColor => ValidSemanticVersion ? Color.white : Pink;

        // https://github.com/semver/semver/issues/232#issuecomment-405596809
        private const string SemverRegex = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
        #endregion
    }
}