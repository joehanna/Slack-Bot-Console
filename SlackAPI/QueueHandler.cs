using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Pook.SlackAPI
{
	public class QueueHandler<T>
	{
		public static QueueHandler<T> Start(Action<T> handler, CancellationToken? token = null, ParallelOptions options = null)
		{
			var q = new QueueHandler<T>(handler, token, options);
			q.Start();
			return q;
		}

		public QueueHandler(Action<T> handler, CancellationToken? token = null, ParallelOptions options = null)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));
			this.handler = handler;
			this.token = token ?? CancellationToken.None;
			this.options = options ?? new ParallelOptions();
		}

		private readonly Action<T> handler;
		private readonly CancellationToken token;
		private readonly ParallelOptions options;
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
				}, token);
		}
		private void Loop()
		{
			var partitioner = Partitioner.Create(queue.GetConsumingEnumerable(token), EnumerablePartitionerOptions.NoBuffering);
			Parallel.ForEach(partitioner, options, handler);
		}

		public void Add(T item)
		{
			queue.Add(item, token);
		}
	}
}