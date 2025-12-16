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

        float scaleValue = Mathf.Lerp(0.0004f, 0.025f, num);

        _spriteObject.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
}
