using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.UI.Espionage
{
    public class MissionSelectView : MonoBehaviour
    {
        [SerializeField] private MissionDetailsView _missionDetailsView;
        [SerializeField] private SpySelectView _spySelectView;
        [SerializeField] private UnityEvent _evtClose;

        public void Temp_Details()
        {
            _missionDetailsView.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        public void Temp_Close()
        {
            _spySelectView.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        public void Temp_Start()
        {
            gameObject.SetActive(false);
            _evtClose.Invoke();
        }
    }
}