using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestonePrerequisiteListView : MonoBehaviour
    {
        [SerializeField] private MilestonePrerequisiteElement _template;
        [SerializeField] private GameObject _emptyState;
        
        public Technology Technology { get; private set; }
        
        private readonly List<MilestonePrerequisiteElement> _elements = new();
        
        private void Awake()
        {
            _template.gameObject.SetActive(false);
            _emptyState.gameObject.SetActive(false);
        }

        public void Show(Technology technology)
        {
            Technology = technology;
            CreateElements();
            _emptyState.SetActive(!technology?.Prerequisites.Customs.Any() ?? true);
        }

        private void CreateElements()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            if (Technology == null) return;
            foreach (var prerequisite in Technology.Prerequisites.Customs)
            {
                var element =  Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Initialize(prerequisite);
                element.gameObject.SetActive(true);
            }
        }
    }
}