# VRC-Omni-Action
Multi-purpose user-action/event and function-handling component for VRChat.

![Omni-Action](P_OmniAction.png)

## Overview
Omni-Action is a single configurable UdonSharp script that can be used for a growing multitude of VRChat world interactions and game-logic functions. It is intended to be efficient and relatively simple to use without the need for any additional editor scripts or dependencies outside of UdonSharp. All configuration including networking and event routing can be done within the Inspector tab without the need for any programming knowledge. That said, the source code is cleanly-organized and commented in the hopes of also being a good learning tool, and there are few limitations imposed on the level of complexity you can achieve.

### Requirements
- [VRChat Worlds SDK3](https://vrchat.com/home/download) (Tested: v2021.11.8)
- [UdonSharp](https://github.com/MerlinVR/UdonSharp) (Tested: v0.20.3)

### Optional
- [VRC Haptics Profile](https://github.com/Pokeyi/VRC-Haptics-Profile) (Controller Vibration)
- [CyanEmu](https://github.com/CyanLaser/CyanEmu) (Unity-Window Testing)
- [Udon AudioLink](https://github.com/llealloo/vrc-udon-audio-link) (Audio Data Input)

## Features
The main features of Omni-Action can be broken down into three categories:
- Functions - What activity the behaviour will be performing with its target game objects each time it is activated.
- Actions - *How* the function is activated, be it via direct player interaction or other defined circumstances.
- Options - Additional settings to further customize or add functionality.

### Functions
- Events Only - No target-object functionality outside of options and event routing (detailed below).
- Pickup Reset - Reset target VRC-Pickup objects to their original positions and rotations from instance start.
- Binary Toggle - Swap current active status of all target objects.
- Sequence Toggle - Enable active status of one target object at a time (disabling all others) in sequence.
- Enable All - Enable active status of all target objects.
- Disable All - Disable active status of all target objects.
- Animator Toggle - Toggle specified boolean variable on target objects' animator components.
- Animator True - Enable specified boolean variable on target objects' animator components.
- Animator False - Disable specified boolean variable on target objects' animator components.
- Teleport Player - Teleport the player to the location of the first target object or in sequence if multiple.
- Teleport Object - Teleport the first target object to all target-object locations in sequence.
- Stopwatch - Toggle a stopwatch counter that outputs its accrued time to the Text component of the first target object.
If using both entry & exit trigger actions, stopwatch will start on exit and stop on re-entry. Otherwise, it will toggle.
- Object-Pool Spawn - Attempt to spawn next object from each target object's VRC-Object-Pool.
- Object-Pool Reset - Reset each target object's VRC-Object-Pool.

### Actions
- Button Interact - Activate when player clicks the button.
- Entry/Exit Trigger - Activate/deactivate when player or specified pickup object enters/exits trigger collider.
- Occupied Trigger - Activate when *any* player or specified pickup object enters trigger collider, deactivate when empty.
- On-Enable/Disable - Activate/deactivate when object's active status is changed. (See: Known Issues / Bugs - #1)
- Timer Repeat - Activate on a repeated timer, either random range or set interval.
- AudioLink - Activate when Udon AudioLink data meets conditions on specified audio band.
- All-Active Scan - Activate when all target objects found active, deactivate when this becomes untrue. (See: Known Issues / Bugs - #2)
- Remote Action - Activate remotely from another script.

### Options
- Is Global - Script will not explicitly sync objects or serialize data over the network for other players unless this is enabled. (See: Known Issues / Bugs - #3)
- One-Shot - When enabled, the function can only be activated one time. An optional "\_ReEnable" event can be called to reset this.
- Randomize Functions - Adds randomized aspect to certain functions. (Currently: Sequence Toggle, Player/Object Teleport, Object-Pool Spawn/Reset)
- Animator Bool - Name of the effected animator bool for relevant animator functions.
- Override Name Contains - Triggers will be activated by game objects containing this name *instead* of by players unless this field is left empty.
- Min/Max Timer - Range in seconds for 'Timer Repeat' action. For a set interval, leave 'Max Timer' set to 0, or set both fields to the same value.
- Delay Time - Delay final step of target object function for set number of seconds. Delay is observed *after* the action's audio, haptics, and events are fired.
- AudioLink - Only relevant if you are using Udon AudioLink data in your project. Threshold adjusts band sensitivity and applies an inverse time throttle (<1s).
- Entry/Exit - Functionality is split for certain actions that can behave differently whether they are being entered/exited or enabled/disabled, etc.
- Haptics Profile - Reference a [VRC Haptics Profile](https://github.com/Pokeyi/VRC-Haptics-Profile) to relay customized controller vibration to the player.
- Audio Source - Play sound from referenced audio source when function is activated. If 'Is Global' is enabled, all players within range will hear the sound as well.
- Events - These options enable you to call public methods / custom events on other scripts / behaviours. Global events will trigger for all players and are ignored if 'Is Global' is not enabled. Events are called sequentially for the receiver on the same numbered line in the inspector array. If calling both local and global events on different behaviours, leaving an event name blank will skip it for that receiver. As an example, you can trigger a remote action on another Omni-Action behaviour with the "\_RemoteAction" local event. (See: Known Issues / Bugs - #4)

## Use Cases / Examples
- WIP

## Known Issues / Bugs
As stated above, few limitations are imposed on what configurations can be made, but some combinations definitely won't play well together.
1. Spotty functionality and player-syncing if component is on an object that is subject to having its own active status disabled, for obvious reasons. Maybe just don't do that.
2. 'All-Active Scan' action performs its function on the same target objects it is reacting to, so you should probably choose 'Events Only' for most cases.
3. 'Is Global' should be enabled if you are manipulating target objects that are themselves network-synced or contain networked components like VRC-Object-Sync or VRC-Object-Pool. Nonlocal actions (On-Enable/Disable, All-Active Scan, AudioLink, Timer Repeat, Occupied Trigger) will appropriately be filtered to only activate once through the network owner.
4. Per the VRChat API, method/event names starting with an "\_Underscore" are protected from remote network calls, necessitating a local-only event.
5. Scan and timing functionality does not stack and the following will override eachother by priority: All-Active Scan > AudioLink > Timer Repeat > Stopwatch.
6. (Bug) 'Teleport Object' function does nothing on occasion due to being included in the sequence and teleporting to itself. Easy fix for the next release.
7. (Planned) Wider functionality for the Randomize option will be added in the next release.

## License

## Support
