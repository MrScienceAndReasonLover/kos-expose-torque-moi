# kOS Expose Torque & Moment of Inertia

A **kOS addon** for Kerbal Space Program that exposes a vessel’s **moment of inertia** and **available control torque** (reaction wheels, RCS, control surfaces, engines) directly to kOS scripts.

This addon is intended for **guidance, navigation, and control (GNC)** scripts that need physically meaningful vehicle properties instead of hard-coded gains or guesswork.

---

## Features

- Exposes vessel **moment of inertia** (about roll, pitch, yaw axes)
- Exposes **available control torque**, broken down by source:
  - Reaction wheels
  - RCS
  - Control surfaces
  - Engine gimbals
- Provides **total available torque** as a convenience
- RCS torque is computed **geometrically per thruster**, matching kRPC behavior
- Values update dynamically as the vessel changes

---

## Installation

1. Download the release ZIP
2. Extract into your KSP install so you have:

```
GameData/
└── kOS_Inertia/
    └── Plugins/
        └── InertiaAddon.dll
```

3. Make sure **kOS is installed**
4. Start KSP

---

## Usage in kOS

All functionality is exposed via `ADDONS:INERTIA`.

### Moment of Inertia

```kos
SET moi TO ADDONS:INERTIA:MOI.
PRINT moi:ROLL.
PRINT moi:PITCH.
PRINT moi:YAW.
```

- Units: **t·m²** (KSP internal units)
- Axes are aligned with the vessel reference frame: roll, pitch, yaw

---

### Available Torque

```kos
SET torque TO ADDONS:INERTIA:TORQUE.

PRINT torque:TOTAL:POS.
PRINT torque:TOTAL:NEG.

PRINT torque:RCS:POS:ROLL.
PRINT torque:REACTIONWHEEL:POS:PITCH.
```

Each torque provider exposes:

- `:POS` — positive-direction torque components
- `:NEG` — negative-direction torque components

Each of `:POS` and `:NEG` exposes:

- `:ROLL`
- `:PITCH`
- `:YAW`
- `:VECTOR`

---

### Example

```kos
SET t TO ADDONS:INERTIA:TORQUE.

SET rollAuthority TO t:TOTAL:POS:ROLL + t:TOTAL:NEG:ROLL.
PRINT "Available roll torque: " + rollAuthority.
```

---

## Notes on Physics & Interpretation

- **RCS torque** is computed per thruster using geometry (`τ = r × F`) in the vessel reference frame.
  - This avoids inconsistencies in KSP’s built-in torque estimates
  - Matches kRPC’s interpretation of available RCS torque
- **Total available torque** is a *control authority estimate*.
  - Not all torque sources can be applied simultaneously in practice (surfaces need airspeed, gimbals need thrust, RCS needs propellant, etc.)
  - Intended for controller design and guidance logic, not exact prediction

---

## Requirements

- Kerbal Space Program
- kOS

---

## Compatibility

- Designed for modern KSP + kOS
- Tested with multiple vessels simultaneously
- Does **not** depend on kRPC

---

## License

MIT License

---

## Motivation

This addon exists because:

- kOS does not natively expose moment of inertia
- Torque availability is critical for control design
- RCS torque in particular is difficult to reason about correctly

If you are writing:

- attitude controllers
- adaptive control laws
- guidance algorithms
- physically grounded autopilots

…this addon is for you.

---

## Acknowledgements

- Inspired by kRPC’s torque and reference-frame handling
- Thanks to the kOS community for making deep scripting possible in KSP
