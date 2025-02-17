﻿namespace R3;

public static class ReactivePropertyExtensions
{
    public static ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty<T>(this Observable<T> source, T initialValue = default!)
    {
        return source.ToReadOnlyReactiveProperty(EqualityComparer<T>.Default, initialValue);
    }

    public static ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty<T>(this Observable<T> source, EqualityComparer<T>? equalityComparer, T initialValue = default!)
    {
        if (source is ReadOnlyReactiveProperty<T> rrp)
        {
            return rrp;
        }

        // allow to cast ReactiveProperty<T>
        return new ConnectedReactiveProperty<T>(source, initialValue, equalityComparer);
    }
}

internal sealed class ConnectedReactiveProperty<T> : ReactiveProperty<T>
{
    readonly IDisposable sourceSubscription;

    public ConnectedReactiveProperty(Observable<T> source, T initialValue, EqualityComparer<T>? equalityComparer)
        : base(initialValue, equalityComparer)
    {
        this.sourceSubscription = source.Subscribe(new Observer(this));
    }

    protected override void DisposeCore()
    {
        sourceSubscription.Dispose();
    }

    class Observer(ConnectedReactiveProperty<T> parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            parent.Value = value;
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            parent.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            parent.OnCompleted(result);
        }
    }
}
