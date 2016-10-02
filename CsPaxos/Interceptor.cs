using Castle.DynamicProxy;
using System.Threading.Tasks;

namespace CsPaxos
{
    public class SynchronizedInterceptor : IInterceptor
    {
        private readonly object mutex = new object();

        public void Intercept(IInvocation invocation)
        {
            lock (mutex)
            {
                invocation.Proceed();
            }
        }
    }

    public class AsyncProxyProcess<T> : IProcess<T>
    {
        private readonly IProcess<T> innerProcess;

        public AsyncProxyProcess(IProcess<T> innerProcess)
        {
            this.innerProcess = innerProcess;
        }

        public bool IsValueChosen()
        {
            return innerProcess.IsValueChosen();
        }

        public void Propose(T value)
        {
            Task.Run(() => innerProcess.Propose(value));
        }

        public T Value()
        {
            return innerProcess.Value();
        }
    }
}
