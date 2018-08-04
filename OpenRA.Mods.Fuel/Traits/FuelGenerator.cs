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
using OpenRA;
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Fuel;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("Converts resources into fuel. Fuel is either deposited into an actor's own ",
		"fueltank or a player's global fuel reserve.")]
	public class FuelGeneratorInfo : ITraitInfo
	{
		[Desc("Amount of fuel generated per interval.")]
		public readonly int FuelPerInterval = 1;

		[Desc("The cost of producing fuel per interval. The will be no reimbursement " +
			"for partial fuel batches (when total capacity is exceeded).")]
		public readonly int CostPerInterval = 1;

		[Desc("Fuel generation interval in ticks.")]
		public readonly int Interval = 25;

		[Desc("Add generated fuel to the player's global fuel reserve instead of the actor's own fueltank.")]
		public readonly bool UseFuelReserve = true;

		public object Create(ActorInitializer init) { return new FuelGenerator(init.Self, this); }
	}

	public class FuelGenerator : ITick, INotifyOwnerChanged
	{
		public readonly FuelGeneratorInfo Info;

		Fueltank fueltank;
		PlayerResources resources;
		int ticks;

		public FuelGenerator(Actor self, FuelGeneratorInfo info)
		{
			Info = info;

			var source = info.UseFuelReserve ? self.Owner.PlayerActor : self;
			fueltank = source.Trait<Fueltank>();
			resources = self.Owner.PlayerActor.TraitOrDefault<PlayerResources>();
			ticks = info.Interval;
		}

		void ITick.Tick(Actor self)
		{
			if (Info.FuelPerInterval > 0 && --ticks < 0)
			{
				if (fueltank.ReceivableFuel(Info.FuelPerInterval) > 0 && resources.TakeCash(Info.CostPerInterval))
					fueltank.ReceiveFuel(Info.FuelPerInterval);

				ticks = Info.Interval;
			}
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (Info.UseFuelReserve)
				fueltank = newOwner.PlayerActor.Trait<Fueltank>();

			resources = newOwner.PlayerActor.TraitOrDefault<PlayerResources>();
		}
	}
}