using OpenKh.Tools.Kh2MapStudio.Models;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MapStudio.Interfaces
{
    interface ISpawnPointController
    {
        List<SpawnPointModel> SpawnPoints { get; }
        SpawnPointModel CurrentSpawnPoint { get; }
        string SelectSpawnPoint { get; set; }
    }
}
