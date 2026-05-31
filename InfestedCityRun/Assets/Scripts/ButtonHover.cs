using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A modular component that adds interactive hover and click micro-animations to UI buttons.
/// Satisfies the PDF's requirement of button feedback on hover/click.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;
    public float animationSpeed = 8f;

    [Header("Color Tint Settings")]
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color normalColor = Color.white;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color targetColor;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            normalColor = buttonImage.color;
            targetColor = normalColor;
        }
    }

    private void Update()
    {
        // Smoothly interpolate the scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, animationSpeed * Time.deltaTime);

        // Smoothly interpolate the color if image is available
        if (buttonImage != null)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, animationSpeed * Time.deltaTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetColor = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }
}
