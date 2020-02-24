using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Aragas.TupleEventSystem
{
    public sealed class WeakReferenceEventHandler<TEventArgs> : BaseEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly struct DelegateWithWeakReference
        {
            public (Type Type, WeakReference<object> Value)? ObjectStorage { get; }

            public EventHandler<TEventArgs> Delegate { get; }

            public DelegateWithWeakReference(object @object, EventHandler<TEventArgs> @delegate)
            {
                ObjectStorage = (@object.GetType(), new WeakReference<object>(@object));
                Delegate = @delegate;
            }
            internal DelegateWithWeakReference(EventHandler<TEventArgs> @delegate)
            {
                ObjectStorage = null;
                Delegate = @delegate;
            }


            public static bool operator ==(DelegateWithWeakReference left, DelegateWithWeakReference right) => left.Delegate == right.Delegate;
            public static bool operator !=(DelegateWithWeakReference left, DelegateWithWeakReference right) => !(left == right);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj) => obj is DelegateWithWeakReference origin && Equals(origin);
            public bool Equals(DelegateWithWeakReference other) => other.Delegate.Equals(Delegate);

            public override int GetHashCode() => HashCode.Combine(ObjectStorage?.Value.GetHashCode(), Delegate.GetHashCode());
        }
        private List<DelegateWithWeakReference> Subscribers { get; } = new List<DelegateWithWeakReference>();
        private ManualResetEvent SubscribersLock { get; } = new ManualResetEvent(true);

        private bool IsDisposed { get; set; }

        public override BaseEventHandler<TEventArgs> Subscribe(object @object, EventHandler<TEventArgs> @delegate)
        {
            SubscribersLock.WaitOne();
            Subscribers.Add(new DelegateWithWeakReference(@object, @delegate));
            return this;
        }
        /*
        public override BaseEventHandler<TEventArgs> Subscribe((object Object, EventHandler<TEventArgs> Delegate) tuple)
        {
            SubscribersLock.WaitOne();
            Subscribers.Add(new DelegateWithWeakReference(tuple.Object, tuple.Delegate));
            return this;
        }
        */
        public override BaseEventHandler<TEventArgs> Subscribe(EventHandler<TEventArgs> @delegate)
        {
            SubscribersLock.WaitOne();
            Subscribers.Add(new DelegateWithWeakReference(@delegate));
            return this;
        }
        public override BaseEventHandler<TEventArgs> Unsubscribe(EventHandler<TEventArgs> @delegate)
        {
            SubscribersLock.WaitOne();
            Subscribers.Remove(new DelegateWithWeakReference(@delegate));
            return this;
        }

        public override void Invoke(object sender, TEventArgs eventArgs)
        {
            SubscribersLock.WaitOne();
            SubscribersLock.Reset();
            foreach (var subscriber in Subscribers)
                subscriber.Delegate?.Invoke(sender, eventArgs);
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
                        {
                            if (subscriber.ObjectStorage == null)
                                Debug.WriteLine("A leaking event was subscribed to without passing the object with the delegate!");
                            else
                            {
                                if(subscriber.ObjectStorage.Value.Value.TryGetTarget(out var @object))
                                    Debug.WriteLine($"Object {@object} forgot to unsubscribe");
                                else
                                    Debug.WriteLine($"Object of type {subscriber.ObjectStorage.Value.Type} was disposed but forgot to unsubscribe!");
                            }
                        }
#if DEBUG
                        Debugger.Break();
#endif
                        Subscribers.Clear();
                    }
                    SubscribersLock.Set();
                    SubscribersLock.Dispose();
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}