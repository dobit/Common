using System;

namespace System
{
    internal class DelegateBasedObserver<T> : IObserver<T>
    {
        private Action _onCompleted;
        private Action<Exception> _onError;
        private Action<T> _onNext;

        internal DelegateBasedObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }
            if (onError == null)
            {
                throw new ArgumentNullException("onError");
            }
            if (onCompleted == null)
            {
                throw new ArgumentNullException("onCompleted");
            }
            this._onNext = onNext;
            this._onError = onError;
            this._onCompleted = onCompleted;
        }

        public void OnCompleted()
        {
            this._onCompleted();
        }

        public void OnError(Exception error)
        {
            this._onError(error);
        }

        public void OnNext(T value)
        {
            this._onNext(value);
        }
    }
}

