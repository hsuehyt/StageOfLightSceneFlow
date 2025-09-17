using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager I;

    [Header("Fade")]
    [SerializeField] CanvasGroup fader;
    [SerializeField] float fadeTime = 0.35f;

    [Header("Auto Boot (optional)")]
    [SerializeField] bool autoLoadOnStart = true;
    [SerializeField] string firstScene = "Scene00";
    [SerializeField] float bootDelay = 0f; // seconds (e.g., show a logo before boot)

    string bootstrapSceneName;

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

    void Start()
    {
        if (autoLoadOnStart && !string.IsNullOrEmpty(firstScene))
            StartCoroutine(CoAutoBoot());
    }

    IEnumerator CoAutoBoot()
    {
        if (bootDelay > 0f) yield return new WaitForSeconds(bootDelay);
        // Only boot if the target scene isn¡¦t already loaded
        if (!SceneManager.GetSceneByName(firstScene).isLoaded)
            yield return CoNext(firstScene);
    }

    // Public API
    public void Next(string sceneName) => StartCoroutine(CoNext(sceneName));
    public void Boot(string sceneName) => StartCoroutine(CoNext(sceneName)); // alias

    IEnumerator CoNext(string sceneName)
    {
        yield return Fade(1f);

        // Load additively if not already loaded
        var scn = SceneManager.GetSceneByName(sceneName);
        if (!scn.isLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Make target active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        // Unload everything except the target scene, the bootstrap, and DontDestroyOnLoad
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
        float start = fader ? fader.alpha : 0f, t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            if (fader) fader.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        if (fader) fader.alpha = target;
    }
}
