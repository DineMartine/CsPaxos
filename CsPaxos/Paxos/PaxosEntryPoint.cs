using Castle.DynamicProxy;
using Monad;
using System.Linq;

namespace CsPaxos.Paxos
{
    public static class PaxosEntryPoint
    {
        public static ISystem<T> System<T>(int processCount)
        {
            var system = new GenericSystem<T, IPaxosProcess<T>>();
            var generator = new SequentialNumberGenerator();
            foreach(var process in Enumerable.Range(0, processCount).Select(i => new PaxosProcess<T>(system, generator)))
            {
                system.AddProcess(process, process);
            }
            return system;
        }

        public static ISystem<T> DistributedSystem<T>(int processCount)
        {
            var system = new GenericSystem<T, IPaxosProcess<T>>();
            var generator = new SequentialNumberGenerator();
            var proxyGenerator = new ProxyGenerator();
            foreach (var process in Enumerable.Range(0, processCount).Select(i => new PaxosProcess<T>(system, generator)))
            {
                var systemProcess = new AsyncProxyProcess<T>(process);
                var paxosProcess = proxyGenerator.CreateInterfaceProxyWithTarget((IPaxosProcess<T>)process, new SynchronizedInterceptor());
                system.AddProcess(systemProcess, paxosProcess);
            }
            return system;
        } 
    }

    public interface IPaxosProcess<T>
    {
        Promise<T> Prepare(int number);
        bool Accept(int number, T value);
        void Learn(T value);
    }

    public class Promise<T>
    {
        public bool IsPromise;
        public Option<Proposal<T>> LastAcceptedProposal;

        public static Promise<T> MakePromise(Option<Proposal<T>> lastAcceptedProposal)
        {
            return new Promise<T>(true, lastAcceptedProposal);
        }

        public static Promise<T> Refuse()
        {
            return new Promise<T>(false, Option.Nothing<Proposal<T>>());
        }

        private Promise(bool isPromise, Option<Proposal<T>> lastAcceptedProposal)
        {
            IsPromise = isPromise;
            LastAcceptedProposal = lastAcceptedProposal;
        }
    }

    public struct Proposal<T>
    {
        public readonly int Number;
        public readonly T Value;

        public Proposal(int number, T value)
        {
            Number = number;
            Value = value;
        }
    }
}
