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
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("Refuels units with the `Refuelable` trait within a certain range.")]
	public class RefuelsUnitsNearInfo : ITraitInfo, Requires<FueltankInfo>
	{
		[Desc("The amount of fuel transferred to the recipient per interval.")]
		public readonly int FuelPerTransfer = 1;

		[Desc("Refuel transfer interval (in ticks).")]
		public readonly int TransferInterval = 1;

		[Desc("Range in which actors are close enough to receive fuel.")]
		public readonly WDist Range = WDist.FromCells(1);

		[Desc("When this is set to false, actors transferring fuel must not move.")]
		public readonly bool RefuelWhileMoving = false;

		[Desc("When this is set to false, actors receiving fuel must not move.")]
		public readonly bool RefuelMovingActors = false;

		[Desc("Draw range circle when this is set to true.")]
		public readonly bool ShowRangeCircle = true;

		[Desc("Color of the range circle.")]
		public readonly Color RangeCircleColor = Color.Violet;

		public virtual object Create(ActorInitializer init) { return new RefuelsUnitsNear(init.Self, this); }
	}

	public class RefuelsUnitsNear : ITick, IRefuelUnits, IRenderAboveShroudWhenSelected
	{
		public readonly RefuelsUnitsNearInfo Info;
		readonly Actor self;
		readonly Fueltank fueltank;
		readonly IMove move;

		int ticks;

		public RefuelsUnitsNear(Actor self, RefuelsUnitsNearInfo info)
		{
			Info = info;
			this.self = self;
			fueltank = self.Trait<Fueltank>();
			move = self.TraitOrDefault<IMove>();
			ticks = info.TransferInterval;
		}

		bool IRefuelUnits.CanRefuel(Actor self, Refuelable refuelable)
		{
			return !fueltank.IsEmpty;
		}

		void ITick.Tick(Actor self)
		{
			if (--ticks > 0 || (!Info.RefuelWhileMoving && move != null && move.IsMoving))
				return;

			var actorsToRefuel = self.World.FindActorsInCircle(self.CenterPosition, Info.Range)
				.Where(a => a.AppearsFriendlyTo(self));

			foreach (var actor in actorsToRefuel)
			{
				if (actor == self)
					continue;

				var refuelable = actor.TraitOrDefault<Refuelable>();
				if (refuelable == null || !refuelable.CanRefuelAt(self, this) || refuelable.Fueltank.IsFull)
					continue;

				if (!Info.RefuelMovingActors)
				{
					if (refuelable.Move.IsMoving)
						continue;
				}

				var otherFueltank = refuelable.Fueltank;
				var amount = Math.Min(fueltank.AvailableFuel(Info.FuelPerTransfer), otherFueltank.ReceivableFuel(Info.FuelPerTransfer));
				if (amount > 0)
				{
					fueltank.TakeFuel(amount);
					otherFueltank.ReceiveFuel(amount);
				}
			}

			ticks = Info.TransferInterval;
		}

		IEnumerable<IRenderable> IRenderAboveShroudWhenSelected.RenderAboveShroud(Actor self, WorldRenderer wr)
		{
			if (!Info.ShowRangeCircle || (!Info.RefuelWhileMoving && move != null && move.IsMoving) ||
				(self.World.RenderPlayer != null && self.Owner != self.World.RenderPlayer))
				yield break;

			yield return new RangeCircleRenderable(
				self.CenterPosition,
				Info.Range,
				0,
				Info.RangeCircleColor,
				Color.FromArgb(96, Color.Black));
		}

		bool IRenderAboveShroudWhenSelected.SpatiallyPartitionable { get { return false; } }
	}
}