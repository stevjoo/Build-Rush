//using UnityEditor.Timeline;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelSelectionData", menuName = "Game Data/LevelSelectionData")]
public class LevelSelectionData : ScriptableObject
{
    public int levelID;
    public string levelName;
    public string levelJson;
    public Sprite levelThumbnail;
    public bool isLocked = true;
    public bool isCompleted = false;
    public int timer;
    public int matchCount;
}
