using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI
{
    [RequireComponent(typeof(Image))]
    public class AlphaThresholdHandler : MonoBehaviour
    {
        [SerializeField][Range(0.0f, 1.0f)] private float _alphaThreshold = 0.5f;

        private void Start()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = _alphaThreshold;
        }
    }
}
