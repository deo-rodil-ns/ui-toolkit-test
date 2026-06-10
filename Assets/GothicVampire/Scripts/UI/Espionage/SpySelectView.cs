using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.UI.Espionage
{
    public class SpySelectView : MonoBehaviour
    {
        [SerializeField] private MissionSelectView _missionSelectView;
        [SerializeField] private UnityEvent _evtClose;
        
        public void Temp_SelectSpy()
        {
            _missionSelectView.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        public void Temp_Close()
        {
            gameObject.SetActive(false);
            _evtClose.Invoke();
        }
    }
}