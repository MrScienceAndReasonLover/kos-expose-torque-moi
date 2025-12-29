using CommNet.Network;
using kOS.AddOns.Inertia.Torque;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.Suffixed;
using UnityEngine;
using UnityEngine.Rendering;

namespace kOS.AddOns.Inertia.Structures
{
    [KOSNomenclature("TorqueProviders")]
    public class TorqueProviders : Structure
    {
        private readonly TorquePair total;
        private readonly TorquePair reactionwheel;
        private readonly TorquePair rcs;
        private readonly TorquePair engine;
        private readonly TorquePair controlsurface;

        public TorqueProviders(TorquePair total,TorquePair reactionwheel, TorquePair rcs, TorquePair engine, TorquePair controlsurface)
        {
            this.total = total;
            this.reactionwheel = reactionwheel;
            this.rcs = rcs;
            this.engine = engine;
            this.controlsurface = controlsurface;
            InitializeSuffixes();
        }
        private void InitializeSuffixes()
        {
            AddSuffix("total", new NoArgsSuffix<TorquePair>(() => total));
            AddSuffix("reactionwheel", new NoArgsSuffix<TorquePair>(() => reactionwheel));
            AddSuffix("rcs", new NoArgsSuffix<TorquePair>(() => rcs));
            AddSuffix("engine", new NoArgsSuffix<TorquePair>(() => engine));
            AddSuffix("controlsurface", new NoArgsSuffix<TorquePair>(() => controlsurface));                    
        }
    }

    [KOSNomenclature("TorquePair")]
    public class TorquePair : Structure
    {
        private readonly VecRollPitchYaw pos;
        private readonly VecRollPitchYaw neg;

        public TorquePair(Vector3 pos, Vector3 neg)
        {
            this.pos = new VecRollPitchYaw(pos);
            this.neg = new VecRollPitchYaw(neg);

            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("pos", new NoArgsSuffix<VecRollPitchYaw>(() => pos));
            AddSuffix("neg", new NoArgsSuffix<VecRollPitchYaw>(() => neg));
        }
    }
    [KOSNomenclature("VecRollPitchYaw")]
    public class VecRollPitchYaw : Structure
    {
        private readonly Vector vec;

        public VecRollPitchYaw(Vector3 vec)
        {
            this.vec = new Vector(vec.x, vec.y, vec.z);

            InitializeSuffixes();
        }
        private void InitializeSuffixes()
        {
            AddSuffix("vector", new NoArgsSuffix<Vector>(() => vec));
            AddSuffix("roll", new NoArgsSuffix<ScalarDoubleValue>(() => new ScalarDoubleValue(vec.Y)));
            AddSuffix("pitch", new NoArgsSuffix<ScalarDoubleValue>(() => new ScalarDoubleValue(vec.X)));
            AddSuffix("yaw", new NoArgsSuffix<ScalarDoubleValue>(() => new ScalarDoubleValue(vec.Z)));
        }
    }
}