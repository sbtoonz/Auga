﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugaUnity
{
    public class SkillsPanelController : MonoBehaviour
    {
        public GameObject SkillsContainer;
        public SkillsPanelSkillController SkillPrefab;

        private readonly Dictionary<Skills.SkillType, SkillsPanelSkillController> _skills = new Dictionary<Skills.SkillType, SkillsPanelSkillController>();
        private int _skillsCount;

        public void Start()
        {
            SkillPrefab.gameObject.SetActive(false);
            Update();
        }

        public void Update()
        {
            var player = Player.m_localPlayer;
            if (player != null)
            {
                UpdateSkills(player);
            }
        }

        private void UpdateSkills(Player player)
        {
            var skills = player.GetSkills();

            foreach (var skillDef in skills.m_skills)
            {
                _skills.TryGetValue(skillDef.m_skill, out var currentSkillElement);

                if (skills.m_skillData.ContainsKey(skillDef.m_skill))
                {
                    if (currentSkillElement == null)
                    {
                        var effect = Instantiate(SkillPrefab, SkillsContainer.transform, false);
                        effect.SkillType = skillDef.m_skill;
                        _skills.Add(skillDef.m_skill, effect);
                    }
                    else
                    {
                        currentSkillElement.SetActive(true);
                    }
                }
                else if (currentSkillElement != null)
                {
                    currentSkillElement.SetActive(true);
                }
            }

            if (_skillsCount != _skills.Count)
            {
                _skillsCount = _skills.Count;
                SortSkillElements();
            }
        }

        public void SortSkillElements()
        {
            var children = SkillsContainer.transform.Cast<Transform>().Select(x => x.GetComponent<SkillsPanelSkillController>()).ToList();
            children.Sort((a, b) => a.SkillType.CompareTo(b.SkillType));
            for (var i = 0; i < children.Count; ++i)
            {
                children[i].transform.SetSiblingIndex(i);
            }
        }
    }
}
