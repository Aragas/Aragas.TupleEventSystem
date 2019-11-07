using System;
using System.Runtime.CompilerServices;

namespace Aragas.TupleEventSystem
{
    /// <summary>
    /// Event system that provides a reference to the subscriber.
    /// </summary>
    public abstract class BaseEventHandler<TEventArgs> : IDisposable where TEventArgs : EventArgs
    {
        private bool IsDisposed { get; set; }

        // TODO: Find some way to make the class invokable
        //public static explicit operator Action<object, TEventArgs>(BaseEventHandler<TEventArgs> handler) => handler.Invoke;
        public abstract void Invoke(object sender, TEventArgs eventArgs);

        public static BaseEventHandler<TEventArgs> operator +(BaseEventHandler<TEventArgs> eventHandler, (object, EventHandler<TEventArgs>) tuple) => eventHandler.Subscribe(tuple);
        public static BaseEventHandler<TEventArgs> operator +(BaseEventHandler<TEventArgs> eventHandler, EventHandler<TEventArgs> @delegate) => eventHandler.Subscribe(@delegate);
        public static BaseEventHandler<TEventArgs> operator -(BaseEventHandler<TEventArgs> eventHandler, EventHandler<TEventArgs> @delegate) => eventHandler.Unsubscribe(@delegate);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BaseEventHandler<TEventArgs> Subscribe((object Object, EventHandler<TEventArgs> Delegate) tuple) => Subscribe(tuple.Object, tuple.Delegate);
        public abstract BaseEventHandler<TEventArgs> Subscribe(object @object, EventHandler<TEventArgs> @delegate);
        //public abstract BaseEventHandler<TEventArgs> Subscribe((object Object, EventHandler<TEventArgs> Delegate) tuple);
        public abstract BaseEventHandler<TEventArgs> Subscribe(EventHandler<TEventArgs> @delegate);
        public abstract BaseEventHandler<TEventArgs> Unsubscribe(EventHandler<TEventArgs> @delegate);


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {

                }
                IsDisposed = true;
            }
        }
        ~BaseEventHandler()
        {
            Dispose(false);
        }
    }
}