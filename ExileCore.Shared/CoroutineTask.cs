namespace ExileCore.Shared;

public class CoroutineTask<T> : ICoroutine
{
	public bool IsCompleted { get; private set; }

	public ICoroutine[] Children { get; }

	public void Continue()
	{
	}
}
