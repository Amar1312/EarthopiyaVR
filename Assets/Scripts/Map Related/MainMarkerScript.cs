using UnityEngine;


public class MainMarkerScript : MonoBehaviour
{
    public GameObject _spriteObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetSprite(float num)
    {
        //0.0003 => Small Size, 0.03 => Max Size
        //0.00005
        float scaleValue = Mathf.Lerp(0.00002f, 0.025f, num);

        _spriteObject.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
}
