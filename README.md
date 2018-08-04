# OpenRA Fuel Plugin
The OpenRA Fuel Plugin is a .dll that can be used by OpenRA mods to add support for fuel-consuming and refueling actors.

## General Overview and Features
The plugin provides several new traits which interact in a number of ways. These traits are
* Fueltank
* FuelStorage
* FuelBar
* FuelGenerator
* NeedsFuel
* Refuelable
* RefuelsUnits
* RefuelsUnitsNear

The basic trait is the `Fueltank` and acts as the principal container that holds fuel. Fueltanks can:
* hold a variable capacity of fuel per actor type
* be configured to be either empty or completely full when an actor spawns
* emit a condition when a low fuel threshold (also configurable) is reached, useful for warning players with conditional overlays.

`FuelStorage` is optional and can be used as a global, off-map fuel reserve, assigned to a `Player` actor.  This requires adding the `Fueltank` trait to the `Player` actor.

`FuelBar` adds a graphical indicator of the fill level of an actor's `Fueltank` or the player's `FuelStorage`.

`FuelGenerator`s generate a configurable amount of fuel in a configurable amount of time. They can be configured to deduct a configurable amount of funds for generated fuel.  The generated fuel can be directed towards either the actor's `Fueltank` or the player's `FuelStorage`.

`NeedsFuel` is used by actors that consume fuel from their `Fueltank` by moving from one cell to the next. When the actor is out of fuel, a condition can be granted, allowing to disable the `Mobile` trait. Actors can be configured to consume fuel while stopped, and to be killed if they are out of fuel after a configurable amount of time. A possible use case for this might be to simulate starving infantry, or for airplanes falling out of the sky when their tanks run dry. Optionally, and enabled by default, the trait can extend the actor's tooltip to show the remaining and total range in cells.

The `Refuelable` trait has to be added to actors meant to receive fuel from other actors. Fuel can only be taken from actors of specific, configurable types.

The `RefuelsUnits` trait is used by buildings that serve as gas stations. The speed of fuel transfer can be configured, as well as the offset on the giving actor where the receiving actor needs to be position to receive fuel. Fuel can be taken from either the giving actor's own `Fueltank` or the player's `FuelStorage`.

The `RefuelsUnitsNear` trait is refuel units within a circular area around the giving actor. As with `RefuelsUnits`, the speed of transfer is configurable, as well as the range, which can be shown as a range circle. Additionally, it is configurable whether either the giving or receiving actors need to be stopped to give or receive fuel, respectively.


## How to get started
You basically just need to copy the .dll into your mod directory and add it to the `Assemblies:` stanza of your `mod.yaml` file.

``` yaml
Assemblies:
  yourmod|OpenRA.Mods.Fuel.dll
```

## Trait documentation
The trait documentation can be found [in the wiki](https://github.com/obrakmann/openra-fuel/wiki/Traits).

