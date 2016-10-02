using FsCheck;
using System.Linq;

namespace CsPaxos.Paxos
{
    public class PaxosTestFixture : ConsensusTestFixture
    {
        private class SystemGenerator
        {
            public static Arbitrary<ISystem<T>> TestCaseGenerator<T>()
            {
                var generator = Gen.Choose(1, 32)
                    .Select(i => PaxosEntryPoint.System<T>(i));
                return Arb.From(generator);
            }
        }

        protected override void RegisterSystemGenerator()
        {
            Arb.Register<SystemGenerator>();
        }
    }

    public class DistributedPaxosTestFixture : ConsensusTestFixture
    {
        private class SystemGenerator
        {
            public static Arbitrary<ISystem<T>> TestCaseGenerator<T>()
            {
                var generator = Gen.Choose(1, 32)
                    .Select(i => PaxosEntryPoint.DistributedSystem<T>(i));
                return Arb.From(generator);
            }
        }

        protected override void RegisterSystemGenerator()
        {
            Arb.Register<SystemGenerator>();
        }
    }
}
