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
using OpenRA;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.Mods.Fuel.Traits;

namespace OpenRA.Mods.Fuel.Activities
{
	public class RefuelNear : Activity
	{
		readonly IMove move;
		readonly Target target;
		readonly RefuelsUnitsNear refuelsNear;

		public RefuelNear(Actor self, Actor host)
		{
			move = self.TraitOrDefault<IMove>();
			target = Target.FromActor(host);
			refuelsNear = host.TraitOrDefault<RefuelsUnitsNear>();
		}

		public override Activity Tick(Actor self)
		{
			if (move == null || refuelsNear == null)
				return NextActivity;

			self.SetTargetLine(target, Color.Green);

			return ActivityUtils.SequenceActivities(move.MoveWithinRange(target, refuelsNear.Info.Range), NextActivity);
		}
	}
}