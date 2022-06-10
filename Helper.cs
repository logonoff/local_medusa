using Microsoft.Win32;

// <summary>
// A static class with a bunch of helpers
// </summary>
public static class Helper
{
    // <summary>
    // Returns a UNC (samba) path if it is a mapped drive, otherwise it returns the path unchanged
    // See https://stackoverflow.com/a/28540229
    // </summary>
    public static string UNCPath(string path)
    {
        if (!path.StartsWith(@"\\"))
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Network\\" + path[0]))
            {
                if (key != null)
                {
                    return key.GetValue("RemotePath").ToString() + path.Remove(0, 2).ToString();
                } // if
            } // using
        } // if
        return path;
    }
}
