#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Traits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Fuel.Activities;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("This actor can be refueled.")]
	public class RefuelableInfo : ITraitInfo, Requires<NeedsFuelInfo>, Requires<FueltankInfo>, Requires<IMoveInfo>
	{
		[Desc("List of actor types at which this actor can refuel.")]
		[ActorReference]
		public readonly string[] RefuelActors = { };

		public object Create(ActorInitializer init) { return new Refuelable(init.Self, this); }
	}

	public class Refuelable : ITick, IIssueOrder, IResolveOrder
	{
		readonly Actor self;
		readonly Actor[] excludeSelf;
		public readonly RefuelableInfo Info;
		public readonly Fueltank Fueltank;
		public readonly IMove Move;

		RefuelsUnits refuels;

		public Refuelable(Actor self, RefuelableInfo info)
		{
			this.self = self;
			Info = info;
			excludeSelf = new[] { self };

			Fueltank = self.Trait<Fueltank>();
			Move = self.Trait<IMove>();
		}

		public bool CanRefuelAt(Actor host, IRefuelUnits refuelsUnits = null)
		{
			if (host == self)
				return false;

			if (host.IsDead || !host.IsInWorld)
				return false;

			if (!(host.AppearsFriendlyTo(self) && Info.RefuelActors.Contains(host.Info.Name)))
				return false;

			if (refuelsUnits == null)
				refuelsUnits = host.TraitOrDefault<IRefuelUnits>();

			if (refuelsUnits == null || !refuelsUnits.CanRefuel(host, this))
				return false;

			return true;
		}

		IEnumerable<IOrderTargeter> IIssueOrder.Orders
		{
			get
			{
				yield return new EnterAlliedActorTargeter<IRefuelUnitsInfo>("Refuel", 5,
					target => CanRefuelAt(target), _ => !Fueltank.IsFull);
			}
		}

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
		{
			if (order.OrderID == "Refuel")
				return new Order(order.OrderID, self, target, queued);

			return null;
		}

		void IResolveOrder.ResolveOrder(Actor self, Order order)
		{
			if (order.OrderString == "Refuel")
				self.QueueActivity(false, Refuel(self, order.Target));
		}

		void ITick.Tick(Actor self)
		{
			if (self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length > 0)
				return;

			if (refuels != null && refuels.CurrentUnit != self)
				refuels = null;

			var actorBelow = self.World.ActorMap.GetActorsAt(self.Location).Except(excludeSelf).FirstOrDefault();
			if (actorBelow == null)
				return;

			var r = actorBelow.TraitOrDefault<RefuelsUnits>();
			if (r == null)
				return;

			if (CanRefuelAt(actorBelow, r))
			{
				r.RefuelUnit(actorBelow, self);
				refuels = r;
			}
		}

		Activity Refuel(Actor self, Target target)
		{
			if (target.Type != TargetType.Actor)
				return null;

			return Refuel(self, target.Actor);
		}

		public Activity Refuel(Actor self, Actor host)
		{
			var refuels = host.TraitOrDefault<IRefuelUnits>();
			if (refuels is RefuelsUnits)
				return new Refuel(self, host);
			else if (refuels is RefuelsUnitsNear)
				return new RefuelNear(self, host);

			return null;
		}
	}
}