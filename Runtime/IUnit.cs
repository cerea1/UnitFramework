using CerealDevelopment.LifetimeManagement;
using System.Collections.Generic;

namespace CerealDevelopment.UnitFramework
{
    public interface IUnitBase
    {

    }
    public interface IUnit<U> : IUnitBase where U : IUnit<U>
    {
        T GetAbility<T>() where T : IAbility<U>;
        List<T> GetAbilities<T>() where T : IAbility<U>;

    }
}
