using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{

    #region Variables
    public static UIManager instance;
    public List<GameObject> screens;
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
}
