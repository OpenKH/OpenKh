using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Models;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces
{
    interface ISpawnPointController
    {
        List<SpawnPointModel> SpawnPoints { get; }
        SpawnPointModel CurrentSpawnPoint { get; }
        string SelectSpawnPoint { get; set; }
    }
}
