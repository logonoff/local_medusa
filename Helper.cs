using Microsoft.Win32;

// <summary>
// A static class with a bunch of helpers
// </summary>
public static class Helper
{

    public static string UNCPath(string path)
    {
        // prepend a backslash if input is a UNC path that only starts with one
        if (path.StartsWith(@"\") && !path.StartsWith(@"\\"))
        {
            return _UNCPath("\\" + path);
        }
        else
        {
            return _UNCPath(path);
        }
    }

    // <summary>
    // Returns a UNC (samba) path if it is a mapped drive, otherwise it returns the path unchanged
    // See https://stackoverflow.com/a/28540229
    // </summary>
    private static string _UNCPath(string path)
    {

        if (!path.StartsWith(@"\\"))
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Network\\" + path[0]))
            {
                if (key != null)
                {
                    return key.GetValue("RemotePath").ToString() + path.Remove(0, 2).ToString();
                }
            }
        }
        return path;
    }
}
