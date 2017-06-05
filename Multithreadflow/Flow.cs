using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreadflow
{
    public class StepConfig
    {
        public bool ModeContinue { get; set; }
        public int CheckInterval { get; set; }
        public int DurationModeContinue { get; set; }
        public Action<int> Update { get; set; }
        
        public int NumThreadStopped { get; set; }
        public Action<int, List<object>> Action { get; set; }
        public int[] MaxItemsPerThread { get; set; }
        
        public List<object> Queue { get; set; } = new List<object>();
    }

    public class FlowConfig
    {
        public List<StepConfig> Step = new List<StepConfig>();
    }

    public class ThreadParameters
    {
        public int ThreadId { get; set; }
        public StepConfig Step { get; set; }
    }

    public class Flow
    {
        public FlowConfig Config { get; set; }
        private List<Thread> listThread = new List<Thread>();

        private object lockThread = new object();
        public void Start()
        {
            foreach (var stepItem in Config.Step)
            {
                for (int i = 0; i < stepItem.MaxItemsPerThread.Length; i++)
                {
                    Thread t = new Thread(new ParameterizedThreadStart((parameters) =>
                    {
                        var tParam = parameters as ThreadParameters;
                        var step = tParam.Step;
                        int stepIndex = Config.Step.IndexOf(step);
                        int ticks = 0;

                        while (Thread.CurrentThread.IsAlive)
                        {
                            // Vérifier si le thread est tjrs en vie.
                            bool checkIsAlive = true;
                            if (stepIndex > 0)
                            {
                                var previousStep = Config.Step[stepIndex - 1];
                                if (previousStep.NumThreadStopped != previousStep.MaxItemsPerThread.Length)
                                {
                                    checkIsAlive = false;
                                }
                            }
                            if (checkIsAlive)
                            {
                                if (!step.Queue.Any() && !step.ModeContinue)
                                {
                                    step.NumThreadStopped++;
                                    break;
                                }
                            }

                            // Vérifier s'il y a de la job à faire.
                            List<object> queueThread = new List<object>();
                            lock (lockThread)
                            {
                                int maxItems = step.MaxItemsPerThread[tParam.ThreadId];

                                var count = step.Queue.Count;
                                if (count > maxItems
                                    ||
                                    ticks % 40 == 0 && count > 0 /* Attendre plus longtemps s'il n'y a pas suffisamment d'élément */
                                    )
                                {
                                    // Il y a suffisamment d'élément ou on a attendu un peu.
                                    queueThread.AddRange(step.Queue.Take(maxItems));
                                    foreach (var item in queueThread)
                                    {
                                        step.Queue.Remove(item);
                                    }
                                }
                            }

                            if (queueThread.Any())
                            {
                                step.Action(tParam.ThreadId, queueThread);
                            }

                            // Mode continue: update.
                            if (step.ModeContinue && ticks % step.CheckInterval == 0)
                            {
                                step.Update(tParam.ThreadId);
                            }

                            // Mode continue: verifier si l'on a atteint la durée maximal.
                            if (step.ModeContinue && step.DurationModeContinue <= ticks)
                            {
                                step.ModeContinue = false;
                            }

                            ticks++;
                            Thread.Sleep(10);
                        }
                    }));
                    t.Start(new ThreadParameters
                    {
                        Step = stepItem,
                        ThreadId = i
                    });
                    listThread.Add(t);
                }
            }
        }

        public void Join()
        {
            listThread.ForEach(m => m.Join());
        }
    }
}
