using DG.Tweening;
using UnityEngine.UI;

namespace GothicVampire.Utils.UI
{
    public static class ImageAnimation
    {
        public static void AnimateFill(this Image image, float to, float duration)
        {
            DOTween.To(
                () => image.fillAmount, 
                x => image.fillAmount = x, 
                to, 
                duration);
        }
    }
}