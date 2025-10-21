using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class toggleScrnBtn : NetworkBehaviour, IPointerDownHandler {

    [SerializeField] GameObject targetScrn;

    
    public void OnPointerDown(PointerEventData eventData) {
        if (targetScrn == null) {
            Debug.LogError("toggleScrnBtn: targetScrn is not assigned.");
            return;
        }
        if (!IsOwner) { return; }

        if (targetScrn != targetScrn.activeSelf) {
            targetScrn.SetActive(true);
            Debug.Log("toggleScrnBtn: " + targetScrn.name + " Screen Activated.");
        } else {
            targetScrn.SetActive(false);
            Debug.Log("toggleScrnBtn: " + targetScrn.name + " Screen Deactivated.");
        }
    }
}
