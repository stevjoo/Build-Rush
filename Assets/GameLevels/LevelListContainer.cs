using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

[CreateAssetMenu(fileName = "LevelList", menuName = "Game Data/LevelListContainer")]
public class LevelListContainer : ScriptableObject
{
    public List<LevelSelectionData> levels;
    public LevelSelectionData GetLevelByID(int levelID)
    {
        return levels.Find(level => level.levelID == levelID);
    }
}
