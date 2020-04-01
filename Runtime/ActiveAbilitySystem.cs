using System;
using System.Collections.Generic;
using CerealDevelopment.LifetimeManagement;
using UnityEngine;

namespace CerealDevelopment.UnitFramework
{
	public class ActiveAbilitySystem : LifetimeMonoBehaviour, ILifetimePerceiver<IActiveAbility>
	{
		private static ActiveAbilitySystem _instance;
		public static ActiveAbilitySystem Instance
		{
			get
			{
				if (isRunning)
				{
					if (_instance == null)
					{
						CreateInstance();
					}
					return _instance;
				}
				return null;
			}
		}

		private static bool isRunning = false;


		[RuntimeInitializeOnLoadMethod]
		static void CreateInstance()
		{
			if (_instance == null)
			{
				if (Application.isPlaying)
				{
					isRunning = true;
					var gameObject = new GameObject(typeof(ActiveAbilitySystem).Name);
					_instance = gameObject.AddComponent<ActiveAbilitySystem>();
					DontDestroyOnLoad(gameObject);
					Debug.Log("Active ability system initialized");
				}
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

		protected override void Dispose()
		{
			base.Dispose();
			Debug.Log("Destroyed " + GetType().Name);
			isRunning = false;
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