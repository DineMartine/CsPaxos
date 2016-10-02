using Castle.DynamicProxy;
using Monad;
using System.Linq;

namespace CsPaxos.Naive
{
    public static class NaiveEntryPoint
    {
        public static ISystem<T> System<T>(int processCount)
        {
            var system = new GenericSystem<T, INaiveProcess<T>>();
            foreach (var process in Enumerable.Range(0, processCount).Select(i => new NaiveProcess<T>(system)))
            {
                system.AddProcess(process, process);
            }
            return system;
        }

        public static ISystem<T> DistributedSystem<T>(int processCount)
        {
            var system = new GenericSystem<T, INaiveProcess<T>>();
            var proxyGenerator = new ProxyGenerator();
            foreach (var process in Enumerable.Range(0, processCount).Select(i => new NaiveProcess<T>(system)))
            {
                var systemProcess = new AsyncProxyProcess<T>(process);
                var naiveProcess = proxyGenerator.CreateInterfaceProxyWithTarget((INaiveProcess<T>)process, new SynchronizedInterceptor());
                system.AddProcess(systemProcess, naiveProcess);
            }
            return system;
        }
    }

    public interface INaiveProcess<T>
    {
        void Inform(T value);
    }

    internal class NaiveProcess<T> : IProcess<T>, INaiveProcess<T>
    {
        private readonly IInternalSystem<INaiveProcess<T>> system; 
        private Option<T> chosenValue = Option.Nothing<T>();

        public NaiveProcess(IInternalSystem<INaiveProcess<T>> system)
        {
            this.system = system;
        }

        public bool IsValueChosen()
        {
            return chosenValue.HasValue();
        }

        public T Value()
        {
            return chosenValue.Value();
        }

        public void Inform(T chosenValue)
        {
            this.chosenValue = Option.Return(() => chosenValue);
        }

        public void Propose(T value)
        {
           if (!chosenValue.HasValue())
           {
                foreach( var p in system.InternalProcesses)
                {
                    p.Inform(value);
                }
           }
        }
    }
}
