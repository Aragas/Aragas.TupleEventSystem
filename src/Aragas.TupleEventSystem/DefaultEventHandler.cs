using System;
using System.Diagnostics;

namespace Aragas.TupleEventSystem
{
    /// <summary>
    /// Based on EventHandler, it does not stores the object reference that subscribed.
    /// </summary>
    public sealed class DefaultEventHandler<TEventArgs> : BaseEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        private event EventHandler<TEventArgs>? EventHandler;

        private bool IsDisposed { get; set; }

        public override BaseEventHandler<TEventArgs> Subscribe(object @object, EventHandler<TEventArgs> @delegate) { EventHandler += @delegate; return this; }
        //public override BaseEventHandler<TEventArgs> Subscribe((object Object, EventHandler<TEventArgs> Delegate) tuple) { EventHandler += tuple.Delegate; return this; }
        public override BaseEventHandler<TEventArgs> Subscribe(EventHandler<TEventArgs> @delegate) { EventHandler += @delegate; return this; }
        public override BaseEventHandler<TEventArgs> Unsubscribe(EventHandler<TEventArgs> @delegate) { EventHandler -= @delegate; return this; }

        public override void Invoke(object sender, TEventArgs eventArgs) { EventHandler?.Invoke(sender, eventArgs); }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (EventHandler?.GetInvocationList().Length > 0)
                    {
                        Debug.WriteLine("Leaking events!");
#if DEBUG
                        Debugger.Break();
#endif
                    }
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}