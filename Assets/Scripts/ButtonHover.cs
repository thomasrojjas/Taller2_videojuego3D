using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Le da una animacion a los botones cuando pasas el mouse por encima o haces click.
// Sirve para el boton de reiniciar del menu de derrota.
[RequireComponent(typeof(Button))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animacion de tamaño")]
    public float hoverScale = 1.1f;   // cuanto crece al pasar el mouse
    public float clickScale = 0.95f;  // cuanto se achica al hacer click
    public float animationSpeed = 8f; // que tan rapido se hace la animacion

    [Header("Color del boton")]
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color normalColor = Color.white;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color targetColor;

    private void Start()
    {
        // guardamos el tamaño original para volver a el despues
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
        // vamos cambiando el tamaño de a poco para que se vea suave
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, animationSpeed * Time.deltaTime);

        // lo mismo con el color
        if (buttonImage != null)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, animationSpeed * Time.deltaTime);
        }
    }

    // cuando el mouse entra al boton
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    // cuando el mouse sale del boton
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetColor = normalColor;
    }

    // cuando aprietas el boton
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    // cuando sueltas el boton
    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }
}
