using CsPaxos.Naive;
using CsPaxos.Paxos;
using System;
using System.Linq;
using System.Threading;

namespace CsPaxos
{
    class Program
    {
        static void Main(string[] args)
        {
            var all = true;
            while(all)
            {
                var system = PaxosEntryPoint.DistributedSystem<string>(10);
                system.Processes[9].Propose("Obama");
                system.Processes[1].Propose("b");
                system.Processes[6].Propose("Clinton");
                system.Processes[2].Propose("Sarkozy");
                system.Processes[2].Propose("Trump");
                system.Processes[4].Propose("Sanders");
                system.Processes[3].Propose("g");
                system.Processes[7].Propose("h");
                system.Processes[3].Propose("i");
                Thread.Sleep(10);
                var valueChosen = system.Processes.Where(p => p.IsValueChosen()).Select(p => p.Value()).ToArray();
                if(valueChosen.Any())
                {
                    Console.WriteLine(string.Join(" ", valueChosen));
                    var firstValueChosen = valueChosen.First();
                    all = valueChosen.All(v => v.Equals(firstValueChosen));
                }
            }
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
