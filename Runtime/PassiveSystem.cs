using CerealDevelopment.LifetimeManagement;
using CerealDevelopment.TimeManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
    public class PassiveSystem : LifetimeMonoBehaviour, IUpdatable, ILifetimePerceiver<IPassiveAbility>
    {
        private HashSet<IPassiveAbility> passiveAbilities = new HashSet<IPassiveAbility>();
        private HashSet<IPassiveAbility> activeAbilities = new HashSet<IPassiveAbility>();

        [RuntimeInitializeOnLoadMethod]
        static void CreateInstance()
        {
            var gameObject = new GameObject(typeof(PassiveSystem).Name);
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<PassiveSystem>();
        }

        private void OnEnable()
        {
            this.EnableUpdates();
        }

        private void OnDisable()
        {
            this.DisableUpdates();
        }

        void IUpdatable.OnUpdate()
        {
            var i = 0;
            //var enumerator = activeAbilities.GetEnumerator();
            var list = new List<IPassiveAbility>(activeAbilities);

            while (i < list.Count)
            {
                try
                {
                    for (; i < list.Count; i++)
                    {
                        if(list[i].IsActive)
                        {
                            list[i].PassiveAct();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    i++;
                }
            }
        }

        void ILifetimePerceiver<IPassiveAbility>.OnInitialized(IPassiveAbility obj)
        {
            passiveAbilities.Add(obj);
            obj.Activated += OnActivated;
            obj.Deactivated += OnDeactivated;
            if (obj.IsActive)
            {
                OnActivated(obj);
            }
        }

        void ILifetimePerceiver<IPassiveAbility>.OnDisposed(IPassiveAbility obj)
        {
            activeAbilities.Remove(obj);
            passiveAbilities.Remove(obj);
            obj.Activated -= OnActivated;
            obj.Deactivated -= OnDeactivated;
        }

        private void OnActivated(IAbilityBase obj)
        {
            if (!activeAbilities.Add(obj as IPassiveAbility))
            {
                Debug.LogError("Doubled activation on " + obj, obj as UnityEngine.Object);
            }
        }

        private void OnDeactivated(IAbilityBase obj)
        {
            if (!activeAbilities.Remove(obj as IPassiveAbility))
            {
                Debug.LogError("Deactivation without activation on " + obj, obj as UnityEngine.Object);

            }
        }
    }
}