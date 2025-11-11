using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{

    #region Variables
    public static UIManager instance;
    public List<GameObject> screens;
    public VideoPlayer _splaceScreenVideo;
    #endregion



    // Ensure only one instance of UIManager exists
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
    }

    // Switch to a specific screen by index
    public void SwitchScreen(int screen)
    {
        for (int i = 0; i < screens.Count; i++)
        {
            if (i == screen)
            {
                screens[i].SetActive(true);
            }
            else
            {
                screens[i].SetActive(false);
            }
        }
    }

    public void ButtonClick()
    {
        Debug.Log("Button Click");
    }

}
