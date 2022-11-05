using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    public void OnTutorial()
    {
        StartCoroutine(LoadTutorial());
    }
    IEnumerator LoadTutorial()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Tutorial");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    public void OnPlay()
    {
        StartCoroutine(LoadPlay());
    }
    IEnumerator LoadPlay()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("island");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    public void OnMenu()
    {
        StartCoroutine(LoadMenu());
    }
    IEnumerator LoadMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("menu");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    public void OnExit()
    {
        Application.Quit();
    }
}
