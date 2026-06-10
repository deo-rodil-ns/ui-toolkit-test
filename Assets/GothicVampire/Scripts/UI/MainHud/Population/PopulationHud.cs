using GothicVampire.Buildings;
using GothicVampire.Buildings.Effects;
using GothicVampire.Game;
using GothicVampire.Jobs;
using GothicVampire.UI.Currencies;
using GothicVampire.Villagers;
using Sylpheed.Core;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class PopulationHud : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text _totalPopulationText;
        [SerializeField] private Image _fillBarImage;
        [SerializeField] private PopulationHudBreakdownElement _populationBreakdown;

        [Header("Unused For Now")]
        [SerializeField] private PopulationHudTierElement _tierTemplate;
        [SerializeField] private CurrencyListView _totalUpkeepView;

        [Header("Data")]
        [SerializeField] private BuildingData _houseBuildingData;

        private BuildingManager _buildingManager;
        private VillagerManager _villagerManager;
        private JobManager _jobManager;
        //private readonly List<PopulationHudTierElement> _elements = new();

        private void Awake()
        {
            //_tierTemplate.gameObject.SetActive(false);
            _populationBreakdown.gameObject.SetActive(false);
        }

        private void Start()
        {
            _buildingManager = ServiceLocator.Get<World>().Player.GetService<BuildingManager>();
            _villagerManager = ServiceLocator.Get<World>().Player.GetService<VillagerManager>();
            _jobManager = _villagerManager.Faction.GetService<JobManager>();

            _villagerManager.EvtUpdated.AddListener(OnVillagersUpdated);
            _jobManager.EvtJobUpdated.AddListener(OnJobUpdated);

            _populationBreakdown.Init(_villagerManager);
            Refresh();
            //CreateTierElements();
        }

        private void OnDestroy()
        {
            _villagerManager?.EvtUpdated.RemoveListener(OnVillagersUpdated);
            _jobManager?.EvtJobUpdated.RemoveListener(OnJobUpdated);
        }

        private void Refresh()
        {
            /// ToDo: Temporary, refactor when Villager Immigration is implemented
            int housingSlots = 0;

            for (int i = 0; i < _houseBuildingData.Tiers.Count; i++)
            {
                int tierSlots = ((AddVillager)_houseBuildingData.Tiers[i].Effect).NumVillagers;
                int tierCount = _buildingManager.Buildings.Where(b => b.Data == _houseBuildingData).Count(b => b.CurrentTier?.TierLevel == i + 1);
                housingSlots += tierCount * tierSlots;
            }

            float villagerCount = _villagerManager.Villagers.Count;

            _totalPopulationText.text = $"{villagerCount}/{housingSlots}";
            _fillBarImage.fillAmount = villagerCount == 0 && housingSlots == 0 ? 1.0f : villagerCount / housingSlots;
            _populationBreakdown.Refresh(housingSlots);

            //_totalUpkeepView.Show(_villagerManager.UpkeepCost);
            //_totalUpkeepView.gameObject.SetActive(_villagerManager.UpkeepCost.Any());
        }

        //private void CreateTierElements()
        //{
        //    _elements.Clear();
        //    foreach (var data in _villagerManager.Settings.Villagers.OrderBy(v => v.Tier))
        //    {
        //        var element = Instantiate(_tierTemplate, _tierTemplate.transform.parent);
        //        element.gameObject.SetActive(true);
        //        _elements.Add(element);
        //        element.Initialize(data, _villagerManager);
        //    }
        //}

        private void OnJobUpdated(Job job) => Refresh();

        private void OnVillagersUpdated() => Refresh();
    }
}