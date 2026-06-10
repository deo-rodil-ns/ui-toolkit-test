using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneUnlockListView : MonoBehaviour
    {
        [SerializeField] private MilestoneUnlockElement _template;
        [SerializeField] private GameObject _emptyState;
        
        public Technology Technology { get; private set; }
        
        private readonly List<MilestoneUnlockElement> _elements = new();
        
        private void Awake()
        {
            _template.gameObject.SetActive(false);
            _emptyState.gameObject.SetActive(false);
        }

        public void Show(Technology technology)
        {
            Technology = technology;
            CreateElements();
            _emptyState.SetActive(!technology?.Data.Unlockables.Any() ?? true);
        }

        private void CreateElements()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            if (Technology == null) return;
            foreach (var unlockable in Technology.Data.Unlockables)
            {
                var element =  Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Initialize(unlockable);
                element.gameObject.SetActive(true);
            }
        }
    }
}