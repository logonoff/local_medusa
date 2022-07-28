namespace local_medusa.Helpers;

using Microsoft.Win32;

/// <summary>
/// A static class with a bunch of helpers relating to files and folders
/// </summary>
public static class PathInteractionHelper
{
    private static readonly int MaxRecursionDepth = 5;

    /// <summary>
    /// Find all the folders in a directory up to and including a depth of 5
    /// (including the one provided in the parameter)
    /// </summary>
    /// <param name="path">the path that will be searched</param>
    /// <returns>an array of all the folders in a directory (including the one provided in the parameter)</returns>
    public static string[] GetAllFolders(string path)
    {
        return GetAllFolders(new(path), MaxRecursionDepth);
    }

    /// <summary>
    /// Helper for public GetAllFolders
    /// </summary>
    /// <param name="path">path that will be searched</param>
    /// <param name="depth">current recursion depth</param>
    /// <returns>an array of all the folders in a directory</returns>
    private static string[] GetAllFolders(DirectoryInfo directory, int depth)
    {
        DirectoryInfo[] subdirs = directory.GetDirectories();
        string[] folders = { directory.FullName };

        if (depth > 0) // recursive case
        {
            foreach (DirectoryInfo subdir in subdirs)
            {
                folders = folders.Concat(GetAllFolders(subdir, depth - 1)).ToArray();
            }
        }

        return folders;
    }

    /// <summary>
    /// Returns a UNC (samba) path if it is a mapped drive, otherwise it returns the path unchanged.
    /// This only works on Windows!!
    /// See https://stackoverflow.com/a/28540229
    /// </summary>
    /// <param name="path">path of the file/folder to be converted</param>
    /// <returns>the converted, full UNC path</returns>
    #pragma warning disable CA1416 // Validate platform compatibility

    public static string UNCPath(string path)
    {
        // prepend a backslash if input is a UNC path that only starts with one
        if (path.StartsWith(@"\") && !path.StartsWith(@"\\"))
        {
            path = "\\" + path;
        }

        // do the actual thing
        if (!path.StartsWith(@"\\"))
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey("Network\\" + path[0]);

            if (key != null)
            {
                return key.GetValue("RemotePath").ToString() + path.Remove(0, 2).ToString();
            }
        }
        return path;
    }
    #pragma warning restore CA1416 // Validate platform compatibility
}
