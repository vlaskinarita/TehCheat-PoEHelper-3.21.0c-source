using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ExileCore.Shared;

public class SyncAwaiter<T> : SyncAwaiter, INotifyCompletion
{
	public bool IsCompleted => ResultTask.Task.IsCompleted;

	internal TaskCompletionSource<T> ResultTask { get; } = new TaskCompletionSource<T>();


	public T GetResult()
	{
		return ResultTask.Task.Result;
	}

	public void OnCompleted(Action completion)
	{
		ResultTask.Task.ContinueWith(delegate
		{
			completion();
		}, TaskContinuationOptions.OnlyOnRanToCompletion);
	}
}
public abstract class SyncAwaiter
{
	private readonly Queue<Action> _methodExecutionQueue = new Queue<Action>();

	private Action<Action> _enqueueItemAction;

	internal void RedirectExecutionQueue(Action<Action> target)
	{
		_enqueueItemAction = target;
		Action result;
		while (_methodExecutionQueue.TryDequeue(out result))
		{
			EnqueueItem(result);
		}
	}

	internal void EnqueueItem(Action item)
	{
		if (_enqueueItemAction == null)
		{
			_methodExecutionQueue.Enqueue(item);
		}
		else
		{
			_enqueueItemAction(item);
		}
	}

	public void PumpEvents()
	{
		try
		{
			Action result;
			while (_methodExecutionQueue.TryDequeue(out result))
			{
				result?.Invoke();
			}
		}
		catch
		{
		}
	}
}
