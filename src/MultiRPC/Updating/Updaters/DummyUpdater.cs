using SemVersion;
using TinyUpdate.Core;
using TinyUpdate.Core.Update;

namespace MultiRPC.Updating;

//TODO: Make
public class DummyUpdater : UpdateClient
{
    public DummyUpdater() : base(new DummyApplier())
    {
    }

    public override Task<UpdateInfo?> CheckForUpdate(bool grabDeltaUpdates = true)
    {
        throw new NotImplementedException();
    }

    public override Task<ReleaseNote?> GetChangelog(ReleaseEntry entry)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DownloadUpdate(ReleaseEntry releaseEntry, Action<double>? progress = null)
    {
        throw new NotImplementedException();
    }
}

public class DummyApplier : IUpdateApplier
{
    public Task<bool> ApplyUpdate(ApplicationMetadata applicationMetadata, ReleaseEntry entry, Action<double>? progress = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ApplyUpdate(ApplicationMetadata applicationMetadata, UpdateInfo updateInfo, Action<double>? progress = null)
    {
        throw new NotImplementedException();
    }

    public void RemoveOldBuilds(ApplicationMetadata applicationMetadata)
    {
    }

    public string? GetApplicationPath(string applicationFolder, SemanticVersion? version)
    {
        return null;
    }

    public string Extension => ".dum";
    public bool ShouldContainLoader { get; }
    public bool ShouldRemoveOldBuilds { get; }
}