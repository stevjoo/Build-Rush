using UnityEngine;

public class LevelController : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject playerObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            gridManager.SaveLevel("MyLevel");
            Debug.Log("Level saved!");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            gridManager.LoadLevel("MyLevel", playerObject.transform.position);
            Debug.Log("Level loaded!");
        }
    }
}
