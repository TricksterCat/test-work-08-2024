using Runtime.Controllers;
using UnityEngine;

namespace Runtime.Data.Level
{
    public class LevelModel
    {
        public LevelModel(LevelController prefab)
        {
            Prefab = prefab;
        }

        public LevelController Prefab { get; }
    }
}