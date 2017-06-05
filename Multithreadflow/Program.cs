using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreadflow
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int n = 50; n < 100; n++)
            {
                List<object> queue = new List<object>();
                for (int j = 0; j < n; j++)
                {
                    queue.Add(j);
                }

                Flow m = new Flow();
                m.Config = new FlowConfig();
                
                m.Config.Step.Add(new StepConfig
                {
                    Queue = queue,
                    MaxItemsPerThread = new int[] { 1 },
                    Action = (threadId, items) =>
                    {
                        var i = (int)items.First();

                        Thread.Sleep(i % 10 + 50);
                        Console.WriteLine(" Thread : " + threadId + " Step 0 : " + i);

                        m.Config.Step[1].Queue.AddRange(items);
                    },
                    ModeContinue = true,
                    CheckInterval = 200,
                    DurationModeContinue = 600,
                    Update = (threadId) =>
                    {
                        int nouvelItem = 1000;

                        m.Config.Step[1].Queue.Add(nouvelItem);

                        Console.WriteLine(" Thread : " + threadId + " Update");
                    }
                });
                
                m.Config.Step.Add(new StepConfig
                {
                    MaxItemsPerThread = new int[] { 1, 1, 1, 1 },
                    Action = (threadId, items) =>
                    {
                        var i = (int)items.First();

                        Thread.Sleep(i % 10 + 500);
                        Console.WriteLine(" Thread : " + threadId + " Step 1 : " + i);

                        // Supposons qu'on va créer quelques items à partir d'un item.
                        for (int k = 0; k < 5; k++)
                        {
                            m.Config.Step[2].Queue.Add(i+ "_" + k);
                        }
                    }
                });
                
                m.Config.Step.Add(new StepConfig
                {
                    MaxItemsPerThread = new int[] { 64, 16, 16 },
                    Action = (threadId, items) =>
                    {
                        var ids = string.Join(", ", items);

                        Thread.Sleep(items.Count * 10 + 1500);
                        Console.WriteLine(" Thread : " + threadId + " Step 2 : (" + items.Count + ") " + ids);

                        m.Config.Step[3].Queue.AddRange(items);
                    }
                });
                
                m.Config.Step.Add(new StepConfig
                {
                    MaxItemsPerThread = new int[] { 1 },
                    Action = (threadId, items) =>
                    {
                        var i = (string)items.First();

                        Thread.Sleep(20);
                        Console.WriteLine(" Thread : " + threadId + " Step 3 : " + i);
                    }
                });

                Console.WriteLine("Starting...");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                m.Start();
                m.Join();

                stopwatch.Stop();

                Console.WriteLine("n = " + n + ", duration = " + stopwatch.ElapsedMilliseconds + " ms");
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
