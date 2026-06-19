using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Forwards pointer clicks on the NEXT button to NarrationManager.Advance().
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class NarrationAdvanceButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (NarrationManager.Instance != null)
            NarrationManager.Instance.Advance();
    }
}
