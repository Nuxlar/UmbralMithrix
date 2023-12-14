# Umbral Mithrix

**CHECK OUT THE SONG MOD MADE FOR THE FIGHT! [AltMithrixTheme](https://thunderstore.io/package/Nuxlar/AltMithrixTheme/) (works without umbral)**

**_SUBMIT ANY ISSUES FOUND WITH A LOG TO THE LINK ABOVE_**

Major reworks to the fight, a new trophy item, and a practice mode. When you spawn on the moon there'll be a shrine to optionally activate the mode. You only need to interact with it once even if you're using LunarApostles. When completing the fight you'll get a trophy item to track your victories against Umbral.

### Practice Mode

Practice mode just makes you respawn on death in case you want to learn/practice the fight but don't want to do a whole other run (infinite dio's). The trophy item won't spawn at the end of the fight. The mode deactivates after Mithrix dies in Phase 4.

### Config Info

You can edit config values in-game and during runs through Settings -> Mod Options -> UmbralMithrix. Stats are calculated at the start of each phase so you can change in-game configs up until the phase starts. If the in-game values are too limiting you can edit the modman config to input custom values.

## Credits

- Race for ideas, feedback, testing, and writing the trophy item's logbook entry
- breadguy for the trophy item idea
- Everyone else who gave feedback, ideas, and bug reports

## Changelog

**2.1.5**

- Fixed multiplayer P4 sending out multiple pizzas/shockwaves (fr this time)
- Fixed multiplayer skyleap wonky targeting (indicator spawns on one player but lands on another)
- Updated R2API Content Management dependency

**2.1.4**

- MOONSTORM/STARSTORM BUG FIXED FINALLY (THANK YOU NEBBY!!!) (ALSO ANY OTHER WEIRD INCOMPATS SHOULD BE FIXED)
- Fixes P4 pizzas not working if you die in practice mode
- Fixes multiplayer P4 releasing multiple shockwaves (maybe)

**2.1.3**

- Reduces P3 clone HP significantly
- Fixes leap indicator lingering if Mithrix dies while leaping

**2.1.2**

- Fixes Moonstorm/Starstorm bug (fr this time)
- Swaps P3 crystals with glass clones
- Changes how some things work internally to fix the bug

**2.1.1**

- Adds semi-tracking leap (jumps to a random player position)
- Fixes P4 intervals being way too slow
- Fixes P2 immunity threshold still being bypassed
- Fixes boss subtitle not having the 2 clouds
- Reduces P1 clone duration 12 -> 6 secs
- Adds sound cue to leap

**2.1.0**

- Removes Bonfire mode
- Fixes Starstorm/Moonstorm P3 crystal bug
- Fixes P2/P3 immune threshold not working
- Fixes Multiplayer issues (hopefully)
  - P3 crystals not disappearing
  - Leap spawning multiple shockwaves and only tracking the host
  - P4 shockwaves firing multiple times
  - P3/P4 pizza only tracking the host
- Balance changes
  - P1 clones spawn less frequently but last longer to make it less chaotic
  - Changes leap from tracking to in-place (jumps and lands in the same spot)
  - P3 drones can be bought back (all drones are still destroyed at the beginning of the phase)
  - P4 Tweaks
    - Kills all allies after items are stolen
    - Reduces shockwave interval 6 -> 5.75 seconds
    - Reduces missile interval 3 -> 1 second
    - Reduces pizza damage and force by 50%
    - Reduces shockwave force by 50%
  - Retalitory super shards are now chance based
    - 50% chance to fire a shard when frozen
    - 25% chance to fire a shard when nullified (tentabauble)
