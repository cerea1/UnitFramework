using System.Collections.Generic;
using CerealDevelopment.LifetimeManagement;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{

    public interface IActiveAbility : IAbilityBase
    {

    }
    public static class ActiveComponentExtensions
    {
        public static void AddActivator(this IActiveAbility ability, Object activator)
        {
            ActiveAbilitySystem.AddActivator(ability, activator);
        }

        public static bool RemoveActivator(this IActiveAbility ability, Object activator)
        {
#if UNITY_EDITOR
            if (ability.GetComponent<ActiveComponent>() == null)
            {
                Debug.LogError(ability.GetType().Name, ability as Object);
            }
#endif
            return ActiveAbilitySystem.RemoveActivator(ability, activator);
        }

        private static List<Object> tempDeactivatorsList = new List<Object>();
        public static bool IsAvailable(this IAbilityBase ability)
        {
            tempDeactivatorsList.Clear();
            ability.GetDeactivators(tempDeactivatorsList);
            if (tempDeactivatorsList.Count == 0)
            {
                return true;
            }
            return tempDeactivatorsList.Count == 1 && tempDeactivatorsList[0] is ActiveComponent;
        }
    }

    public class ActiveComponent : MonoBehaviour, IAbilityComponent, 
        ILifetimeObserver<IActiveAbility>
    {
        public event System.Action<ActiveComponent, Object> ActivatorAdded;
        public event System.Action<ActiveComponent, Object> ActivatorRemoved;

        private IActiveAbility ability;
        internal IActiveAbility Ability
        {
            get => ability;
            set
            {
                if (ability != value)
                {
                    if (ability != null)
                    {
                        ability.RemoveObserver(this);
                    }
                    ability = value;
                    if (ability != null)
                    {
                        ability.AddObserver(this);
                        if (ability.IsLifetimeInitialized)
                        {
                            OnInitialized(ability);
                        }
                    }
                }
            }
        }

        [SerializeField]
        private List<Object> activators = new List<Object>();
        public List<Object> Activators => new List<Object>(activators);
        public int ActivatorsCount
        {
            get => activators.Count;
        }
        
        public void AddActivator(Object activator)
        {
            if (!activators.ContainsByID(activator))
            {
                activators.Add(activator);
                UpdateState();

                ActivatorAdded?.Invoke(this, activator);
            }
        }

        public bool RemoveActivator(Object activator)
        {
            var result = activators.RemoveByID(activator);
            if (result)
            {
                UpdateState();

                ActivatorRemoved?.Invoke(this, activator);
            }
            return result;
        }


        public void OnInitialized(IActiveAbility obj)
        {
            UpdateState();
        }

        public void OnDisposed(IActiveAbility obj)
        {
            activators.Clear();
            ability.RemoveDeactivator(this);
        }

        private void UpdateState()
        {
            if (activators.Count == 0)
            {
                ability.AddDeactivator(this);
            }
            else
            {
                ability.RemoveDeactivator(this);
            }
        }
    }
}