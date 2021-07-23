using CerealDevelopment.LifetimeManagement;
using System.Collections.Generic;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
	public class Unit<U> : LifetimeMonoBehaviour, IUnit<U> where U : Unit<U>
	{
		protected List<IAbility<U>> abilities = new List<IAbility<U>>();

		private List<IAbility<U>> initializeAbilitiesList = new List<IAbility<U>>();
		private List<Component> addedPoolAbilities = new List<Component>();
		private List<Component> addedNewObjectAbilities = new List<Component>();

		protected Dictionary<System.Type, IAbility<U>> abilitiesDictionary = new Dictionary<System.Type, IAbility<U>>();
		protected List<System.Type> nullAbilitiesList = new List<System.Type>();

		public event System.Action<IAbility<U>> AbilityAdded;

		private Dictionary<System.Type, System.Delegate> abilityAddedListeners = new Dictionary<System.Type, System.Delegate>();
		private Dictionary<System.Type, System.Delegate> abilityRemovedListeners = new Dictionary<System.Type, System.Delegate>();

		public void AddAbilityAddedListener<A>(System.Action<U, A> callback, bool invokeImmediateIfAbilityExists = true) where A : IAbility<U>
		{
			if (invokeImmediateIfAbilityExists)
			{
				var abilities = GetAbilities<A>();
				for (int i = 0; i < abilities.Count; i++)
				{
					callback?.DynamicInvoke(this, abilities[i]);
				}
			}
			var aType = typeof(A);
			if (abilityAddedListeners.TryGetValue(aType, out var existingDelegate))
			{
				abilityAddedListeners[aType] = System.Delegate.Combine(callback, existingDelegate);
			}
			else
			{
				abilityAddedListeners.Add(aType, callback);
			}
		}
		public void RemoveAbilityAddedListener<A>(System.Action<U, A> callback) where A : IAbility<U>
		{
			var aType = typeof(A);
			if (abilityAddedListeners.TryGetValue(aType, out var existingDelegate))
			{
				abilityAddedListeners[aType] = System.Delegate.Remove(existingDelegate, callback);
			}
		}

		private void InvokeAbilityAdded(IAbility<U> ability)
		{
			var type = ability.GetType();
			if (abilityAddedListeners.TryGetValue(type, out var callbacks))
			{
				callbacks?.DynamicInvoke(this, ability);
			}
			AbilityAdded?.Invoke(ability);
		}

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

		public T AddAbility<T>(T abilityPrefab) where T : Component, IAbility<U>
		{
			var type = abilityPrefab.GetType();
			U thisUnit = (U)this;

			if (abilitiesDictionary.TryGetValue(type, out var existedAbility))
			{
				return (T)existedAbility;
			}

			var abilityObj = Instantiate(abilityPrefab, this.transform.position, this.transform.rotation, this.transform);

			var ability = abilityObj.GetComponent<T>();

			abilitiesDictionary.Add(type, (T)ability);

			if (nullAbilitiesList.Contains(type))
			{
				nullAbilitiesList.Remove(type);
			}
			abilities.Add(ability);		

			if (isLifetimeConstructed)
			{
				ability.AbilityConstruct(thisUnit);
			}

			if (isLifetimeInitialized)
			{
				ability.AbilityInitialize(thisUnit);
			}

			//TODO: Add 'ability added' action
			addedPoolAbilities.Add(abilityObj);
			InvokeAbilityAdded(ability);
			return ability;
		}

		public T AddAbility<T>() where T : Component, IAbility<U>
		{
			var type = typeof(T);

			U thisUnit = (U)this;

			if (abilitiesDictionary.TryGetValue(type, out var existAbility))
			{
				return (T)existAbility;
			}

			var abilityGameObject = new GameObject(type.Name);
			abilityGameObject.transform.SetParent(this.transform);
			var ability = abilityGameObject.AddComponent<T>();
			abilities.Add(ability);

			abilitiesDictionary.Add(type, (T)ability);

			if (nullAbilitiesList.Contains(type))
			{
				nullAbilitiesList.Remove(type);
			}
			abilities.Add(ability);

			if (isLifetimeConstructed)
			{
				ability.AbilityConstruct(thisUnit);
			}

			if (isLifetimeInitialized)
			{
				ability.AbilityInitialize(thisUnit);
			}

			//TODO: Add 'ability added' action
			addedPoolAbilities.Add(ability);
			InvokeAbilityAdded(ability);
			return ability;
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

		public void RemoveAbility<T>(T ability) where T : IAbility<U>
		{
			var type = ability.GetType();

			if (abilitiesDictionary.ContainsKey(type))
			{
				if (ability.IsLifetimeInitialized)
				{
					U thisU = (U)this;
					ability.AbilityDispose(thisU);
				}

				abilitiesDictionary.Remove(type);
				abilities.RemoveAndSwapBack(ability);

				if (!nullAbilitiesList.Contains(type))
				{
					nullAbilitiesList.Add(type);
				}

				var addedPoolIndex = addedPoolAbilities.IndexOf(ability as Component);
				if (addedPoolIndex != -1)
				{
					Destroy(addedPoolAbilities[addedPoolIndex].gameObject);
					addedPoolAbilities.RemoveAtSwapBack(addedPoolIndex);
				}
				else
				{
					var addedNewIndex = addedNewObjectAbilities.IndexOf(ability as Component);
					if (addedNewIndex != -1)
					{
						Destroy(addedNewObjectAbilities[addedNewIndex].gameObject);
						addedNewObjectAbilities.RemoveAtSwapBack(addedNewIndex);
					}
				}
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