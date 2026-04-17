using UnityEngine;

/// <summary>
/// 상호작용 가능한 오브젝트.
/// </summary>
public class SimpleInteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color focusedColor = Color.yellow;
    [SerializeField] private Color interactedColor = Color.cyan;

    [SerializeField] private float rotateAmountOnInteract = 45.0f;
    [SerializeField] private bool allowMultipleInteraction = true;

    [SerializeField] private Renderer cachedRenderer;
    [SerializeField] private Material cachedMaterial;

    private bool hasInteracted = false;

    void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        cachedMaterial = cachedRenderer.material;
    }

    public void OnFocusEnter()
    {
        if(hasInteracted == true && allowMultipleInteraction == false)
        {
            return;
        }

        ApplyColor(focusedColor);
    }

    public void OnFocusExit()
    {
        if(hasInteracted == true)
        {
            ApplyColor(interactedColor);
        }
        else
        {
            ApplyColor(normalColor);
        }
    }

    public void Interact()
    {
        if (hasInteracted == true && allowMultipleInteraction == false)
        {
            return;
        }

        Vector3 currentEulerAngles = transform.eulerAngles;
        float nextYAngle = currentEulerAngles.y + rotateAmountOnInteract;
        Vector3 nextEulerAngles = new Vector3(currentEulerAngles.x, nextYAngle, currentEulerAngles.z);

        transform.eulerAngles = nextEulerAngles;

        hasInteracted = true;
        ApplyColor(interactedColor);
    }

    void ApplyColor(Color targetColor)
    {
        cachedMaterial.color = targetColor;
    }
}
