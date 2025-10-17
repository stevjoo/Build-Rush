using UnityEditor.Timeline;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSelectionData", menuName = "Game Data/LevelSelectionData")]
public class LevelSelectionData : ScriptableObject
{
    public int levelID;
    public string levelName;
    public string sceneName;
    public Sprite levelThumbnail;
    public bool isLocked = true;
    public bool isCompleted;
    public int timer;
    public int matchCount;
}
