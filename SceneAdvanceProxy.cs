using UnityEngine;

public class SceneAdvanceProxy : MonoBehaviour
{
    [SerializeField] string nextScene = "Scene02";
    public void Next()
    {
        if (GameFlowManager.I != null)
            GameFlowManager.I.Next(nextScene);
        else
            Debug.LogWarning("GameFlowManager not found.");
    }
}
