using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
            public DelegateWithReference((object Object, EventHandler<TEventArgs> Delegate) tuple)
            {
                ObjectType = tuple.Object.GetType();
                Object = tuple.Object;
                Delegate = tuple.Delegate;
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

        private bool IsDisposed { get; set; }

        public override BaseEventHandler<TEventArgs> Subscribe(object @object, EventHandler<TEventArgs> @delegate) { lock (Subscribers) { Subscribers.Add(new DelegateWithReference(@object, @delegate)); return this; } }
        public override BaseEventHandler<TEventArgs> Subscribe((object Object, EventHandler<TEventArgs> Delegate) tuple) { lock (Subscribers) { Subscribers.Add(new DelegateWithReference(tuple)); return this; } }
        public override BaseEventHandler<TEventArgs> Subscribe(EventHandler<TEventArgs> @delegate) { lock (Subscribers) { Subscribers.Add(new DelegateWithReference(@delegate)); return this; } }
        public override BaseEventHandler<TEventArgs> Unsubscribe(EventHandler<TEventArgs> @delegate) { lock (Subscribers) { Subscribers.Remove(new DelegateWithReference(@delegate)); return this; } }

        protected override void Invoke(object sender, TEventArgs e)
        {
            lock (Subscribers)
            {
                foreach (var subscriber in Subscribers)
                    subscriber.Delegate?.Invoke(sender, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    lock (Subscribers)
                    {
                        if (Subscribers.Count > 0)
                        {
                            Debug.WriteLine("Leaking events!");
                            foreach (var storage in Subscribers)
                                Debug.WriteLine(storage.Object != null ? $"Object {storage.ObjectType} forgot to unsubscribe" : $"Object of type {storage.ObjectType} was disposed but forgot to unsubscribe!");
#if DEBUG
                            Debugger.Break();
#endif
                        }
                    }

                    Subscribers.Clear();
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}