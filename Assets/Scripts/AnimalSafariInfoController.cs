using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPM;

public class AnimalSafariInfoController : MonoBehaviour
{
    public List<GameObject> _allAnimalPanel;
    public Demo_UI _mapCanvas;

    private void OnEnable()
    {
        for (int i = 0; i < _allAnimalPanel.Count; i++)
        {
            _allAnimalPanel[i].SetActive(false);
        }

        int num = SceneController.Instance._animalData.animalNumber;
        _allAnimalPanel[num].SetActive(true);
        _mapCanvas._animal = _allAnimalPanel[num].GetComponent<AnimalSafariPanel>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

   
}
