using CerealDevelopment.LifetimeManagement;
using System.Collections.Generic;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
    public abstract class Ability<U> : MonoBehaviour, IAbility<U>, ILifetime where U : IUnit<U>
    {
        #region Lifetime
        public bool IsConstructed { get; private set; }
        public bool IsLifetimeInitialized { get; private set; }

        public void AbilityConstruct(U unit)
        {
            Unit = unit;

            innerAbilities = new List<IAbility<U>>(GetComponentsInChildren<IAbility<U>>());
            var siblings = new List<IAbility<U>>(GetComponents<IAbility<U>>());
            while (siblings.Count > 0)
            {
                while (innerAbilities.Remove(siblings[0])) { }
                siblings.RemoveAtSwapBack(0);
            }
            while (innerAbilities.Remove(this)) { }

            if (!IsConstructed)
            {
                Construct();

                FireConstructedEvent();
            }
        }

        public void AbilityInitialize(U unit)
        {
            if (!IsLifetimeInitialized)
            {
                if (unit == null || !unit.Equals(Unit))
                {
                    throw new System.Exception();
                }

                Initialize();


                FireInitializedEvent();

                canUpdateState = true;

                if (gameObject.activeInHierarchy)
                {
                    UpdateState();
                }
                else
                {
                    OnDisable();
                }
            }
            else
            {
                Debug.LogException(new System.Exception(), this);
            }
        }

        public void AbilityDispose(U unit)
        {
            if (IsLifetimeInitialized)
            {
                if (unit == null || !unit.Equals(Unit))
                {
                    throw new System.Exception();
                }

                IsActive = false;
                deactivators.Clear();
                FireDisposedEvent();
                Dispose();
                if (IsActive)
                {
                    IsActive = false;
                    Deactivated?.Invoke(this);
                }
                deactivators.Clear();
                FireDisposedEvent();
                Dispose();

                canUpdateState = false;


                IsLifetimeInitialized = false;
            }
            else
            {
                Debug.LogException(new System.Exception(), this);
            }
        }

        private void OnDestroy()
        {
            if (IsLifetimeInitialized)
            {
                FireDisposedEvent();
                Dispose();
            }

            Destroy();
            FireDestroyedEvent();
        }

        protected virtual void Construct() { }

        protected virtual void Initialize() { }

        protected virtual void Dispose() { }

        protected virtual void Destroy() { }

        public void FireConstructedEvent()
        {
            IsConstructed = true;
        }

        public void FireInitializedEvent()
        {
            IsLifetimeInitialized = true;
            Lifetime.OnInitialized(this);
        }

        public void FireDisposedEvent()
        {
            IsLifetimeInitialized = false;
            Lifetime.OnDisposed(this);
        }

        public void FireDestroyedEvent()
        {
            Lifetime.OnDestroyed(this);
        }
        #endregion

        public event System.Action<IAbilityBase> Activated;
        public event System.Action<IAbilityBase> Deactivated;

        private U unit;
        public virtual U Unit
        {
            get => unit;
            private set
            {
                if (unit == null)
                {
                    unit = value;
                }
                else if (IsLifetimeInitialized)
                {
                    throw new System.Exception();
                }
            }
        }


        public bool IsActive
        {
            get;
            private set;
        }

        [SerializeField]
        private List<Object> deactivators = new List<Object>();

        public int DeactivatorsCount
        {
            get => deactivators.Count;
        }

        private bool canUpdateState;

        private List<IAbility<U>> innerAbilities;

        public void GetDeactivators(List<Object> getList)
        {
            getList.AddRange(deactivators);
        }

        public void AddDeactivator(Object deactivator)
        {
            if (!deactivators.ContainsByID(deactivator))
            {
                deactivators.Add(deactivator);
                if (IsLifetimeInitialized && innerAbilities != null)
                {
                    UpdateState();
                }
            }
        }

        public void RemoveDeactivator(Object deactivator)
        {
            if (deactivators.RemoveByIDSwapBack(deactivator))
            {
                if (IsLifetimeInitialized && innerAbilities != null)
                {
                    UpdateState();
                }
            }
        }

        public T GetAbility<T>() where T : IAbility<U>
        {
            return unit.GetAbility<T>();
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }

        protected virtual void OnEnable()
        {
            if (IsLifetimeInitialized)
            {
                RemoveDeactivator(gameObject);
            }
        }

        protected virtual void OnDisable()
        {
            if (IsLifetimeInitialized)
            {
                AddDeactivator(gameObject);
            }
        }

        private void UpdateState()
        {
            if (canUpdateState)
            {
                if (innerAbilities == null)
                {
                    Debug.LogError("Ability is not initialized completely", this);
                    return;
                }
                var previousState = IsActive;
                IsActive = deactivators.Count == 0;

                if (IsActive)
                {
                    for (int i = 0; i < innerAbilities.Count; i++)
                    {
                        innerAbilities[i].RemoveDeactivator(transform);
                    }
                }
                else
                {
                    for (int i = 0; i < innerAbilities.Count; i++)
                    {
                        innerAbilities[i].AddDeactivator(transform);
                    }
                }

                if (previousState != IsActive)
                {
                    if (IsActive)
                    {
                        OnActivated();
                        Activated?.Invoke(this);
                    }
                    else
                    {
                        OnDeactivated();
                        Deactivated?.Invoke(this);
                    }
                }
            }
        }
    }
}
