using CommNet.Network;
using Contracts.Agents.Mentalities;
using kOS.AddOns.Inertia.Structures;
using kOS.Safe;
using kOS.Safe.Encapsulation;
using kOS.Suffixed;
using KspPhysicsUtils.Torque;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace kOS.AddOns.Inertia.Torque
{
    public static class InertiaTorque
    {
        public static TorqueProviders GetAll(SharedObjects shared)
        {
            var v = shared.Vessel;
            if (v == null)
            {
                var zero = new TorquePair(Vector3.zero, Vector3.zero);
                return new TorqueProviders(zero, zero, zero, zero, zero);
            }

            var wheels = v.FindPartModulesImplementing<ModuleReactionWheel>()
                          .Cast<ITorqueProvider>();
            TorqueSums.SumPotentialTorque(wheels, out Vector3 rwPos, out Vector3 rwNeg);
            rwNeg = -rwNeg; // match sign convention

            var surfaces = v.FindPartModulesImplementing<ModuleControlSurface>()
                            .Cast<ITorqueProvider>();
            TorqueSums.SumPotentialTorque(surfaces, out Vector3 csPos, out Vector3 csNeg);

            var gimbals = v.FindPartModulesImplementing<ModuleGimbal>().Cast<ITorqueProvider>(); // may be empty on some builds
            TorqueSums.SumPotentialTorque(gimbals, out Vector3 engPos, out Vector3 engNeg);
            engNeg = -engNeg; // match sign convention

            var rcsKnM = RcsTorqueHelper.AvailableTorqueRcs(v); // Tuple<Vector3d, Vector3d>
            Vector3 rcsPos = new Vector3((float)rcsKnM.Item1.x, (float)rcsKnM.Item1.y, (float)rcsKnM.Item1.z);
            Vector3 rcsNeg = new Vector3((float)rcsKnM.Item2.x, (float)rcsKnM.Item2.y, (float)rcsKnM.Item2.z);

            // 5) Total
            Vector3 totalPos = rwPos + csPos + engPos + rcsPos;
            Vector3 totalNeg = rwNeg + csNeg + engNeg + rcsNeg;

            // 6) Wrap into kOS structures once
            var total = new TorquePair(totalPos, totalNeg);
            var rw = new TorquePair(rwPos, rwNeg);
            var rcs = new TorquePair(rcsPos, rcsNeg);
            var eng = new TorquePair(engPos, engNeg);
            var cs = new TorquePair(csPos, csNeg);

            return new TorqueProviders(total, rw, rcs, eng, cs);
        }
        public static TorquePair AvailableTorqueTotal(SharedObjects shared)
        {
            var v = shared.Vessel;
            if (v == null)
                return new TorquePair(Vector3.zero, Vector3.zero);
            var wheels = v.FindPartModulesImplementing<ModuleReactionWheel>();
            var gimbs = v.FindPartModulesImplementing<ModuleGimbal>();
            var controlSurfaces = v.FindPartModulesImplementing<ModuleControlSurface>();
            Vector3 posTotal = Vector3.zero;
            Vector3 negTotal = Vector3.zero;
            foreach (var wheel in wheels)
            {
                wheel.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }
            foreach (var gimb in gimbs)
            {
                gimb.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }
            foreach (var cf in controlSurfaces)
            {
                cf.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }
            var t = RcsTorqueHelper.AvailableTorqueRcs(shared.Vessel);
            posTotal += t.Item1;
            negTotal += t.Item2;
            return new TorquePair(posTotal, negTotal);
        }

        public static TorquePair AvailableTorqueReactionWheel(SharedObjects shared)
        {
            var v = shared.Vessel;
            if (v == null)
                return new TorquePair(Vector3.zero, Vector3.zero);

            var wheels = v.FindPartModulesImplementing<ModuleReactionWheel>();
            Vector3 posTotal = Vector3.zero;
            Vector3 negTotal = Vector3.zero;
            foreach (var wheel in wheels)
            {
                wheel.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }

            return new TorquePair(posTotal, negTotal);
        }

        public static TorquePair AvailableTorqueRCS(SharedObjects shared)
        {
            var v = shared.Vessel;
            if (v == null)
                return new TorquePair(Vector3.zero, Vector3.zero);

            var t = RcsTorqueHelper.AvailableTorqueRcs(shared.Vessel);
            Vector3d posTotal = t.Item1;
            Vector3d negTotal = t.Item2;

            return new TorquePair(posTotal, negTotal);
        }


        public static TorquePair AvailableTorqueEngine(SharedObjects shared)
        {

            var v = shared.Vessel;
            if (v == null)
                return new TorquePair(Vector3.zero, Vector3.zero);

            var gimbs = v.FindPartModulesImplementing<ModuleGimbal>();

            Vector3 posTotal = Vector3.zero;
            Vector3 negTotal = Vector3.zero;

            foreach (var gimb in gimbs)
            {
                gimb.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }

            return new TorquePair(posTotal, negTotal);
        }
        public static TorquePair AvailableTorqueControlSurface(SharedObjects shared)
        {
            var v = shared.Vessel;
            if (v == null)
                return new TorquePair(Vector3.zero, Vector3.zero);

            var controlSurfaces = v.FindPartModulesImplementing<ModuleControlSurface>();

            Vector3 posTotal = Vector3.zero;
            Vector3 negTotal = Vector3.zero;

            foreach (var controlSurface in controlSurfaces)
            {
                controlSurface.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }

            return new TorquePair(posTotal, negTotal);
        }
    }
    public static class TorqueSums
    {
        public static void SumPotentialTorque(
            IEnumerable<ITorqueProvider> providers,
            out Vector3 posTotal,
            out Vector3 negTotal)
        {
            posTotal = Vector3.zero;
            negTotal = Vector3.zero;

            foreach (var tp in providers)
            {
                if (tp == null) continue;

                tp.GetPotentialTorque(out Vector3 pos, out Vector3 neg);
                posTotal += pos;
                negTotal += neg;
            }
        }
    }
}
