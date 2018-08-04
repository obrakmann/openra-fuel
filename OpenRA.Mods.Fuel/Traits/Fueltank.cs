#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.Fuel.Traits

{
	[Desc("This actor stores fuel. Also used for a player's global fuel reserve.")]
	public class FueltankInfo : ITraitInfo
	{
		[Desc("Total capacity of the fuel tank.")]
		public readonly int Capacity;

		[Desc("Determines whether the tank should be empty when the actor is created.")]
		public readonly bool EmptyOnCreation = false;

		[Desc("Low fuel warning threshold (in percent).")]
		public readonly int LowFuelWarning = 20;

		[Desc("These conditions are granted when the actor is low on fuel.")]
		[GrantedConditionReference] public readonly string[] LowOnFuelConditions = { };

		public object Create(ActorInitializer init) { return new Fueltank(init.Self, this); }
	}

	public class Fueltank : INotifyCreated
	{
		public readonly FueltankInfo Info;
		readonly Actor self;

		[Sync] public int Capacity { get; private set; }
		[Sync] public int Amount { get; private set; }
		public int Free { get { return Capacity - Amount; } }

		public bool IsFull { get { return Amount == Capacity; } }
		public bool IsLowOnFuel { get { return (Amount * 100 / Capacity) <= Info.LowFuelWarning; } }
		public bool IsEmpty { get { return Amount == 0; } }

		ConditionManager cm;
		List<int> conditionTokens;

		public Fueltank(Actor self, FueltankInfo info)
		{
			Info = info;
			Capacity = Info.Capacity;
			Amount = Info.EmptyOnCreation ? 0 : Capacity;

			this.self = self;
			conditionTokens = new List<int>();
		}

		void INotifyCreated.Created(Actor self)
		{
			cm = self.TraitOrDefault<ConditionManager>();
		}

		public void AddCapacity(int amount)
		{
			if (amount <= 0)
				return;

			if (int.MaxValue - amount < Capacity)
				Capacity = int.MaxValue;
			else
				Capacity += amount;
		}

		public void RemoveCapacity(int amount)
		{
			if (amount <= 0)
				return;

			if (amount > Capacity)
				Capacity = 0;
			else
				Capacity -= amount;

			if (Amount > Capacity)
				Amount = Capacity;
		}

		public int ReceivableFuel(int amount)
		{
			if (amount <= 0)
				return 0;

			return Math.Min(Free, amount);
		}

		public int AvailableFuel(int amount)
		{
			if (amount <= 0)
				return 0;

			return Math.Min(Amount, amount);
		}

		public void ReceiveFuel(int amount)
		{
			var a = ReceivableFuel(amount);
			if (a == 0)
				return;

			var oldAmount = Amount;

			Amount += a;

			if (oldAmount < Capacity && IsFull)
				foreach (var t in self.TraitsImplementing<INotifyFuelStateChanged>())
					t.Refuelled(self, this);

			if (cm != null && conditionTokens.Any() && !IsLowOnFuel)
			{
				foreach (var token in conditionTokens)
					cm.RevokeCondition(self, token);

				conditionTokens.Clear();
			}
		}

		public void TakeFuel(int amount)
		{
			var a = AvailableFuel(amount);
			if (a == 0)
				return;

			var oldAmount = Amount;

			Amount -= a;

			if (oldAmount > Info.LowFuelWarning && IsLowOnFuel && !IsEmpty)
				foreach (var t in self.TraitsImplementing<INotifyFuelStateChanged>())
					t.LowOnFuel(self, this);

			if (oldAmount > 0 && IsEmpty)
				foreach (var t in self.TraitsImplementing<INotifyFuelStateChanged>())
					t.OutOfFuel(self, this);

			if (cm != null && !conditionTokens.Any() && IsLowOnFuel)
			{
				foreach (var condition in Info.LowOnFuelConditions)
					conditionTokens.Add(cm.GrantCondition(self, condition));
			}
		}
	}
}