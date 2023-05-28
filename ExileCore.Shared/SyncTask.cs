using System.Runtime.CompilerServices;

namespace ExileCore.Shared;

[AsyncMethodBuilder(typeof(SyncTaskMethodBuilder<>))]
public class SyncTask<T>
{
	internal SyncAwaiter<T> Awaiter { get; } = new SyncAwaiter<T>();


	public SyncAwaiter<T> GetAwaiter()
	{
		return Awaiter;
	}
}
