using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using UnityEngine;

namespace GothicVampire.UI.Technologies
{
    public class PrerequisiteListView : MonoBehaviour
    {
        [SerializeField] private PrerequisiteElement _template;
        [SerializeField] private GameObject _emptyState;
        
        public PrerequisiteGroup Prerequisites { get; private set; }
        
        private readonly List<PrerequisiteElement> _elements = new();
        
        private void Awake()
        {
            _template.gameObject.SetActive(false);
            _emptyState.gameObject.SetActive(false);
        }

        public void Show(PrerequisiteGroup prerequisites)
        {
            Prerequisites = prerequisites;
            CreateElements();
            _emptyState.SetActive(!Prerequisites?.Customs.Any() ?? true);
        }

        private void CreateElements()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            if (Prerequisites == null) return;
            
            // Create elements from everything defined in PrerequisiteGroup
            var entries = new List<PrerequisiteElementData>();
            entries.AddRange(Prerequisites.Technologies.Select(t => PrerequisiteElementData.Create(t, Prerequisites)));
            entries.AddRange(Prerequisites.Unlockables.Select(u => PrerequisiteElementData.Create(u, Prerequisites)));
            entries.AddRange(Prerequisites.Customs.Select(p => PrerequisiteElementData.Create(p, Prerequisites)));
            
            foreach (var entry in entries)
            {
                var element =  Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Initialize(entry);
                element.gameObject.SetActive(true);
            }
        }
    }
}