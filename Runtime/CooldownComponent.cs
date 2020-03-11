using System;
using CerealDevelopment.TimeManagement;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
    public class CooldownComponent : MonoBehaviour, IAbilityComponent, IUpdatable
    {
        public float cooldown = 0f;

        private IActAbility ability;

        private float lastAct;

        private void Awake()
        {
            ability = GetComponent<IActAbility>();
            if (ability == null)
            {
                throw new System.Exception();
            }
        }

        private void OnEnable()
        {
            if (ability == null)
            {
                Awake();
            }
            ability.Acted += OnAbilityActed;
        }

        private void OnDisable()
        {
            ability.Acted -= OnAbilityActed;
        }

        private void OnAbilityActed(IActAbility obj)
        {
            lastAct = Time.time;
            this.EnableUpdates();
            ability.AddDeactivator(this);
        }

        void IUpdatable.OnUpdate()
        {
            if (lastAct + cooldown < Time.time)
            {
                ability.RemoveDeactivator(this);
                this.DisableUpdates();
            }
        }
    }
}