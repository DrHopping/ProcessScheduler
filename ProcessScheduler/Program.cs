using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    public class ProcessComparer : IComparer<Process>
    {
        public int Compare(Process x, Process y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.ExecutionTime.CompareTo(y.ExecutionTime);
        }
    }

    public class RandomProcessGenerator
    {
        private readonly Random _random = new Random();
        public int MaxExecutionTime { get; set; }
        public RandomProcessGenerator(int maxExecutionTime)
        {
            MaxExecutionTime = maxExecutionTime;
        }

        public Process GenerateProcess() => new Process(_random.Next(1, MaxExecutionTime));

        public List<Process> GenerateProcesses(int count)
        {
            var list = new List<Process>();
            for (var i = 0; i < count; i++) list.Add(GenerateProcess());
            return list;
        }

    }

    public class Process : IEquatable<Process>
    {
        public Guid Id { get; set; }
        public int ExecutionTime { get; set; }
        public int WaitingTime { get; set; }

        public Process(int executionTime)
        {
            this.Id = Guid.NewGuid();
            this.ExecutionTime = executionTime;
        }

        public override string ToString() =>
            $"ProcessId: {Id},    Time to Execute: {ExecutionTime, 4},    Waiting for: {WaitingTime, 4}";

        public bool Equals(Process other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && ExecutionTime == other.ExecutionTime && WaitingTime == other.WaitingTime;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Process) obj);
        }

        public override int GetHashCode()
            => HashCode.Combine(Id, ExecutionTime, WaitingTime);
    }

    public class ProcessScheduler
    {
        public SortedSet<Process> Processes { get; }


        public ProcessScheduler()
        {
            Processes = new SortedSet<Process>(new ProcessComparer());
        }

        public void Add(Process process)
        {
            Processes.Add(process);
            CalculateWaitingTime();
        }

        public void Add(List<Process> processes)
        {
            processes.ForEach(p => Processes.Add(p));
            CalculateWaitingTime();
        }

        private void CalculateWaitingTime()
        {
            foreach (var process in Processes)
            {
                process.WaitingTime = Processes.TakeWhile(p => !p.Equals(process)).Sum(p => p.ExecutionTime);
            }
        }

        public void PrintSchedulerInfo()
        {
            foreach (var process in Processes)
            {
                Console.WriteLine(process);
            }

            Console.WriteLine("------------------------------------------------------------------------------------------------");
            Console.WriteLine($"Average waiting time: {Processes.Average(p => p.WaitingTime)}ms");
        }

        public void Execute()
        {
            var totalExecutionTime = 0;
            while (Processes.Count != 0)
            {
                var process = Processes.First();
                Console.WriteLine("---------------------------------------------------------------------------------------------");
                Console.WriteLine($"Start executing process    {process.Id} after {process.WaitingTime,6}ms waiting");
                Task.Delay(process.ExecutionTime).Wait();
                Processes.Remove(process);
                totalExecutionTime += process.ExecutionTime;
                Console.WriteLine($"Finished executing process {process.Id} after {process.ExecutionTime,6}ms executing");
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine($"Total execution time: {totalExecutionTime}ms");
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var generator = new RandomProcessGenerator(1000);
            var scheduler = new ProcessScheduler();
            scheduler.Add(generator.GenerateProcesses(10));
            scheduler.PrintSchedulerInfo();
            scheduler.Execute();
        }
    }
}
