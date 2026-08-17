using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private InteractionSystem interactionSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionSystem.SetInteractable(GetComponent<IInteractable>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionSystem.RemoveInteractable(GetComponent<IInteractable>());
        }
    }
}