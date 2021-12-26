using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionsManager : MonoBehaviour
{
    public static SceneTransitionsManager instance;

    public Canvas[] canvases;

    private Transform screenCover;
    private bool coverScreen = false;
    private Vector2 offScreenPos;

    private GameObject loadingIcon;

    private string nextScene = "";

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
            return;
        }
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnFinishedLoadingScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnFinishedLoadingScene;
    }

    void OnFinishedLoadingScene(Scene _scene, LoadSceneMode _mode)
    {
        foreach (var _c in canvases)
        {
            _c.worldCamera = Camera.main;
        }
    }

    private void Start()
    {
        screenCover = transform.Find("Canvas").Find("ScreenCover");
        loadingIcon = transform.Find("Canvas").Find("LoadingIcon").gameObject;
        loadingIcon.SetActive(false);

        offScreenPos = new Vector2(0f, Screen.height * 3f);
    }

    public void LoadScene(string _sceneName)
    {
        nextScene = _sceneName;
        StartCoroutine(SceneChange(0.25f));
    }

    public void LoadSceneWithHoldTime(string _sceneName, float _holdTime)
    {
        nextScene = _sceneName;
        StartCoroutine(SceneChange(_holdTime));
    }

    public void LoadSceneWithoutReference(string _sceneName)
    {
        nextScene = _sceneName;
        StartCoroutine(SceneChangeWithoutReference());
    }

    private void FixedUpdate()
    {
        float _interpolation = 10f * Time.deltaTime;
        if (coverScreen)
        {
            Vector2 _newPos;
            _newPos.x = Mathf.SmoothStep(screenCover.localPosition.x, transform.position.x, _interpolation);
            _newPos.y = Mathf.SmoothStep(screenCover.localPosition.y, transform.position.y, _interpolation);
            screenCover.localPosition = _newPos;
        }
        else
        {
            Vector2 _newPos;
            _newPos.x = Mathf.SmoothStep(screenCover.localPosition.x, offScreenPos.x, _interpolation);
            _newPos.y = Mathf.SmoothStep(screenCover.localPosition.y, offScreenPos.y, _interpolation);
            screenCover.localPosition = _newPos;
        }
    }

    IEnumerator SceneChange(float _holdTime)
    {
        coverScreen = true;
        loadingIcon.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        if (nextScene == "DeckCreatorScene" || nextScene == "LoginScene")
            StatsManagerScript.instance.ShowStatsBar(false);
        else
        {
            StatsManagerScript.instance.UpdateUI();
            StatsManagerScript.instance.ShowStatsBar(true);
        }
        SceneManager.LoadScene(nextScene);
        yield return new WaitForSeconds(_holdTime);
        nextScene = "";
        coverScreen = false;
        yield return new WaitForSeconds(0.1f);
        loadingIcon.SetActive(false);
    }

    IEnumerator SceneChangeWithoutReference()
    {
        coverScreen = true;
        loadingIcon.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene(nextScene);
        yield return new WaitForSeconds(0.25f);
        nextScene = "";
        coverScreen = false;
        yield return new WaitForSeconds(0.1f);
        loadingIcon.SetActive(false);
    }
}