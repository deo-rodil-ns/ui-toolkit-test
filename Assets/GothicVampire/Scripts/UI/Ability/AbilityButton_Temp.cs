using System;
using Cysharp.Threading.Tasks;
using GothicVampire.Abilities;
using GothicVampire.Abilities.Effects;
using GothicVampire.Game;
using Sylpheed.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.Abilities
{
    public class AbilityButton_Temp : MonoBehaviour
    {
        [SerializeField] private AbilityData _abilityData;

        [Header("Reference")] 
        [SerializeField] private AbilityButtomTooltip _tooltip;

        [Header("UI")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _abilityNameText;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private Image _cooldownFill;
        [SerializeField] private TMP_Text _effectStatusText;

        [Header("Config")] 
        [SerializeField] private Color _activeStatusColor;
        [SerializeField] private Color _inactiveStatusColor;
        
        public Ability Ability { get; private set; }
        private CrimsonDecree _effect;
        private bool _initialized;
        
        private void Start()
        {
            // TODO: Hack. Race condition with AbilityActor
            UniTask.NextFrame().ContinueWith(Initialize).Forget();
        }

        private void Initialize()
        {
            _initialized = true;
            
            // Get ability reference
            var actor = ServiceLocator.Get<World>()?.Player.AbilityActor ?? throw new Exception("No player AbilityActor found");
            var ability = actor.GetAbility(_abilityData);
            _effect = ability.Effect as CrimsonDecree ?? throw new Exception("No CrimsonDecree found");
            Ability = ability;
            
            Refresh();
        }
        
        private void Refresh()
        {
            _icon.sprite = Ability.Data.Icon;
            
            _tooltip.Initialize(_abilityData);
            //TODO: when more powers added, get their requirements here and set this one up properly
            _abilityNameText.text = $"{Ability.Data.DisplayName} (6)";
        }

        private void Update()
        {
            if (!_initialized) return;
            
            _cooldownFill.fillAmount = 1f - Ability.Cooldown.Progress;
            _cooldownText.gameObject.SetActive(!Ability.Cooldown.Ready);
            _cooldownText.text = Ability.Cooldown.TimeRemaining >= 10 
                ? $"{Ability.Cooldown.TimeRemaining:####}s" 
                : $"{Ability.Cooldown.TimeRemaining:####.0}s";
            _effectStatusText.text = _effect.Active 
                ? $"Effect Active: {_effect.EffectTimeRemaining:####}s" 
                : "Effect Inactive";
            _effectStatusText.color = _effect.Active ? _activeStatusColor : _inactiveStatusColor;
        }

        public void Evt_ButtonClick() => Ability.BeginTargeting();
    }
}