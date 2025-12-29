using kOS.Safe;
using kOS.Safe.Encapsulation;
using kOS.Suffixed;
using UnityEngine;
using kOS.AddOns.Inertia.Structures;

namespace kOS.AddOns.Inertia.Vessel
{
    public static class InertiaVessel
    {
        public static VecRollPitchYaw MomentOfInertia(SharedObjects shared)
        {
            var v = shared.Vessel;
            if (v == null) return new VecRollPitchYaw(Vector3.zero);

            return new VecRollPitchYaw(v.MOI);
        }
    }
}