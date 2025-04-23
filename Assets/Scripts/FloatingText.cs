using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    // Text Settings
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private string message = "Rope Broke!";
    [SerializeField] private float lifetime = 2f;

    // Animation Settings
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve;

    private float timeAlive;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        }

        textComponent.text = message;
        
        // Make sure we have a canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalPosition = transform.position;
        originalScale = transform.localScale;

        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        timeAlive = 0;
        canvasGroup.alpha = 1f;

        // Initial pop effect
        transform.localScale = originalScale * 0.7f;
        
        while (timeAlive < lifetime)
        {
            timeAlive += Time.deltaTime;
            float normalizedTime = timeAlive / lifetime;

            // Move upward
            Vector3 newPosition = originalPosition;
            newPosition.y += moveSpeed * timeAlive;
            transform.position = newPosition;

            // Scale according to curve
            float scale = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = originalScale * scale;

            // Fade out towards the end
            if (normalizedTime > 0.5f)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (normalizedTime - 0.5f) * 2f * fadeSpeed);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // Static method to create the floating text
    public static FloatingText Create(Vector3 position, Canvas parentCanvas, string message = null)
    {
        // Load the prefab
        GameObject prefab = Resources.Load<GameObject>("FloatingTextPrefab");
        if (prefab == null)
        {
            Debug.LogError("FloatingTextPrefab not found in Resources folder!");
            return null;
        }

        // Instantiate the prefab
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, parentCanvas.transform);
        FloatingText floatingText = instance.GetComponent<FloatingText>();

        // Set the message if provided
        if (!string.IsNullOrEmpty(message))
        {
            floatingText.message = message;
        }

        return floatingText;
    }

    // Overload to create from world position
    public static FloatingText CreateWorldToUI(Vector3 worldPosition, Canvas targetCanvas, Camera camera, string message = null)
    {
        // Convert world position to screen position
        Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);
        
        // Convert screen position to UI position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.transform as RectTransform,
            screenPos,
            targetCanvas.worldCamera,
            out Vector2 localPoint);

        Vector3 uiPosition = targetCanvas.transform.TransformPoint(localPoint);
        
        // Create the floating text at the UI position
        return Create(uiPosition, targetCanvas, message);
    }
} 