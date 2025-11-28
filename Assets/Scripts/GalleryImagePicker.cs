using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text;

public class GalleryImagePicker : MonoBehaviour
{
    public Button _camaraBtn;
    public Image profile_image;
    public GameObject _MaskingImage;
    public int maxSize = 1024;
    public float _profileWidth = 510f;

    private void Start()
    {
        _camaraBtn.onClick.AddListener(OnPickImageButtonClick);
    }

    public void OnPickImageButtonClick()
    {

        bool hasPermission = NativeGallery.CheckPermission(
            NativeGallery.PermissionType.Read,
            NativeGallery.MediaType.Image
        );

        if (hasPermission)
        {
            OpenGallery();
        }
        else
        {
            NativeGallery.RequestPermissionAsync((NativeGallery.Permission permission) =>
            {
                Debug.Log("Permission result: " + permission);
                if (permission == NativeGallery.Permission.Granted)
                {
                    OpenGallery();
                }
                else
                {
                    Debug.LogWarning("Permission denied!");
                }
            }, NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        }
    }

    private void OpenGallery()
    {
        NativeGallery.GetImageFromGallery((string path) =>
        {
            Debug.Log("Picked image path: " + path);
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
                profile_image.sprite = TextureToSprite(texture);
                UIManager.instance._profilePanel._saveBtn.gameObject.SetActive(true);
                UIManager.instance.SetMaxSize(_profileWidth, profile_image);
                profile_image.gameObject.SetActive(true);
                _MaskingImage.SetActive(true);
            }
            else
            {
                Debug.Log("No image selected");
            }
        }, "Select an image", "image/*");
    }

    private Sprite TextureToSprite(Texture2D texture)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite sprite = Sprite.Create(texture, rect, pivot);
        return sprite;
    }
}
