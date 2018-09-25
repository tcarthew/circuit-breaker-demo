using System;
using circuit_breaker_app.interfaces;

namespace circuit_breaker_app.implementation
{
    internal class CircuitBreakerStateStore : ICircuitBreakerStateStore
    {
        private CircuitBreakerState state;
        public CircuitBreakerState State => this.state;

        private Exception lastException;
        public Exception LastException => this.lastException;

        private DateTime? lastChangedDateTime;
        public DateTime? LastChangedDateTime => this.lastChangedDateTime;

        public bool IsClosed => this.state == CircuitBreakerState.Closed;

        public void HalfOpen()
        {
            this.state = CircuitBreakerState.HalfOpen;
        }

        public void Reset()
        {
            this.state = CircuitBreakerState.Closed;
            this.lastException = null;
            this.lastChangedDateTime = DateTime.UtcNow;
        }

        public void Trip(Exception exception)
        {
            this.state = CircuitBreakerState.Open;
            this.lastException = exception;
            this.lastChangedDateTime = DateTime.UtcNow;
        }

        internal CircuitBreakerStateStore()
        {
            this.state = CircuitBreakerState.Closed;
            this.lastException = null;
            this.lastChangedDateTime = null;
        }
    }

    public static class CircuitBreakerStateStoreFactory
    {
        public static ICircuitBreakerStateStore Create() 
        {
            return new CircuitBreakerStateStore();
        }
    }
}
