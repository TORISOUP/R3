namespace R3;

public static partial class Observable
{
    public static Observable<T> Create<T>(Func<Observer<T>, IDisposable> subscribe)
    {
        return new AnonymousObservable<T>(subscribe);
    }

    public static Observable<T> Create<T, TState>(TState state, Func<Observer<T>, TState, IDisposable> subscribe)
    {
        return new AnonymousObservable<T, TState>(state, subscribe);
    }

    public static Observable<T> Create<T>(Func<Observer<T>, CancellationToken, ValueTask> subscribe)
    {
        return new AsyncAnonymousObservable<T>(subscribe);
    }

    public static Observable<T> Create<T, TState>(TState state, Func<Observer<T>, TState, CancellationToken, ValueTask> subscribe)
    {
        return new AsyncAnonymousObservable<T, TState>(state, subscribe);
    }
}

internal sealed class AnonymousObservable<T>(Func<Observer<T>, IDisposable> subscribe) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subscribe(observer);
    }
}

internal sealed class AnonymousObservable<T, TState>(TState state, Func<Observer<T>, TState, IDisposable> subscribe)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subscribe(observer, state);
    }
}

internal sealed class AsyncAnonymousObservable<T>(Func<Observer<T>, CancellationToken, ValueTask> subscribe)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var cancellationDisposable = new CancellationDisposable();
        subscribe(observer, cancellationDisposable.Token);
        return cancellationDisposable;
    }
}

internal sealed class AsyncAnonymousObservable<T, TState>(TState state, Func<Observer<T>, TState, CancellationToken, ValueTask> subscribe)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var cancellationDisposable = new CancellationDisposable();
        subscribe(observer, state, cancellationDisposable.Token);
        return cancellationDisposable;
    }
}
