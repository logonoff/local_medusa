namespace local_medusa;

using System.Xml;
using static Helpers.PathInteractionHelper;

/// <summary>
/// Simple WPL parser
/// Can get absolute paths from a Windows Media playlist
/// Does not support smart playlists
/// </summary>
public class WPLParser
{
    private readonly XmlDocument _doc;
    private readonly string _path;

    public WPLParser(string path)
    {
        _path = path;
        _doc = new XmlDocument();
        _doc.LoadXml(File.ReadAllText(path));

        if (_doc.GetElementsByTagName("smartPlaylist").Count != 0)
        {
            throw new NotSupportedException("Smart playlist support not implemented!");
        }
    }

    /// <summary>
    /// Reads the songs from the WPL file.
    /// </summary>
    /// <returns>A list of absolute paths leading to the songs</returns>
    public List<string> GetSongs()
    {
        XmlNodeList songs = _doc.GetElementsByTagName("media");
        List<string> paths = new();
        // trailing backslash is required for proper full path conversion
        string baseDirectory = new FileInfo(_path).Directory.FullName + "\\";

        foreach (XmlNode song in songs)
        {
            string mediaSrc = song.Attributes["src"].Value;
            string path;

            // directly get full path 
            if (Path.IsPathRooted(mediaSrc))
            {
                path = mediaSrc;
            }
            else
            {
                path = Path.GetFullPath(baseDirectory + mediaSrc);
            }

            // convert to UNC path so that nadeko can read it and finally append
            paths.Add(UNCPath(path));
        }

        return paths;
    }
}
