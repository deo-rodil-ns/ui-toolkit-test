using System;
using GothicVampire.Game;
using GothicVampire.Roads;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class RoadConstructionHud : MonoBehaviour
    {
        #region Inspector
        [Header("References")]
        [SerializeField] private RoadConstructionButton _template;
        #endregion

        private RoadManager _roadManager;

        private readonly List<RoadConstructionButton> _elements = new();

        private void Awake()
        {
            _template.gameObject.SetActive(false);
        }

        private void Start()
        {
            _roadManager = ServiceLocator.Get<World>().Player.GetService<RoadManager>();

            CreateElements();
        }

        private void CreateElements()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();
            
            foreach (var data in _roadManager.Settings.PurchasableRoads)
            {
                var element = Instantiate(_template, _template.transform.parent);
                element.Initialize(data, _roadManager.Faction);
                element.gameObject.SetActive(true);
                element.name = $"Element - {data.DisplayName}";
                _elements.Add(element);
            }
        }

        public void Evt_MenuToggled()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
