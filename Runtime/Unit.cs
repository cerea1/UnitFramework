using CerealDevelopment.LifetimeManagement;
using System.Collections.Generic;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
	public class Unit<U> : LifetimeMonoBehaviour, IUnit<U> where U : Unit<U>
	{
		protected List<IAbility<U>> abilities = new List<IAbility<U>>();

		protected Dictionary<System.Type, IAbility<U>> abilitiesDictionary = new Dictionary<System.Type, IAbility<U>>();
		protected List<System.Type> nullAbilitiesList = new List<System.Type>();

		public T GetAbility<T>() where T : IAbility<U>
		{
			var type = typeof(T);
			IAbility<U> dictionaryAbility;
			if (abilitiesDictionary.TryGetValue(type, out dictionaryAbility))
			{
				return (T)dictionaryAbility;
			}

			if (!nullAbilitiesList.Contains(type))
			{
				foreach (var ability in abilities)
				{
					if (ability is T)
					{
						var temp = (T)ability;
						abilitiesDictionary.Add(typeof(T), temp);
						return temp;
					}
				}
			}
			nullAbilitiesList.Add(type);
			return default(T);
		}

		public bool TryGetAbility<T>(out T ability) where T : IAbility<U>
		{
			var type = typeof(T);
			if (abilitiesDictionary.TryGetValue(type, out var dictionaryAbility))
			{
				ability = (T)dictionaryAbility;
				return true;
			}
			if (!nullAbilitiesList.Contains(type))
			{
				foreach (var checkAbility in abilities)
				{
					if (checkAbility is T)
					{
						ability = (T)checkAbility;
						abilitiesDictionary.Add(typeof(T), ability);
						return true;
					}
				}
			}
			nullAbilitiesList.Add(type);
			ability = default(T);
			return false;
		}

		public List<T> GetAbilities<T>() where T : IAbility<U>
		{
			var abilitiesList = new List<T>();
			for (int i = 0; i < abilities.Count; i++)
			{
				if (abilities[i] is T)
				{
					abilitiesList.Add((T)abilities[i]);
				}
			}
			return abilitiesList;
		}

		protected override void Construct()
		{
			if (!isLifetimeConstructed)
			{
				base.Construct();
				if (abilities.Count > 0)
				{
					Debug.Log("Wtf");
				}

				abilities.AddRange(GetComponentsInChildren<IAbility<U>>());

				foreach (var ability in abilities)
				{
					if (!abilitiesDictionary.ContainsKey(ability.GetType()))
					{
						abilitiesDictionary.Add(ability.GetType(), ability);
					}
				}

				U thisU = (U)this;
				for (int i = 0; i < abilities.Count; i++)
				{
					abilities[i].AbilityConstruct(thisU);
				}
			}
			else
			{
				Debug.LogException(new System.Exception(), this);
			}
		}

		protected override void Initialize()
		{
			if (!isLifetimeInitialized)
			{
				base.Initialize();

				U thisU = (U)this;
				for (int i = 0; i < abilities.Count; i++)
				{
					abilities[i].AbilityInitialize(thisU);
				}
			}
			else
			{
				Debug.LogException(new System.Exception(), this);
			}
		}

		protected override void Dispose()
		{
			if (isLifetimeInitialized)
			{
				base.Dispose();

				U thisU = (U)this;
				for (int i = 0; i < abilities.Count; i++)
				{
					abilities[i].AbilityDispose(thisU);
				}
			}
			else
			{
				Debug.LogException(new System.Exception(), this);
			}
		}
	}
}