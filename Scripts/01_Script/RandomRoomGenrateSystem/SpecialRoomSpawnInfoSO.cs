using System;
using UnityEngine;

[Serializable]
public struct SpawnRoomInfo
{
    public int shopSpawnCount;
    public int bounsSpawnCount;
    public int healingSpawnCount;
    public int bossSpawnCount;
}

namespace BIS
{
    [CreateAssetMenu(fileName = "SpecialRoomSpawnInfoSO", menuName = "SO/LJS/SpecialRoomSpawnInfoSO")]
    public class SpecialRoomSpawnInfoSO : ScriptableObject
    {
        public SpawnRoomInfo SpawnInfo;
    }
}
