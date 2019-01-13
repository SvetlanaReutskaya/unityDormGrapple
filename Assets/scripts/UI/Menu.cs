using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void StartLevel(int num)
    {
        var go = GameObject.Find("LevelManager");

        var manager = go.GetComponent<LevelManager>();

        manager.SetLevel(num);

        SceneManager.LoadScene("GameScreen");
    }
}
