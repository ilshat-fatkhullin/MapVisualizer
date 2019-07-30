using System.Threading;

public abstract class MultithreadedEntity
{
    public bool IsCompleted
    {
        get
        {
            return thread == null || !thread.IsAlive;
        }
    }

    protected Thread thread;

    public void Execute()
    {
        if (!IsCompleted)
        {            
            throw new System.Exception("Thread is still executing.");
        }

        thread = new Thread(ExecuteThread);
        thread.Start();
    }

    public void Stop()
    {
        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
        }
    }

    protected abstract void ExecuteThread();
}
