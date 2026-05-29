using TinyUpdate.Core;
using TinyUpdate.Core.Update;

namespace MultiRPC.Updating;

//TODO: Make
public class UpdateManager
{
    private readonly UpdateClient _updateClient;
    public UpdateManager(UpdateClient updateClient)
    {
        _updateClient = updateClient;
    }
    
    public async Task CheckForUpdate()
    {
        var updates = await _updateClient.CheckForUpdate(true) 
                      ?? await _updateClient.CheckForUpdate(false);

        if (updates != null)
        {
            
        }
    }

    public Task<ReleaseNote?> GetChangelog(ReleaseEntry entry)
    {
        return Task.FromResult((ReleaseNote?)null);
    }

    public Task<bool> DownloadUpdate(UpdateInfo updateInfo, Action<double>? progress = null)
    {
        return Task.FromResult(false);
    }
    
    public Task<bool> ApplyUpdate(UpdateInfo updateInfo, Action<double>? progress = null)
    {
        return Task.FromResult(false);
    }
}