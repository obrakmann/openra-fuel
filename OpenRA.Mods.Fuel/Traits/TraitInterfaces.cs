#region Copyright & License Information
/*
 * Copyright 2015-2018 Oliver Brakmann
 * This file is part of the OpenRA Fuel Plugin, which is free software.
 * It is made available to you under the terms of the GNU General Public
 * License as published by the Free Software Foundation. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.Fuel.Traits
{
	public interface IRefuelUnitsInfo : ITraitInfo { }
	[RequireExplicitImplementation]
	public interface IRefuelUnits
	{
		bool CanRefuel(Actor host, Refuelable refuelable);
	}

	[RequireExplicitImplementation]
	public interface INotifyFuelStateChanged
	{
		void LowOnFuel(Actor self, Fueltank fueltank);
		void OutOfFuel(Actor self, Fueltank fueltank);
		void Refuelled(Actor self, Fueltank fueltank);
	}
}