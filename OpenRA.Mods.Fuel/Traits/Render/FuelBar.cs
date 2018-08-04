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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Traits;
using OpenRA.Mods.Fuel;

namespace OpenRA.Mods.Fuel.Traits
{
	[Desc("Visualizes the fuel capcity of an actor.")]
	class FuelBarInfo : ITraitInfo
	{
		public readonly Color Color = Color.Violet;

		[Desc("Use the player's global fuel reserve instead of the actor's own fueltank.")]
		public readonly bool UseFuelReserve = false;

		public object Create(ActorInitializer init) { return new FuelBar(init.Self, this); }
	}

	class FuelBar : ISelectionBar, INotifyCreated, INotifyOwnerChanged
	{
		readonly Actor self;
		readonly FuelBarInfo info;
		Fueltank fueltank;

		public FuelBar(Actor self, FuelBarInfo info)
		{
			this.self = self;
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			var source = info.UseFuelReserve ? self.Owner.PlayerActor : self;
			fueltank = source.Trait<Fueltank>();
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (info.UseFuelReserve)
				fueltank = newOwner.PlayerActor.Trait<Fueltank>();
		}

		float ISelectionBar.GetValue()
		{
			if (!self.Owner.IsAlliedWith(self.World.RenderPlayer))
				return 0;

			return fueltank.Amount * 1f / fueltank.Capacity;
		}

		Color ISelectionBar.GetColor() { return info.Color; }

		bool ISelectionBar.DisplayWhenEmpty { get { return true; } }
	}
}