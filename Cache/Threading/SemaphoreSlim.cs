using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheRepository.Threading
{
    public class KeyedSemaphoreSlim : IDisposable
    {
        //lock
        private readonly object _lock = new object();
        //waiting queue
        private readonly Queue<SemaphoreWrapper> _wrapperQueue = new Queue<SemaphoreWrapper>();
        //maping key to the semaphore
        private readonly Dictionary<string, SemaphoreWrapper> _wrapperMap = new Dictionary<string, SemaphoreWrapper>();
        //flag for status to show if disposed
        private bool _isDisposed;
        
        //wait method
        public Task<IDisposable> WaitAsync(string key, CancellationToken cancelToken = default(CancellationToken))
        {
            //if locked, starting waiting
            lock (_lock)
            {
                SemaphoreWrapper wrapper;

                if (_wrapperMap.ContainsKey(key))
                    wrapper = _wrapperMap[key];
                else
                {
                    wrapper = _wrapperMap[key] = _wrapperQueue.Count == 0
                        ? new SemaphoreWrapper(Release)
                        : _wrapperQueue.Dequeue();

                    wrapper.Key = key;
                }

                return wrapper.WaitAsync(cancelToken);
            }
        }

        //dispose method
        public void Dispose()
        {
            //cleared
            if (_isDisposed)
                return;
            //lock and start dispose
            lock (_lock)
                foreach (var value in _wrapperMap.Values)
                    value.InternalDispose();

            _isDisposed = true;
        }

        //release the lock
        private void Release(SemaphoreWrapper wrapper)
        {
            lock (_lock)
            {
                var isEmpty = wrapper.Release();
                if (!isEmpty)
                    return;

                _wrapperMap.Remove(wrapper.Key);
                _wrapperQueue.Enqueue(wrapper);
            }
        }

        //def for semaphore wrapper
        private class SemaphoreWrapper : IDisposable
        {

            private readonly Action<SemaphoreWrapper> _parentRelease;
            private readonly SemaphoreSlim _semaphoreSlim;
            private int _useCount;
            public string Key { get; set; }

            public SemaphoreWrapper(Action<SemaphoreWrapper> parentRelease)
            {
                _parentRelease = parentRelease;
                _semaphoreSlim = new SemaphoreSlim(1, 1);
            }

            public async Task<IDisposable> WaitAsync(CancellationToken cancelToken)
            {
                _useCount++;
                await _semaphoreSlim.WaitAsync(cancelToken).ConfigureAwait(false);

                return this;
            }

            public bool Release()
            {
                _semaphoreSlim.Release();
                _useCount--;

                return _useCount == 0;
            }

            public void Dispose()
            {
                _parentRelease(this);
            }

            public void InternalDispose()
            {
                _semaphoreSlim.Dispose();
            }
        }
    }
}