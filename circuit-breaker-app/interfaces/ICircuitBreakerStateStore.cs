using System;

namespace circuit_breaker_app.interfaces
{
    public enum CircuitBreakerState
    {
        Closed = 1,
        HalfOpen = 2,
        Open = 3
    }

    public interface ICircuitBreakerStateStore
    {
        CircuitBreakerState State { get; }

        Exception LastException { get; }

        DateTime? LastChangedDateTime { get; }

        void Trip(Exception ex);

        void Reset();

        void HalfOpen();

        bool IsClosed { get; }
    }
}
