using System.Collections.ObjectModel;
using MultiRPC.Rpc;

namespace MultiRPC.Extensions;

public static class PresenceExt
{
    public static void CheckName(this ObservableCollection<Presence> profiles, Presence profile)
    {
        if (profiles.Any(x => profile.Name == x.Name))
        {
            var count = 0;
            var name = profile.Name;
            var spaceInd = name.LastIndexOf(' ');
            if (spaceInd != -1
                && int.TryParse(name[(spaceInd + 1)..], out count))
            {
                name = name[..spaceInd];
            }
            
            while (profiles.Any(x => name + $" {count}" == x.Name))
            {
                count++;
            }
            profile.Name = name + $" {count}";
        }
    }
}