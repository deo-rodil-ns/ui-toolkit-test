using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Productions;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.WorldEvents
{
    public class WorldEventManager : MonoBehaviour, IFactionService
    {
        [SerializeField] private UnityEvent<WorldEvent> _evtShowMessage;
        [SerializeField] private UnityEvent<WorldEvent> _evtOnShowBanner;
        [SerializeField] private UnityEvent<WorldEvent> _evtOnUpdateBanner;
        [SerializeField] private UnityEvent<WorldEvent> _evtOnHideBanner;
        [SerializeField] private List<WorldEventData> _worldEventDatas;
        [Header("Reference")] 
        [SerializeField] private WorldEventBannerView _worldEventBannerView;
        
        public IReadOnlyCollection<WorldEventResult> WorldEventEntryDatas => GetAllWorldEventResults();
        public UnityEvent<WorldEvent> EvtShowMessage => _evtShowMessage;
        public UnityEvent<WorldEvent> EvtShowBanner => _evtOnShowBanner;
        
        public UnityEvent<WorldEvent> EvtUpdateBanner => _evtOnUpdateBanner;
        public UnityEvent<WorldEvent> EvtHideBanner => _evtOnHideBanner;

        public List<WorldEvent> WorldEvents { get; private set; } = new List<WorldEvent>();
        
        private Wallet _wallet;
        private ProductionManager _productionManager;
        
        #region IFactionService
        public Faction Faction { get; set; }

        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _wallet = faction.GetService<Wallet>();
            
            _worldEventDatas.ForEach(x =>
            {
                var newEvent = new WorldEvent(x, Faction);
                WorldEvents.Add(newEvent);
            });
        }
        
        #endregion

        private void Awake()
        {
            _evtOnShowBanner.AddListener(_worldEventBannerView.Show);
            _evtOnHideBanner.AddListener(_worldEventBannerView.Hide);
            
            _evtOnUpdateBanner.AddListener(_worldEventBannerView.Show);
        }

        private void LateUpdate()
        {
            WorldEvents.ForEach(x =>
            {
                if (x.IsActive)
                {
                    x.Update(Time.deltaTime);
                }
            });
        }

        private List<WorldEventResult> GetAllWorldEventResults()
        {
            var result = new List<WorldEventResult>();
            
            WorldEvents.ForEach(x =>
            {
                result.AddRange(x.Results);    
            });

            result = result.OrderByDescending(x => x.Cycle).ToList();

            return result;
        }
    }
}
