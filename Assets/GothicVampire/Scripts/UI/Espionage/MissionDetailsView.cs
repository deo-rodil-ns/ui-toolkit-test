using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.UI.Espionage
{
    public class MissionDetailsView : MonoBehaviour
    {
        [SerializeField] private MissionSelectView _missionSelectView;
        [SerializeField] private UnityEvent _evtClose;

        public void Temp_Start()
        {
            gameObject.SetActive(false);
            _missionSelectView.gameObject.SetActive(false);
            _evtClose.Invoke();
        }

        public void Temp_Close()
        {
            _missionSelectView.gameObject.SetActive(true);
            gameObject.SetActive(false);
            _evtClose.Invoke();
        }
    }
}