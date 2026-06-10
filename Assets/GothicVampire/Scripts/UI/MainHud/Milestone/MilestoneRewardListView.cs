using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneRewardListView : MonoBehaviour
    {
        [SerializeField] private MilestoneRewardElement _template;
        [SerializeField] private GameObject _emptyState;
        
        public Technology Technology { get; private set; }
        
        private readonly List<MilestoneRewardElement> _elements = new();
        
        private void Awake()
        {
            _template.gameObject.SetActive(false);
            _emptyState.gameObject.SetActive(false);
        }

        public void Show(Technology technology)
        {
            Technology = technology;
            CreateElements();
            _emptyState.SetActive(!technology?.Effects.Any() ?? true);
        }

        private void CreateElements()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            if (Technology == null) return;
            foreach (var effect in Technology.Effects)
            {
                var element =  Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Initialize(effect);
                element.gameObject.SetActive(true);
            }
        }
    }
}