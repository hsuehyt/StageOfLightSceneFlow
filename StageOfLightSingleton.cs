using UnityEngine;

public class StageOfLightSingleton : MonoBehaviour
{
    static StageOfLightSingleton _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // only the master copy survives
        }
        else
        {
            // Already have a live instance (from SceneMaster), so kill this duplicate
            Destroy(gameObject);
        }
    }
}
