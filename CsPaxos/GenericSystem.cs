using System;
using System.Collections.Generic;

namespace CsPaxos
{
    public interface IInternalSystem<TInternalProcess>
    {
        IReadOnlyList<TInternalProcess> InternalProcesses { get; }
        int Quorum { get; }
    }

    public class GenericSystem<TValue, TInternalProcess> : ISystem<TValue>, IInternalSystem<TInternalProcess>
    {
        private readonly List<IProcess<TValue>> processes = new List<IProcess<TValue>>();
        private readonly List<TInternalProcess> internalProcesses = new List<TInternalProcess>();

        public void AddProcess(IProcess<TValue> process, TInternalProcess internalProcess)
        {
            processes.Add(process);
            internalProcesses.Add(internalProcess);
        }

        public IReadOnlyList<IProcess<TValue>> Processes
        {
            get
            {
                return processes;
            }
        }

        public IReadOnlyList<TInternalProcess> InternalProcesses
        {
            get
            {
                return internalProcesses;
            }
        }

        public int Quorum
        {
            get
            {
                return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(internalProcesses.Count) / 2.0));
            }
        }
    }
}
