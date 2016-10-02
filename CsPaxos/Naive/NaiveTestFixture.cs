using FsCheck;
using System.Linq;

namespace CsPaxos.Naive
{
    public class NaiveTestFixture : ConsensusTestFixture
    {
        private class SystemGenerator
        {
            public static Arbitrary<ISystem<T>> TestCaseGenerator<T>()
            {
                var generator = Gen.Choose(1, 32)
                    .Select(i => NaiveEntryPoint.System<T>(i));
                return Arb.From(generator);
            }
        }

        protected override void RegisterSystemGenerator()
        {
            Arb.Register<SystemGenerator>();
        }
    }

    public class DistributedNaiveTestFixture : ConsensusTestFixture
    {
        private class SystemGenerator
        {
            public static Arbitrary<ISystem<T>> TestCaseGenerator<T>()
            {
                var generator = Gen.Choose(1, 32)
                    .Select(i => NaiveEntryPoint.DistributedSystem<T>(i));
                return Arb.From(generator);
            }
        }

        protected override void RegisterSystemGenerator()
        {
            Arb.Register<SystemGenerator>();
        }
    }
}
