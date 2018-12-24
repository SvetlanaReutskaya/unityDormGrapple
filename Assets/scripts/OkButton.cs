using UnityEngine;
using UnityEngine.SceneManagement;

public class OkButton : MonoBehaviour
{
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}