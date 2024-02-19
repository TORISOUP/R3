﻿namespace R3.Tests;

public class SubjectTest
{
    [Fact]
    public void Test()
    {
        // Dispose(not yet completed)
        {
            var s = new Subject<int>();
            using var l = s.ToLiveList();
            s.OnNext(1);
            s.OnNext(2);
            s.OnNext(3);
            s.Dispose();

            l.AssertEqual([1, 2, 3]);
            l.AssertIsCompleted();
            s.IsDisposed.Should().BeTrue();
        }

        // already OnCompleted(Success), Dispose
        {
            var s = new Subject<int>();
            using var l = s.ToLiveList();
            s.OnNext(1);
            s.OnNext(2);
            s.OnNext(3);
            s.OnCompleted();
            s.Dispose();

            l.AssertEqual([1, 2, 3]);
            l.AssertIsCompleted();
            s.IsDisposed.Should().BeTrue();
        }

        // already OnCompleted(Failure), Dispose
        {
            var s = new Subject<int>();
            using var l = s.ToLiveList();
            s.OnNext(1);
            s.OnNext(2);
            s.OnNext(3);
            s.OnCompleted(new Exception("foo"));
            s.Dispose();

            l.AssertEqual([1, 2, 3]);
            l.AssertIsCompleted();
            s.IsDisposed.Should().BeTrue();
        }


        // already Disposed, call OnNext
        {
            var s = new Subject<int>();
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => s.OnNext(1));
        }
        // already Disposed, call OnError
        {
            var s = new Subject<int>();
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => s.OnErrorResume(new Exception()));
        }
        // already Disposed, call OnCompleted
        {
            var s = new Subject<int>();
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => s.OnCompleted());
        }
    }

    [Fact]
    public void SubscribeAfterCompleted()
    {
        {
            // after Success
            var s = new Subject<int>();
            s.OnCompleted();

            using var l = s.ToLiveList();

            l.AssertIsCompleted();
            l.Result.IsSuccess.Should().BeTrue();
        }
        {
            // after Failure
            var s = new Subject<int>();
            s.OnCompleted(new Exception("foo"));

            using var l = s.ToLiveList();

            l.AssertIsCompleted();
            l.Result.IsFailure.Should().BeTrue();
            l.Result.Exception!.Message.Should().Be("foo");
        }
    }

    [Fact]
    public void SubscribeToSubject()
    {
        var firstSubject = new Subject<int>();
        var secondSubject = new Subject<int>();
        using var l = secondSubject.Materialize().ToLiveList();
        var ex = new Exception("error");

        firstSubject.Subscribe(secondSubject);
        firstSubject.OnNext(1);
        firstSubject.OnNext(2);
        firstSubject.OnNext(3);
        firstSubject.OnErrorResume(ex);
        firstSubject.Dispose();

        l.AssertEqual([
            new Notification<int>(1),
            new Notification<int>(2),
            new Notification<int>(3),
            new Notification<int>(ex),
            new Notification<int>(Result.Success)
        ]);
        l.AssertIsCompleted();
    }
}
