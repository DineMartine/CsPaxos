using FsCheck;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CsPaxos
{
    [TestFixture]
    public abstract class ConsensusTestFixture
    {
        [SetUp] public void SetUp()
        {
            RegisterSystemGenerator();
            Arb.Register<TestCaseGenerator>();
        }

        [FsCheck.NUnit.Property]
        public bool OnlyProposedValuesAreChosen(ConsensusTestCase<int> testCase)
        {
            testCase.Execute();
            var proposedValues = testCase.GetProposedValues();
            return testCase.GetChosenValues().All(v => proposedValues.Contains(v));

        }

        [FsCheck.NUnit.Property]
        public bool OnlyOneValueCanBeChosen(ConsensusTestCase<string> testCase)
        {
            testCase.Execute();
            return testCase.GetChosenValues().Distinct().Count() <= 1;
        }

        protected abstract void RegisterSystemGenerator();

        private class TestCaseGenerator
        {
            public static Arbitrary<ConsensusTestCase<T>> GenerateTestCase<T>()
            {
                var generator = Arb.Generate<ISystem<T>>()
                    .SelectMany(
                        s => Gen.Choose(0, s.Processes.Count - 1)
                            .SelectMany(a => Arb.Generate<T>(), Tuple.Create)
                            .ListOf()
                            .Select(l => new ReadOnlyCollection<Tuple<int, T>>(l)),
                        (s, p) => new ConsensusTestCase<T>(s, p));
                return Arb.From(generator);
            }
        }

        public class ConsensusTestCase<T>
        {
            private readonly ISystem<T> system;
            private readonly IReadOnlyCollection<Tuple<int, T>> proposals;

            public ConsensusTestCase(ISystem<T> system, IReadOnlyCollection<Tuple<int,T>> proposals)
            {
                this.system = system;
                this.proposals = proposals;
            }

            internal void Execute()
            {
                foreach ( var proposal in proposals)
                {
                    system.Processes[proposal.Item1].Propose(proposal.Item2);
                }
            }

            public IReadOnlyCollection<T> GetProposedValues()
            {
                return (IReadOnlyCollection<T>) new HashSet<T>(proposals.Select(p => p.Item2));
            }

            public IReadOnlyCollection<T> GetChosenValues()
            {
                return system.Processes.Where(p => p.IsValueChosen()).Select(p => p.Value()).ToArray();
            }
        }
    }
}
