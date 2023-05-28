using System;
using System.Diagnostics;
using System.Threading;

namespace ExileCore;

public class ThreadUnit
{
	private readonly AutoResetEvent _event;

	private readonly Stopwatch sw;

	private readonly Thread thread;

	private bool _wait = true;

	private bool running = true;

	public static int CountJobs { get; set; }

	public static int CountWait { get; set; }

	public int Number { get; }

	public Job Job { get; private set; }

	public Job SecondJob { get; private set; }

	public bool Free
	{
		get
		{
			if (!Job.IsCompleted)
			{
				return SecondJob.IsCompleted;
			}
			return true;
		}
	}

	public long WorkingTime => sw.ElapsedMilliseconds;

	public ThreadUnit(string name, int number)
	{
		Number = number;
		Job = new Job("InitJob", null)
		{
			IsCompleted = true
		};
		SecondJob = new Job("InitJob", null)
		{
			IsCompleted = true
		};
		_event = new AutoResetEvent(initialState: false);
		thread = new Thread(DoWork);
		thread.Name = name;
		thread.IsBackground = true;
		thread.Start();
		sw = Stopwatch.StartNew();
	}

	private void DoWork()
	{
		while (running)
		{
			if (Job.IsCompleted && SecondJob.IsCompleted)
			{
				_event.WaitOne();
				CountWait++;
				_wait = true;
			}
			if (!Job.IsCompleted)
			{
				try
				{
					sw.Restart();
					Job.Work?.Invoke();
				}
				catch (Exception ex)
				{
					DebugWindow.LogError(ex.ToString());
					Job.IsFailed = true;
				}
				finally
				{
					Job.ElapsedMs = sw.Elapsed.TotalMilliseconds;
					Job.IsCompleted = true;
					sw.Restart();
				}
			}
			if (SecondJob.IsCompleted)
			{
				continue;
			}
			try
			{
				sw.Restart();
				SecondJob.Work?.Invoke();
			}
			catch (Exception ex2)
			{
				DebugWindow.LogError(ex2.ToString());
				SecondJob.IsFailed = true;
			}
			finally
			{
				SecondJob.ElapsedMs = sw.Elapsed.TotalMilliseconds;
				SecondJob.IsCompleted = true;
				sw.Restart();
			}
		}
	}

	public bool AddJob(Job job)
	{
		job.WorkingOnThread = this;
		bool flag = false;
		if (Job.IsCompleted)
		{
			Job = job;
			flag = true;
			CountJobs++;
		}
		else if (SecondJob.IsCompleted)
		{
			SecondJob = job;
			flag = true;
			CountJobs++;
		}
		if (_wait && flag)
		{
			_wait = false;
			_event.Set();
		}
		return flag;
	}

	public void Abort()
	{
		Job.IsCompleted = true;
		SecondJob.IsCompleted = true;
		Job.IsFailed = true;
		Job.IsFailed = true;
		if (_wait)
		{
			_event.Set();
		}
		running = false;
	}

	[Obsolete("Forced thread abort is not supported")]
	public void ForceAbort()
	{
		DebugWindow.LogError("Forced thread abort is not supported");
	}
}
