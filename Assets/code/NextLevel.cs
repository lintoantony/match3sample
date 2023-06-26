using UnityEngine;

public class NextLevel : MonoBehaviour {

    public void OnNextLevelClick(){
        Debug.Log("OnNextLevelClick");

        GameManagerScript.Instance.generateNewLevel();
    }
}
