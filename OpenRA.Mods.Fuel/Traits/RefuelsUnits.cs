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
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("Refuels units with the `Refuelable` trait and which are located on top of this actor.")]
	public class RefuelsUnitsInfo : ITraitInfo, Requires<BuildingInfo>
	{
		[Desc("The amount of fuel transferred to the recipient per interval.")]
		public readonly int FuelPerTransfer = 1;

		[Desc("Refuel transfer interval (in ticks).")]
		public readonly int TransferInterval = 1;

		[Desc("Offset relative to the building's top-left where the actor needs to be to receive fuel.")]
		public readonly CVec RefuelOffset = CVec.Zero;

		[Desc("Retrieve fuel from the player's global fuel reserve instead of the actor's own fueltank.")]
		public readonly bool UseFuelReserve = true;

		public object Create(ActorInitializer init) { return new RefuelsUnits(init.Self, this); }
	}

	public class RefuelsUnits : ITick, IRefuelUnits
	{
		public readonly RefuelsUnitsInfo Info;
		public readonly Fueltank Fueltank;

		public Actor CurrentUnit { get; private set; }
		Fueltank otherFueltank;
		WPos cachedPosition;
		int ticks;

		public RefuelsUnits(Actor self, RefuelsUnitsInfo info)
		{
			Info = info;

			var source = info.UseFuelReserve ? self.Owner.PlayerActor : self;
			Fueltank = source.Trait<Fueltank>();
		}

		bool IRefuelUnits.CanRefuel(Actor self, Refuelable refuelable)
		{
			return !Fueltank.IsEmpty;
		}

		void ITick.Tick(Actor self)
		{
			if (CurrentUnit == null)
				return;

			if (cachedPosition != CurrentUnit.CenterPosition)
			{
				CurrentUnit = null;
				return;
			}

			if (--ticks > 0)
				return;

			if (otherFueltank.IsFull)
				return;

			var amount = Math.Min(Fueltank.AvailableFuel(Info.FuelPerTransfer), otherFueltank.ReceivableFuel(Info.FuelPerTransfer));
			if (amount > 0)
			{
				Fueltank.TakeFuel(amount);
				otherFueltank.ReceiveFuel(amount);
			}

			cachedPosition = CurrentUnit.CenterPosition;
			ticks = Info.TransferInterval;
		}

		public void RefuelUnit(Actor self, Actor unit)
		{
			if (CurrentUnit != null)
				return;

			var refuelable = unit.TraitOrDefault<Refuelable>();
			if (refuelable == null || !refuelable.CanRefuelAt(self, this))
				return;

			otherFueltank = refuelable.Fueltank;

			CurrentUnit = unit;
			cachedPosition = CurrentUnit.CenterPosition;
			ticks = Info.TransferInterval;
		}
	}
}