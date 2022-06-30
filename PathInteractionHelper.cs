namespace local_medusa
{
    using Microsoft.Win32;

    /// <summary>
    /// A static class with a bunch of helpers relating to files and folders
    /// </summary>
    public static class PathInteractionHelper
    {
        private static readonly int MaxRecursionDepth = 5;

        /// <summary>
        /// Returns a list of all the folders in a directory
        /// (including the one provided in the parameter)
        /// </summary>
        /// <param name="path">the path that will be searched</param>
        /// <returns></returns>
        public static string[] GetAllFolders(string path)
        {
            return _GetAllFolders(new(path), MaxRecursionDepth);
        }

        /// <summary>
        /// Helper for GetAllFolders
        /// </summary>
        /// <param name="path">path that will be searched</param>
        /// <param name="currDepth">current recursion depth</param>
        /// <returns></returns>
        private static string[] _GetAllFolders(DirectoryInfo directory, int currDepth)
        {
            DirectoryInfo[] subdirs = directory.GetDirectories();
            string[] folders = { directory.FullName };

            if (currDepth <= 0) // out of recursion
            {
                return folders;
            }
            else
            {
                foreach (DirectoryInfo subdir in subdirs)
                {
                    folders = folders.Concat(_GetAllFolders(subdir, currDepth - 1)).ToArray();
                }

                return folders;
            }
        }

        /// <summary>
        /// Returns a UNC (samba) path if it is a mapped drive, otherwise it returns the path unchanged
        /// See https://stackoverflow.com/a/28540229
        /// </summary>
        /// <param name="path">path of the file/folder to be converted</param>
        /// <returns>the converted, full UNC path</returns>
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
    }
}
