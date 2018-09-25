using System;
using System.Diagnostics;
using System.Threading;
using circuit_breaker_app.implementation;
using circuit_breaker_app.interfaces;

namespace circuit_breaker_app
{
    public class CircuitBreakerOpenException: Exception
    {
        private Exception exceptionThrown;
        public Exception ExceptionThrown
        {
            get { return this.exceptionThrown; }
        }

        public CircuitBreakerOpenException(): base()
        {
        }

        public CircuitBreakerOpenException(Exception exception)
            : this()
        {
            this.exceptionThrown = exception;
        }
    }

    public class CircuitBreaker
    {
        private const int HalfOpenWaitTime = 15;
        private readonly ICircuitBreakerStateStore stateStore;
        private readonly object halfOpenSyncObject;
        public bool IsClosed => stateStore.IsClosed;
        public bool IsOpen => !stateStore.IsClosed;
        public CircuitBreakerState State => stateStore.State;

        public CircuitBreaker()
        {
            this.stateStore = CircuitBreakerStateStoreFactory.Create();
            this.halfOpenSyncObject = new object();
        }

        public T ExecuteAction<T>(Func<T> action)
        {
            if (this.IsOpen)
            {
                if (this.stateStore.LastChangedDateTime.Value.AddSeconds(HalfOpenWaitTime) < DateTime.UtcNow)
                {
                    // open timeout has expired, we can allow 1 operation to execute
                    bool locked = false;
                    try
                    {
                        Monitor.TryEnter(this.halfOpenSyncObject, ref locked);

                        if (locked)
                        {
                            this.stateStore.HalfOpen();

                            T result = action();

                            // if we succeeded, then we reset the circuit breaker
                            this.stateStore.Reset();

                            return result;
                        }
                    }
                    catch(Exception exception)
                    {
                        // if we're still getting an exception, trip the breaker again
                        this.stateStore.Trip(exception);
                        throw new CircuitBreakerOpenException(exception);
                    }
                    finally
                    {
                        if (locked)
                        {
                            Monitor.Exit(this.halfOpenSyncObject);
                        }
                    }
                }

                // open timeout hasn't expired so throuhg the circuit breaker exception
                throw new CircuitBreakerOpenException(this.stateStore.LastException);
            }

            // breaker is closed so execute the action
            try
            {
                Debugger.Log(1, "DEBUG", "Executing action\n");
                return action();
            }
            catch (TimeoutException exception)
            {
                Debugger.Log(1, "DEBUG", "Timeout\n");
                this.TrackException(exception);
                throw new CircuitBreakerOpenException(this.stateStore.LastException);
            }
        }

        private void TrackException(Exception exception)
        {
            // should probably only all the breaker to be tripped for
            // certain exceptions
            this.stateStore.Trip(exception);
        }
    }
}
