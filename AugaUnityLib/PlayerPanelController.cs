﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AugaUnity
{
    public class PlayerPanelController : MonoBehaviour
    {
        public Color HighlightColor1 = Color.white;
        public Color HighlightColor2 = Color.white;

        public Text HealthText;
        public Text HealthTextSecondary;
        public Text StaminaText;
        public Text StaminaTextSecondary;

        public GameObject EffectsContainer;
        public PlayerPanelEffectController EffectPrefab;

        protected string _highlightColor1;
        protected string _highlightColor2;
        protected readonly List<StatusEffect> _playerStatusEffects = new List<StatusEffect>();
        protected readonly List<PlayerPanelEffectController> _statusEffects = new List<PlayerPanelEffectController>();

        public virtual void Start()
        {
            EffectPrefab.gameObject.SetActive(false);
            _highlightColor1 = ColorUtility.ToHtmlStringRGB(HighlightColor1);
            _highlightColor2 = ColorUtility.ToHtmlStringRGB(HighlightColor2);
            Update();
        }

        public virtual void Update()
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            var healthDisplay = Mathf.CeilToInt(player.GetHealth());
            var staminaDisplay = Mathf.CeilToInt(player.GetStamina());
            HealthText.text = $"<color={_highlightColor1}>{healthDisplay}</color> / {Mathf.CeilToInt(player.GetMaxHealth())}";
            StaminaText.text = $"<color={_highlightColor1}>{staminaDisplay}</color> / {Mathf.CeilToInt(player.GetMaxStamina())}";

            player.GetTotalFoodValue(out var hp, out var stamina);
            var healthFoodDisplay = Mathf.CeilToInt(hp - Player.m_baseHP);
            var staminaFoodDisplay = Mathf.CeilToInt(stamina - Player.m_baseStamina);
            HealthTextSecondary.text = $"Base <color={_highlightColor2}>{Player.m_baseHP}</color> + Food <color={_highlightColor2}>{healthFoodDisplay}</color>";
            StaminaTextSecondary.text = $"Base <color={_highlightColor2}>{Player.m_baseStamina}</color> + Food <color={_highlightColor2}>{staminaFoodDisplay}</color>";

            UpdateStatusEffects(player);
        }

        public virtual void UpdateStatusEffects(Player player)
        {
            _playerStatusEffects.Clear();
            player.GetSEMan().GetHUDStatusEffects(_playerStatusEffects);

            while (_statusEffects.Count < _playerStatusEffects.Count)
            {
                var effect = Instantiate(EffectPrefab, EffectsContainer.transform, false);
                effect.Index = _statusEffects.Count;
                _statusEffects.Add(effect);
            }

            foreach (var effect in _statusEffects)
            {
                effect.SetActive(effect.Index < _playerStatusEffects.Count);
            }
        }
    }
}