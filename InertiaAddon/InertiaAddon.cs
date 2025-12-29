using kOS;
using kOS.AddOns;

using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.Suffixed;

using kOS.AddOns.Inertia.Vessel;
using kOS.AddOns.Inertia.Torque;
using kOS.AddOns.Inertia.Structures;

namespace kOS.AddOns.Inertia
{
    [kOSAddon("ex")]
    [KOSNomenclature("InertiaTorqueExpose")]
    public class InertiaAddon : Addon
    {
        public InertiaAddon(SharedObjects shared) : base(shared)
        {
            InitializeSuffixes();
        }
        public override BooleanValue Available()
        {
            return BooleanValue.True;
        }
        private void InitializeSuffixes()
        {
            AddSuffix("MomentOfInertia", new NoArgsSuffix<VecRollPitchYaw>(() => InertiaVessel.MomentOfInertia(shared)));
            AddSuffix("AvailableTorque", new NoArgsSuffix<TorqueProviders>(() => InertiaTorque.GetAll(shared))) ;
        }
    }
}