using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace parallel1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello {nameof(parallel1)}!");
            Console.WriteLine($"single={DoWorkAsync(3).Result}");

            IEnumerable<int> values = Enumerable.Range(1, 12);
            Stopwatch stopwatch = Stopwatch.StartNew();
            // IEnumerable<(int, int, int)> results = DoTasks(values);
            IEnumerable<(int, int, int)> results = DoParallelAsync(values);
            // IEnumerable<(int, int, int)> results = DoParallel(values);
            stopwatch.Stop();
            Console.WriteLine($"count={results.Count()}, results={string.Join(";", results.Select(r => r.ToString()))}, elapsed={stopwatch.Elapsed}");
        }

        static (int, int, int) DoWork(int i) {
            Random r = new Random(DateTime.Now.Millisecond);
            TimeSpan delay = TimeSpan.FromSeconds(r.Next(2, 10));
            Console.WriteLine($"i={i}, delay={delay}");
            Task.Delay(delay).Wait();
            int n = r.Next(1, 11);

            if(n > 10) {
                throw new Exception($"n={n}");
            }

            return (i, n, i * n);
        }

        static async Task<(int, int, int)> DoWorkAsync(int i)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            TimeSpan delay = TimeSpan.FromSeconds(r.Next(2, 10));
            Console.WriteLine($"i={i}, delay={delay}");
            await Task.Delay(delay);
            int n = r.Next(1, 11);

            if(n > 10) {
                throw new Exception($"n={n}");
            }

            return (i, n, i * n);
        }

        static IEnumerable<(int, int, int)> DoTasks(IEnumerable<int> values) {
            ICollection<(int, int, int)> results = new List<(int, int, int)>();
            Task[] tasks = values.Select(v => Task.Run(async () => {
                var result = await DoWorkAsync(v);
                results.Add(result);
            })).ToArray();
            try {
                if(!Task.WaitAll(tasks, TimeSpan.FromSeconds(10))) {
                    Console.WriteLine($"TIMEOUT!");
                }
            }
            catch(Exception x) {
                throw;
            }
            return results;
        }

        static IEnumerable<(int, int, int)> DoParallelAsync(IEnumerable<int> values) {
            ICollection<(int, int, int)> results = new List<(int, int, int)>();
            Parallel.ForEach(values, (value) => {
                Task<(int, int, int)> task = DoWorkAsync(value);
                if(task.Wait(TimeSpan.FromSeconds(5))) {
                    var result = task.Result;
                    results.Add(result);
                }
                else {
                    Console.WriteLine($"timeout, value={value}");
                }
            });
            return results;
        }

        static IEnumerable<(int, int, int)> DoParallel(IEnumerable<int> values) {
            ICollection<(int, int, int)> results = new List<(int, int, int)>();
            Parallel.ForEach(values, (value) => {
                var result = DoWork(value);
                results.Add(result);
            });
            return results;
        }

        /*
        {
            Console.WriteLine($"Hello {nameof(parallel2)}!");

            IEnumerable<Task<(int, int)>> inputs = Enumerable.Range(1, 12)
                .Select(i => Do(i));
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<(int, int)> outputs = await Task.WhenAll(inputs);
            stopwatch.Stop();
            TimeSpan sum = TimeSpan.FromMilliseconds(outputs.Sum(o => o.Item2));
            Console.WriteLine($"{string.Join(';', outputs)}, sum={sum}, elapsed={stopwatch.Elapsed}");
        }

        static async Task<(int, int)> Do(int i) {
            Random random = new Random();
            int delay = random.Next(100, 3000);
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            return (i, delay);
        }
        */
    }
}
