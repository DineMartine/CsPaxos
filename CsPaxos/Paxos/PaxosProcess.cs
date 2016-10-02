using Monad;
using System;
using System.Linq;

namespace CsPaxos.Paxos
{
    internal class PaxosProcess<T> : IProcess<T>, IPaxosProcess<T>
    {
        private Option<T> learnedValue = Option.Nothing<T>();
        private Option<int> promisedNumber = Option.Nothing<int>();
        private Option<T> acceptedValue = Option.Nothing<T>();

        private readonly IInternalSystem<IPaxosProcess<T>> system;
        private readonly IPaxosNumberGenerator generator;

        public PaxosProcess(IInternalSystem<IPaxosProcess<T>> system, IPaxosNumberGenerator generator)
        {
            this.system = system;
            this.generator = generator;
        }

        public bool IsValueChosen()
        {
            return learnedValue.HasValue();
        }

        public T Value()
        {
            return learnedValue.Value();
        }

        public void Propose(T value)
        {
            var number = generator.Generate();
            var promises = system.InternalProcesses.Select(p => p.Prepare(number))
                .Where(p => p.IsPromise).ToArray();
            if (promises.Length >= system.Quorum)
            {
                var lastAcceptedProposals = promises.Select(p => p.LastAcceptedProposal())
                    .Where(p => p.HasValue).Select(p => p.Value);
                if (lastAcceptedProposals.Any())
                {
                    var maxNumber = lastAcceptedProposals.Max(p => p.Number);
                    value = lastAcceptedProposals.First(p => p.Number == maxNumber).Value;
                }
                var acceptCount = system.InternalProcesses.Where(p => p.Accept(number, value)).Count();
                if (acceptCount >= system.Quorum)
                {
                    foreach (var process in system.InternalProcesses)
                    {
                        process.Learn(value);
                    }
                }
            }
        }

        public Promise<T> Prepare(int number)
        {
            if (!promisedNumber.HasValue() || promisedNumber.Value() < number)
            {
                var lastAcceptedProposal = promisedNumber.SelectMany(n => acceptedValue, (n, v) => new Proposal<T>(n, v));
                promisedNumber = Option.Return(() => number);
                return Promise<T>.MakePromise(lastAcceptedProposal);
            }

            return Promise<T>.Refuse();
        }

        public bool Accept(int number, T value)
        {
            if (!promisedNumber.HasValue() || promisedNumber.Value() <= number)
            {
                acceptedValue = Option.Return(() => value);
                return true;
            }

            return false;
        }

        public void Learn(T value)
        {
            learnedValue = Option.Return(() => value);
        }
    }

    internal interface IPaxosNumberGenerator
    {
        int Generate();
    }

    internal class SequentialNumberGenerator : IPaxosNumberGenerator
    {
        private Random rand = new Random(); 

        public int Generate()
        {
            return rand.Next();
        }
    }
}
