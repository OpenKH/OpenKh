using OpenKh.Tools.BbsMapStudio.Models;
using System.Collections.Generic;

namespace OpenKh.Tools.BbsMapStudio.Interfaces
{
    interface ISpawnPointController
    {
        List<SpawnPointModel> SpawnPoints { get; }
        SpawnPointModel CurrentSpawnPoint { get; }
        string SelectSpawnPoint { get; set; }
    }
}
