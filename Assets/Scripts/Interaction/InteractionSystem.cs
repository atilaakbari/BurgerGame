using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    private IInteractable currentInteractable;

    public void SetInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }

    public void Interact()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}