using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public LoginResponce _userData;
    public ProfileResponce _profileData;
    public List<CountryData> countries = new List<CountryData>();
    public DialCodeBtn _prefab;
    public Transform _parentDialCode;
    public Image _testImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(SetDialCode), 3f);
        //UIManager.instance.LoadSVGImage("https://country-code-au6g.vercel.app/AE.svg", _testImage, 200f);
    }

    void SetDialCode()
    {
        for (int i = 0; i < countries.Count; i++)
        {
            DialCodeBtn SpawnBtn = Instantiate(_prefab, _parentDialCode);
            SpawnBtn._countryData = countries[i];
        }
    }

}
