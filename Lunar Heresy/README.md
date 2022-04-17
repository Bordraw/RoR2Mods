# Lunar Heresy

 **Turn Lunar coins into a fully featured per-run resource.** Your coin count will reset on the start of each new run, and the game has been rebalanced to account for this change.

 - Lunar buds found in levels are free, and the price of the lunar shop and lunar seers has been reduced to accomodate the newly scarce coins.
 - The bazaar's reroller is free but can only be used once.
 - Lunar cauldrons now consume coins instead of items to function, giving you a use for any leftover lunar coins at the end of a run.
 - Need a lot of coins guaranteed? Now you can continue your loop after defeating the Twisted Scavenger.
 - Lunar coin drops are shared between all players in a multiplayer lobby, similar to gold.

Specific values for many of these features are configurable.

If you are tired of having to worry about spending vs saving your lunar coins betwen runs, or if you wish there were more things to do with your coins, this is the mod for you.

## Per-Run Lunar Coins
- Lunar coins are now reset to 0 at the start of each run. This is done without editing your save file, so you don't need to worry about your Lunar coin count being overwritten using this mod.
 - Optional integration with [ProperSave](https://thunderstore.io/package/KingEnderBrine/ProperSave/) to restore your coin count when loading a run in progress.
- Lunar coins added during a run don't get added to your save file.
- Multiplayer compatible **(TODO: TEST THIS)**
- The base drop rate has been tripled from vanilla (from 0.5% to 1.5%) and the drop rate decreses more slowly to keep the flow of lunar coins more consistent *(Configurable)*.
- A portal to the bazaar now opens after killing the twisted scavenger, allowing you to continue looping with the ten coins it drops.
  - If you wait long enough after defeating the twisted scavenger, your run will automatically end.
  - This makes the twisted scav fight the ideal way to unlock the artificer.
- Lunar coin pickups are shared with the entire multiplayer lobby *(Configurable)*.

## Lunar Coin Price Changes
- Prices for lunar interactables have been reduced *(Configurable)*:
  - Lunar buds found in stages are now free.
  - The lunar chests and lunar seers in the bazaar now cost one coin.
  - The button in the bazaar that refreshes the shop is now free but can only be used once per visit.
  - Petting the glass frog on commencement is now free, and only needs to be done once to open the portal.

## Lunar Cauldron Changes
- Lunar cauldrons now cost lunar coins to use *(Configurable)*:
 - White cauldrons cost 1 coin for one white item
 - Green Cauldrons cost 3 coins for one green item
 - Red Cauldrons cost 7 coins for one red item
- Commencement guaranteed to have at least one white cauldron for using up leftover lunar coins at the end of a run *(Configurable)*.

## Planned Features
- Consume a beads of fealty when activating the obelisk
- Add a Shrine of the Moon which changes the teleporter rewards from items to lunar coins
 - Possibly also changes the boss to a horde of Lunar Chimeras
- Add a new enemy type that tries to evade the player but drops a guaranteed lunar coin on death.
- Add an artifact that makes the chests and seers in the bazaar free, but disables all means of earning coins
 - Add a mechanism for unlocking this artifact to commencement

## Changelog
**1.0.0**
- Added base LunarHeresy plugin.
- Added Configuration.
- Added Languages.
- Added LunarCoinHandler to manager per-run lunar coin counts across clients and server (TODO: fix client sync logic).
- Added LunarPricesHandler to change prices of lunar items to match config.
- Added LunarCauldronHandler to handle changes to lunar cauldron logic.