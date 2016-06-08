using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Pook.SlackAPI
{
    public class QueueHandler<T>
    {
        public static QueueHandler<T> Start(Func<T, Task> handler, CancellationToken? token = null, int maxDegreeOfParallelism = 5)
        {
            var q = new QueueHandler<T>(handler, token, maxDegreeOfParallelism);
            q.Start();
            return q;
        }

        public QueueHandler(Func<T, Task> handler, CancellationToken? token = null, int maxDegreeOfParallelism = 5)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            this.handler = handler;
            this.token = token ?? CancellationToken.None;
            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        private readonly Func<T, Task> handler;
        private readonly CancellationToken token;
        private readonly int maxDegreeOfParallelism;
        private readonly BlockingCollection<T> queue = new BlockingCollection<T>();

        public void Start()
        {
            Task.Factory
                .StartNew(Loop, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        Trace.TraceError(t.Exception.Message);
                    Trace.TraceInformation("HandlerLoop finished");
                }, CancellationToken.None);
        }
        private void Loop()
        {
            var action = new ActionBlock<T>(
                async x => await handler(x),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
            );
            foreach (var item in queue.GetConsumingEnumerable())
                action.Post(item);
            action.Complete();
        }

        public void Add(T item)
        {
            queue.Add(item, token);
        }
    }
}