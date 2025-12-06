using UnityEngine;
using UnityEngine.EventSystems;

namespace CasperSDK.Samples
{
    /// <summary>
    /// Auto-creates an EventSystem if one doesn't exist.
    /// This allows UI buttons to work without manual EventSystem setup.
    /// </summary>
    [AddComponentMenu("CasperSDK/Samples/Event System Manager")]
    public class EventSystemManager : MonoBehaviour
    {
        private void Awake()
        {
            // Check if there's already an EventSystem in the scene
            if (FindObjectOfType<EventSystem>() == null)
            {
                // Create a new GameObject with EventSystem
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                
                Debug.Log("[BalanceTest] EventSystem created automatically");
            }
        }
    }
}
