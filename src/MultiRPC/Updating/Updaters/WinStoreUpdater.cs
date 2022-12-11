#if _UWP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using TinyUpdate.Core;
using TinyUpdate.Core.Update;
using Windows.Services.Store;
using JetBrains.Annotations;
using SemVersion;
using TinyUpdate.Github;

namespace MultiRPC.Updating;

//TODO: Test
public class WinStoreUpdater : UpdateClient
{
    private readonly StoreContext _storeContext;
    public WinStoreUpdater(StoreContext? storeContext = null) : base(null!)
    {
        _storeContext = storeContext ?? StoreContext.GetDefault();
    }

    public override async Task<UpdateInfo?> CheckForUpdate(bool grabDeltaUpdates = true)
    {
        //Right now we don't want to use this so adding this here
        return new UpdateInfo(AppMetadata.ApplicationVersion);

        var updates = await _storeContext.GetAppAndOptionalStorePackageUpdatesAsync();
        if (!updates.Any())
        {
            return new UpdateInfo(AppMetadata.ApplicationVersion);
        }

        return new UpdateInfo(AppMetadata.ApplicationVersion, updates.Select(x => new WinStoreReleaseEntry(x)).ToArray());
    }

    public override Task<ReleaseNote?> GetChangelog(ReleaseEntry entry)
    {
        //TODO: Add
        return Task.FromResult((ReleaseNote?)null);
        throw new NotImplementedException();
    }

    public override Task<bool> DownloadUpdate(UpdateInfo updateInfo, Action<double>? progress = null) => 
        DownloadUpdate(updateInfo.Updates.Select(x => (StorePackageUpdate)x.Tag), progress);

    public override Task<bool> DownloadUpdate(ReleaseEntry releaseEntry, Action<double>? progress = null) => 
        DownloadUpdate(new[] { (StorePackageUpdate)releaseEntry.Tag }, progress);
    
    private async Task<bool> DownloadUpdate(IEnumerable<StorePackageUpdate> updates, Action<double>? progress = null)
    {
        IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> downloadOperation =
            _storeContext.RequestDownloadStorePackageUpdatesAsync(updates);

        if (progress != null)
        {
            downloadOperation.Progress = (_, storeProgress) => progress.Invoke(storeProgress.TotalDownloadProgress);
        }
        
        var result = await downloadOperation.AsTask();
        return result.OverallState == StorePackageUpdateState.Completed;
    }

    public override Task<bool> ApplyUpdate(UpdateInfo updateInfo, Action<double>? progress = null) => 
        ApplyUpdate(updateInfo.Updates.Select(x => (StorePackageUpdate)x.Tag), progress);

    public override Task<bool> ApplyUpdate(ReleaseEntry releaseEntry, Action<double>? progress = null) => 
        ApplyUpdate(new[] { (StorePackageUpdate)releaseEntry.Tag }, progress);
    
    private async Task<bool> ApplyUpdate(IEnumerable<StorePackageUpdate> updates, Action<double>? progress = null)
    {
        IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> installOperation =
            _storeContext.RequestDownloadAndInstallStorePackageUpdatesAsync(updates);

        if (progress != null)
        {
            installOperation.Progress = (_, storeProgress) =>  progress.Invoke(storeProgress.TotalDownloadProgress);
        }
        
        var result = await installOperation.AsTask();
        return result.OverallState == StorePackageUpdateState.Completed;
    }
}

public class WinStoreReleaseEntry : ReleaseEntry
{
    //Give it a random hash so it doesn't complain
    public WinStoreReleaseEntry(StorePackageUpdate storePackageUpdate)
        : base("3IQDNKKQC5CJZ8922SYPIR7DTO184VFHO94HUTMK1KRF1WWEZ4",
            "a", 0, false, null!, "a", tag: storePackageUpdate) { }

    private WinStoreReleaseEntry(string sha256, string filename, long filesize, bool isDelta, SemanticVersion version,
        string folderPath, SemanticVersion? oldVersion = null, object? tag = null, int? stagingPercentage = null) 
        : base(sha256, filename, filesize, isDelta, version, folderPath, oldVersion, tag, stagingPercentage) { }
}
#endif