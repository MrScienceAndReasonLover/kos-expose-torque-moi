using System;
using System.Collections.Generic;
using UnityEngine;

namespace KspPhysicsUtils.Torque
{
    public static class RcsTorqueHelper
    {
        /// <summary>
        /// Computes available RCS torque in vessel coordinates (kN·m), split into positive and negative bins per axis.
        /// Axis meaning is the vessel reference transform local axes:
        ///   +X = right, +Y = up, +Z = forward (Unity/KSP convention).
        ///
        /// Returns:
        ///   pos = ( +X, +Y, +Z ) torque components (kN·m)
        ///   neg = ( -X, -Y, -Z ) torque components as NEGATIVE values (kN·m) to match kRPC style.
        /// </summary>
        public static Tuple<Vector3d, Vector3d> AvailableTorqueRcs(Vessel vessel)
        {
            if (vessel == null) throw new ArgumentNullException(nameof(vessel));

            Vector3d pos = Vector3d.zero;
            Vector3d neg = Vector3d.zero;

            // Match kRPC "Active" style gating at the module level.
            // (kRPC checks action group RCS, part shielded, rcsEnabled, module enabled, and !isJustForShow)
            bool rcsActionGroupOn = vessel.ActionGroups != null &&
                                    vessel.ActionGroups.groups != null &&
                                    vessel.ActionGroups[KSPActionGroup.RCS];

            if (!rcsActionGroupOn)
                return new Tuple<Vector3d, Vector3d>(pos, neg);

            // Vessel-fixed frame: use the vessel's reference transform.
            // We'll compute r and F in this local frame, then tau = r x F.
            Transform vRef = vessel.ReferenceTransform;
            //Vector3d comWorld = vessel.findWorldCenterOfMass();
            Vector3d comWorld = vessel.CoM;


            // Grab ModuleRCS instances (you can add ModuleRCSFX similarly if needed).
            List<ModuleRCS> modules = vessel.FindPartModulesImplementing<ModuleRCS>();
            foreach (var rcs in modules)
            {
                if (rcs == null) continue;

                Part p = rcs.part;
                if (p == null) continue;

                // kRPC-style Active check:
                if (p.ShieldedFromAirstream) continue;
                if (!rcs.rcsEnabled) continue;
                if (!((PartModule)rcs).isEnabled) continue;
                if (rcs.isJustForShow) continue;

                // Compute max thrust per nozzle in kN (kRPC uses MaxThrust in N; we stay in kN).
                // kRPC accounts for thrust limiter + atmospheric conditions via GetThrust(..., staticPressurekPa).
                double maxThrustKn = MaxThrustPerNozzle(rcs, vessel);

                if (maxThrustKn <= 0.0) continue;

                // Each nozzle/transform:
                // KSP stores them in rcs.thrusterTransforms. kRPC wraps these as Thrusters.
                var transforms = rcs.thrusterTransforms;
                if (transforms == null) continue;

                for (int i = 0; i < transforms.Count; i++)
                {
                    Transform t = transforms[i];
                    if (t == null) continue;

                    // --- Position vector r in vessel frame (meters) relative to CoM ---
                    Vector3d rWorld = (Vector3d)t.position - comWorld;
                    Vector3d rLocal = (Vector3d)vRef.InverseTransformDirection((Vector3)rWorld);

                    // --- Force direction in world space ---
                    // This mirrors kRPC Thruster.WorldThrustDirection for RCS:
                    //   if rcs.useZaxis -> (-transform.forward)
                    //   else            -> (-transform.up)
                    Vector3 forceDirWorld = rcs.useZaxis ? (-t.forward) : (-t.up);

                    // Convert to vessel frame:
                    Vector3d forceDirLocal = (Vector3d)vRef.InverseTransformDirection(forceDirWorld);

                    // Force magnitude (kN) times unit direction -> force vector (kN)
                    Vector3d F = forceDirLocal * maxThrustKn;

                    // Torque (kN·m)
                    Vector3d tau = Vector3d.Cross(rLocal, F);

                    // Respect the module axis toggles (kRPC gates each component by enablePitch/enableRoll/enableYaw).
                    // Here we gate X/Y/Z components; if you want to interpret them as pitch/roll/yaw,
                    // do that mapping in your addon layer.
                    AccumulateComponent(rcs.enablePitch, tau.x, ref pos.x, ref neg.x); // X
                    AccumulateComponent(rcs.enableRoll, tau.y, ref pos.y, ref neg.y); // Y
                    AccumulateComponent(rcs.enableYaw, tau.z, ref pos.z, ref neg.z); // Z
                }
            }

            // Match kRPC sign convention: neg vector holds NEGATIVE values.
            return new Tuple<Vector3d, Vector3d>(
                new Vector3d(pos.x, pos.y, pos.z),
                new Vector3d(-neg.x, -neg.y, -neg.z)
            );
        }

        /// <summary>
        /// Adds a signed component into (+) and (-) bins (stored as positive magnitudes in neg accumulator).
        /// </summary>
        private static void AccumulateComponent(bool enabled, double value, ref double posAcc, ref double negAcc)
        {
            if (!enabled) return;

            if (value >= 0.0) posAcc += value;
            else negAcc += -value; // store magnitude; we'll negate at the end
        }

        /// <summary>
        /// Approximation of kRPC's RCS.MaxThrust per nozzle, but in kN instead of N.
        /// Uses:
        ///   - rcs.thrusterPower (kN)
        ///   - rcs.thrustPercentage (0..100)
        ///   - rcs.atmosphereCurve evaluated at pressure in atm (staticPressurekPa / 101.325)
        ///
        /// NOTE: This does not currently zero thrust for "no fuel" cases.
        /// If you want exact kRPC behavior, we can add their connected-resource update logic next.
        /// </summary>
        private static double MaxThrustPerNozzle(ModuleRCS rcs, Vessel vessel)
        {
            // Thrust limiter 0..1
            double limiter = rcs.thrustPercentage / 100.0;

            // pressure in atm for atmosphereCurve: 0=vaccum, 1=Kerbin sea level
            double pressureAtm = 0.0;
            if (vessel != null)
                pressureAtm = vessel.staticPressurekPa / 101.325;

            double isp = 0.0;
            if (rcs.atmosphereCurve != null)
                isp = rcs.atmosphereCurve.Evaluate((float)pressureAtm);

            // kRPC: thrustN = 1000 * maxFuelFlow * throttle * G * isp
            // In kN, drop the *1000:
            double throttle = 1.0; // "available" torque, full authority
            double thrustKn = rcs.maxFuelFlow * throttle * rcs.G * isp;

            return thrustKn * limiter;
        }
    }
}
