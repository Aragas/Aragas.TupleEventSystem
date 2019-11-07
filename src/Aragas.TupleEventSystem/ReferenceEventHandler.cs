using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Aragas.TupleEventSystem
{
    public sealed class ReferenceEventHandler<TEventArgs> : BaseEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly struct DelegateWithReference
        {
            public Type ObjectType { get; }
            public object Object { get; }
            public EventHandler<TEventArgs> Delegate { get; }

            public DelegateWithReference(object @object, EventHandler<TEventArgs> @delegate)
            {
                ObjectType = @object.GetType();
                Object = @object;
                Delegate = @delegate;
            }
            internal DelegateWithReference(EventHandler<TEventArgs> @delegate)
            {
                ObjectType = null;
                Object = null;
                Delegate = @delegate;
            }


            public static bool operator ==(DelegateWithReference left, DelegateWithReference right) => left.Delegate == right.Delegate;
            public static bool operator !=(DelegateWithReference left, DelegateWithReference right) => !(left == right);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj) => obj is DelegateWithReference origin && Equals(origin);
            public bool Equals(DelegateWithReference other) => other.Delegate.Equals(Delegate);

            public override int GetHashCode() => HashCode.Combine(Object.GetHashCode(), Delegate.GetHashCode());
        }
        private List<DelegateWithReference> Subscribers { get; } = new List<DelegateWithReference>();
        private ManualResetEvent SubscribersLock { get; } = new ManualResetEvent(true);

        private bool IsDisposed { get; set; }

        public override BaseEventHandler<TEventArgs> Subscribe(object @object, EventHandler<TEventArgs> @delegate)
        {
            SubscribersLock.WaitOne();
            Subscribers.Add(new DelegateWithReference(@object, @delegate));
            return this;
        }
        /*
        public override BaseEventHandler<TEventArgs> Subscribe((object Object, EventHandler<TEventArgs> Delegate) tuple)
        {
            SubscribersLock.Wait();
            Subscribers.Add(new DelegateWithReference(tuple.Object, tuple.Delegate));
            return this;
        }
        */
        public override BaseEventHandler<TEventArgs> Subscribe(EventHandler<TEventArgs> @delegate)
        {
            SubscribersLock.WaitOne();
            Subscribers.Add(new DelegateWithReference(@delegate));
            return this;
        }
        public override BaseEventHandler<TEventArgs> Unsubscribe(EventHandler<TEventArgs> @delegate)
        {
            SubscribersLock.WaitOne();
            Subscribers.Remove(new DelegateWithReference(@delegate));
            return this;
        }

        protected override void Invoke(object sender, TEventArgs e)
        {
            SubscribersLock.WaitOne();
            SubscribersLock.Reset();
            foreach (var subscriber in Subscribers)
                subscriber.Delegate?.Invoke(sender, e);
            SubscribersLock.Set();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    SubscribersLock.WaitOne();
                    SubscribersLock.Reset();
                    if (Subscribers.Count > 0)
                    {
                        Debug.WriteLine("Leaking events!");
                        foreach (var subscriber in Subscribers)
                            Debug.WriteLine(subscriber.Object != null ? $"Object {subscriber.ObjectType} forgot to unsubscribe" : $"Object of type {subscriber.ObjectType} was disposed but forgot to unsubscribe!");
#if DEBUG
                        Debugger.Break();
#endif
                        Subscribers.Clear();
                    }
                    SubscribersLock.Set();
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}