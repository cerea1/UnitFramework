using CerealDevelopment.LifetimeManagement;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
    public interface IAbilityBase : ILifetime
    {
        event System.Action<IAbilityBase> Activated;
        event System.Action<IAbilityBase> Deactivated;

        bool IsActive { get; }                

        void AddDeactivator(Object deactivator);
        void RemoveDeactivator(Object deactivator);

        void GetDeactivators(List<Object> deactivatorsList);

        T GetComponent<T>();
    }

    public interface IAbility<U> : IAbilityBase where U : IUnit<U>
    {
        U Unit { get; }
        void AbilityConstruct(U unit);

        void AbilityInitialize(U unit);

        void AbilityDispose(U unit);
    }

    public interface IActAbility : IAbilityBase
    {
        event System.Action<IActAbility> Acted;
        void Act();
    }

    public interface IAsyncActAbility : IActAbility
    {
        event System.Action<IAsyncActAbility> ActStarted;
    }
}