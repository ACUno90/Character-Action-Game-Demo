using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonFuncs : MonoBehaviour
{
    public void Resume()
    {
        GameManger.Instance.startUnPause();
        GameManger.Instance.PlayClickSound();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManger.Instance.startUnPause();
        GameManger.Instance.PlayClickSound();
    }

    public void Quit()
    {
        GameManger.Instance.PlayClickSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}
