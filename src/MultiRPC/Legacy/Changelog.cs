using TinyUpdate.Core;

namespace MultiRPC.Legacy;

public class Changelog
{
    public static ReleaseNote[]? MakeReleaseNoteFromChangelog()
    {
        if (!File.Exists(FileLocations.ChangelogFileLocalLocation))
        {
            return null;
        }

        var releaseNotes = new ReleaseNote[43];
        var counter = -1;
        var content = string.Empty;
        foreach (var line in File.ReadLines(FileLocations.ChangelogFileLocalLocation))
        {
            if (line.StartsWith("---") && content != string.Empty)
            {
                counter++;
                releaseNotes[counter] = new ReleaseNote(content.TrimEnd(), NoteType.Plain);
                content = string.Empty;
            }
            content += line + "\r\n";
        }
        counter++;
        releaseNotes[counter] = new ReleaseNote(content.TrimEnd(), NoteType.Plain);
        if (counter != 42)
        {
            Array.Resize(ref releaseNotes, counter + 1);
        }
        Array.Reverse(releaseNotes);

        return releaseNotes;
    }
}