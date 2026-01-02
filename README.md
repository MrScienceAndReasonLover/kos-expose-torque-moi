# Expose Torque & Moment of Inertia to kOS

A **kOS addon** for Kerbal Space Program that exposes a vessel’s **moment of inertia** and **available control torque** (reaction wheels, RCS, control surfaces, engines) directly to kOS scripts.

---

## Usage in kOS

All functionality is exposed via `ADDONS:EX`.

### Moment of Inertia
| Command                 | Return Type | Units                   | Description                       |
|-------------------------|-------------|-------------------------|-----------------------------------|
|`MomentOfInertia:VECTOR` | Vector      |  $\text{tons}\cdot m^2$ | Moment of Inertia about ship axes |
|`MomentOfInertia:ROLL`   | ScalarValue |  $\text{tons}\cdot m^2$ | Moment of Inertia about roll axis |
|`MomentOfInertia:PITCH`  | ScalarValue |  $\text{tons}\cdot m^2$ | Moment of Inertia about pitch axis|
|`MomentOfInertia:YAW`    | ScalarValue |  $\text{tons}\cdot m^2$ | Moment of Inertia about yaw axis  |


The moment of inertia vector is expressed in KSP's native coordinate system where x is starboard, y is the direction the ship points, and z is down. This is different than what kOS uses, so I've also included suffixes that give you the moments in terms of roll, pitch, and yaw, which should be easier to understand. Also note that moment of inertia is a 3x3 tensor in reality, and I have not exposed the entire tensor because KSP does not do it for me. I could be persuaded to go through the math myself, but since ships are usually symmetric in some way, the off-diagonal terms should be pretty negligible. 

```kos
SET moi TO ADDONS:EX:MOMENTOFINERTIA.

PRINT moi:ROLL.
PRINT moi:PITCH.
PRINT moi:YAW.
```
---

### Available Torque
Accessing torque is a little more complicated since there are multiple providers and each can have different values in the positive and negative directions. The available fields are
| Command                        | Return Type | Units                   | Description                            |
|--------------------------------|-------------|-------------------------|----------------------------------------|
|`AvailableTorque:TOTAL`         | TorquePair  |  $kN\cdot m^2$          | Total available torque                 |
|`AvailableTorque:REACTIONWHEEL` | TorquePair  |  $kN\cdot m^2$          | Available torque from reaction wheels  | 
|`AvailableTorque:RCS`           | TorquePair  |  $kN\cdot m^2$          | Available torque from RCS              |
|`AvailableTorque:ENGINE`        | TorquePair  |  $kN\cdot m^2$          | Available torque from engine gimbal    |
|`AvailableTorque:CONTROLSURFACE`| TorquePair  |  $kN\cdot m^2$          | Available torque from control surfaces |

Each torque pair has fields:

- `:POS` — positive-direction torque components
- `:NEG` — negative-direction torque components

Each of `:POS` and `:NEG` has the same fields as moment of inertia:
- `:VECTOR`
- `:ROLL`
- `:PITCH`
- `:YAW`
```kos
SET torque TO ADDONS:EX:AVAILABLETORQUE.

PRINT torque:total:pos:roll.
PRINT torque:total:pos:pitch.
PRINT torque:total:pos:yaw.

PRINT torque:total:neg:roll.
PRINT torque:total:neg:pitch.
PRINT torque:total:neg:yaw.
```

---

## Requirements
- kOS v 1.5.1.0

---
## Motivation

If I were using kOS to learn how to design control systems, the first thing I would do is to try recreate what the cooked control system is doing. I would go to the kOS docs and find that they calculate the PID gains for the inner loop as 

$k_i = I\left(\frac{4}{t_s}\right)^2$

$k_p = 2\sqrt{I} k_i$ 

Where $I$ is the ship's moment of inertia (about the control axis), and $t_s$ is the desired settling time. Since this PID loop calculates torque, you also need to normalize by the total torque to send a command to say, `SHIP:CONTROL:PILOTPITCH`. Unfortunately, there is no way to do this in standard kOS. You can estimate moment of inertia fairly easily by looping through parts, but estimating torque is much more difficult since things like reaction wheel torque, rcs force, control surface area, and maximum engine gimbal are not exposed to the user in any way. Since real aerospace engineers certainly have models of their vessel's moment of inertia and available torque, I see no reason to limit kOS users in this way, especially since the stock system takes advantage of this knowledge. 

---
## License
MIT LICENSE
---
