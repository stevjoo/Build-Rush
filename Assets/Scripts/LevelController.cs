using UnityEngine;

public class LevelController : MonoBehaviour
{
    public GridManager gridManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            gridManager.SaveLevel("MyLevel");
            Debug.Log("Level saved!");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            gridManager.LoadLevel("MyLevel");
            Debug.Log("Level loaded!");
        }
    }
}
