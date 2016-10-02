using System.Collections.Generic;

namespace CsPaxos
{
    public interface ISystem<T>
    {
        IReadOnlyList<IProcess<T>> Processes { get; }
    }

    public interface IProcess<T>
    {
        bool IsValueChosen();
        T Value();
        void Propose(T value);
    }
}
