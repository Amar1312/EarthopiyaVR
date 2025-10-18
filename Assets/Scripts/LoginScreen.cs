using DG.Tweening;
using UnityEngine;

public class LoginScreen : MonoBehaviour
{
    [Header("UI Settings")]
    public RectTransform menuPanel;     // The panel to animate
    public float moveDuration = 0.6f;   // Duration of the animation
    public float offscreenOffset = 800f; // How far below the screen it starts
    public Ease easeType = Ease.OutBack; // Animation easing type

    private Vector2 originalPosition;

    void Start()
    {
        if (menuPanel == null)
            menuPanel = GetComponent<RectTransform>();

        // Save the original position
        originalPosition = menuPanel.anchoredPosition;

        // Move it off-screen (below)
        menuPanel.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y - offscreenOffset);

        // Animate it upward into place
        menuPanel.DOAnchorPos(originalPosition, moveDuration)
                 .SetEase(easeType)
                 .SetUpdate(true); // Works even if timeScale = 0 (like paused menu)
    }

    // Optional: Call this to hide it again
    public void HideMenu()
    {
        menuPanel.DOAnchorPos(new Vector2(originalPosition.x, originalPosition.y - offscreenOffset), moveDuration)
                 .SetEase(Ease.InBack);
    }
}
