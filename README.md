# Lunar Heresy

 A mod for Risk of Rain 2 that turns Lunar coins into a fully featured per-run resource.
## Per-Run Lunar Coins
- Lunar coins are now reset to 0 at the start of each run. This is done without editing your save file, so you don't need to worry about your Lunar coin count being overwritten using this mod.
 - Optional integration with ProperSave to restore your coin count when loading a run in progress.
- Lunar coins added during a run don't get added to your save file. **(TODO: TEST THIS)**
- Multiplayer compatible **(TODO: TEST THIS)**
- The base drop rate has been tripled from vanilla and the drop rate decreses more slowly to keep the flow of lunar coins more consistent *(Configurable)*.
- Killing Mithrix no longer rewards the player with lunar coins. **(TODO: TEST THIS)**
- A portal to the bazaar now opens after killing the twisted scavenger, allowing you to continue looping with the ten coins it drops.
  - If you wait long enough after defeating the twisted scavenger, your run will automatically end.
  - This makes the twisted scav fight the ideal way to unlock the artificer

## Lunar Coin Price Changes
- Prices for lunar interactables have been reduced *(Configurable)*:
  - Lunar buds found in stages are now free.
  - The lunar chests and lunar seers in the bazaar now cost one coin.
  - The button in the bazaar that refreshes the shop is now free but can only be used once per visit.
  - Petting the glass frog on commencement is now free, and only needs to be done once to open the portal.

## Planned Features
- Consume a beads of fealty when activating the obelisk
- Add option to change cauldrons to cost coins instead of items
- Add a Shrine of the Moon which changes the teleporter rewards from items to lunar coins
 - Possibly also changes the boss to a horde of Lunar Chimeras
- Add a new enemy type that tries to evade the player but drops a guaranteed lunar coin on death.
- Add an artifact that makes the chests and seers in the bazaar free, but disables all means of earning coins
 - Add a mechanism for unlocking this artifact to commencement

## Changelog

**1.0.0**

- Added base LunarHeresy plugin.
- Added Configuration.
- Added LunarCoinHandler to manager per-run lunar coin counts across clients and server (TODO: fix client sync logic).
- Added LunarPricesHandler to change prices of lunar items to match config.
