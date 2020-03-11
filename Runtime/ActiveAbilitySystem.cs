using System;
using System.Collections.Generic;
using CerealDevelopment.LifetimeManagement;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
    public class ActiveAbilitySystem : MonoBehaviour, ILifetimePerceiver<IActiveAbility>
    {
        private static ActiveAbilitySystem _instance;
        public static ActiveAbilitySystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }


        [RuntimeInitializeOnLoadMethod]
        static void CreateInstance()
        {
            if (_instance == null)
            {
                var gameObject = new GameObject(typeof(ActiveAbilitySystem).Name);
                DontDestroyOnLoad(gameObject);

                _instance = gameObject.AddComponent<ActiveAbilitySystem>();
            }
        }


        internal static void AddActivator(IActiveAbility ability, UnityEngine.Object activator)
        {
            if (!Instance.activeComponents.TryGetValue(ability, out var activeComponent))
            {
                activeComponent = Instance.CreateActiveComponent(ability);
            }
            activeComponent.AddActivator(activator);
        }

        internal static bool RemoveActivator(IActiveAbility ability, UnityEngine.Object activator)
        {
            if (!Instance.activeComponents.TryGetValue(ability, out var activeComponent))
            {
                activeComponent = Instance.CreateActiveComponent(ability);
            }
            return activeComponent.RemoveActivator(activator);
        }


        private Dictionary<IActiveAbility, ActiveComponent> activeComponents = new Dictionary<IActiveAbility, ActiveComponent>();

        private void Awake()
        {
            Lifetime.AddPerceiver(this);
        }

        private void OnDestroy()
        {
            Lifetime.RemovePerceiver(this);
        }

        public void OnInitialized(IActiveAbility obj)
        {
            if (!activeComponents.TryGetValue(obj, out var activeComponent))
            {
                CreateActiveComponent(obj);
            }
        }

        private ActiveComponent CreateActiveComponent(IActiveAbility obj)
        {
            var ability = obj as Component;
            if (ability != null)
            {
                var existingComponents = ability.GetComponents<ActiveComponent>();
                for (int i = 0; i < existingComponents.Length; i++)
                {
                    var existing = existingComponents[i];
                    var componentAbility = existing.Ability;
                    if (componentAbility == obj)
                    {
                        activeComponents.Add(obj, existing);
                        return existing;
                    }
                    else if (componentAbility == null)
                    {
                        existingComponents[i].Ability = obj;
                        activeComponents.Add(obj, existing);
                        return existing;
                    }
                }
                var activeComponent = ability.gameObject.AddComponent<ActiveComponent>();
                activeComponent.Ability = obj;
                return activeComponent;
            }
            else
            {
                Debug.LogException(new NotImplementedException());
                return null;
            }
        }

        public void OnDisposed(IActiveAbility obj)
        {
        }
    }
}