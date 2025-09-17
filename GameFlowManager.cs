using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager I;
    [SerializeField] CanvasGroup fader;
    [SerializeField] float fadeTime = 0.35f;

    string bootstrapSceneName; // <¡X remember the scene that hosts the Timeline

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
            bootstrapSceneName = SceneManager.GetActiveScene().name; // e.g., "sceneMaster"
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Next(string sceneName) { StartCoroutine(CoNext(sceneName)); }

    IEnumerator CoNext(string sceneName)
    {
        yield return Fade(1f);
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        // Unload everything except the target scene and the bootstrap (master) scene
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.name != sceneName && s.name != bootstrapSceneName && s.name != "DontDestroyOnLoad")
                SceneManager.UnloadSceneAsync(s);
        }

        yield return Fade(0f);
    }

    IEnumerator Fade(float target)
    {
        float start = fader.alpha, t = 0;
        while (t < fadeTime) { t += Time.unscaledDeltaTime; fader.alpha = Mathf.Lerp(start, target, t / fadeTime); yield return null; }
        fader.alpha = target;
    }
}
