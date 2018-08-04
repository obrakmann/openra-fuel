#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA;
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Fuel;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("Used to store the player's global fuel reserves.")]
	class FuelStorageInfo : ITraitInfo
	{
		[Desc("Capacity of this fuel storage.")]
		public readonly int Capacity;

		public object Create(ActorInitializer init) { return new FuelStorage(init.Self, this); }
	}

	class FuelStorage : INotifyCreated, INotifyKilled, INotifySold, INotifyOwnerChanged
	{
		readonly FuelStorageInfo info;
		Fueltank fuelReserve;

		public FuelStorage(Actor self, FuelStorageInfo info)
		{
			this.info = info;
			fuelReserve = self.Owner.PlayerActor.Trait<Fueltank>();
		}

		void INotifyCreated.Created(Actor self)
		{
			fuelReserve.AddCapacity(info.Capacity);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo x)
		{
			fuelReserve.RemoveCapacity(info.Capacity);
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			fuelReserve.RemoveCapacity(info.Capacity);
			fuelReserve = newOwner.PlayerActor.Trait<Fueltank>();
			fuelReserve.AddCapacity(info.Capacity);
		}

		void INotifySold.Selling(Actor self) { }

		void INotifySold.Sold(Actor self)
		{
			fuelReserve.RemoveCapacity(info.Capacity);
		}
	}
}