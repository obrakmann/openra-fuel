#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using System.Drawing;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.Mods.Fuel.Traits;

namespace OpenRA.Mods.Fuel.Activities
{
	public class Refuel : Activity
	{
		readonly IMove move;
		readonly Actor host;
		readonly Target target;
		readonly RefuelsUnits refuels;
		readonly Fueltank fueltank;

		public Refuel(Actor self, Actor host)
		{
			move = self.TraitOrDefault<IMove>();
			this.host = host;
			target = Target.FromActor(host);
			refuels = host.TraitOrDefault<RefuelsUnits>();
			fueltank = self.TraitOrDefault<Fueltank>();
		}

		public override Activity Tick(Actor self)
		{
			if (move == null || refuels == null || fueltank == null)
				return NextActivity;

			self.SetTargetLine(target, Color.Green);

			var act = ActivityUtils.SequenceActivities(
				new MoveAdjacentTo(self, target),
				move.MoveTo(host.Location + refuels.Info.RefuelOffset, 0),
				new CallFunc(() => refuels.RefuelUnit(host, self)),
				new WaitFor(() => fueltank.IsFull, true));

			var rp = host.TraitOrDefault<RallyPoint>();
			if (rp != null)
				act.Queue(new CallFunc(() =>
					{
						self.SetTargetLine(Target.FromCell(self.World, rp.Location), Color.Green);
						self.QueueActivity(move.MoveTo(rp.Location, host));
					}));

			return ActivityUtils.SequenceActivities(act, NextActivity);
		}
	}
}