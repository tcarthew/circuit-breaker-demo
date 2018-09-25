using System;
using System.Collections.Generic;
using System.Threading;
using circuit_breaker_app.interfaces;

namespace circuit_breaker_app
{
    class Program
    {
        static readonly CircuitBreaker breaker = new CircuitBreaker();
        static IDictionary<CircuitBreakerState, string> States = new Dictionary<CircuitBreakerState, string>
        {
            { CircuitBreakerState.Closed, "Closed" },
            { CircuitBreakerState.HalfOpen, "HalfOpen" },
            { CircuitBreakerState.Open, "Open" }
        };

        static void Main(string[] args)
        {
            var executing = true;
            var tripped = false;

            Console.WriteLine("Circuit breaker exception demo application...");

            do
            {
                try{
                    if (breaker.State == CircuitBreakerState.HalfOpen)
                    {
                        Console.WriteLine("Breaker half open");    
                    }

                    var delayUsed = SendData();

                    Console.WriteLine("");
                    Console.WriteLine("Send data delay {0}", delayUsed);
                    PrintState();

                    if (tripped)
                    {
                        tripped = false;
                    }
                }
                catch (CircuitBreakerOpenException exception)
                {
                    if (!tripped){
                        Console.WriteLine("Circuit open: {0}", exception.ExceptionThrown.Message);
                        PrintState();
                        tripped = true;
                    }
                    else {
                        Console.Write('.');
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine("General exception: {0}", exception.Message);
                    executing = false;
                }
            } while (executing);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        static void PrintState()
        {
            Console.WriteLine("Circuit breaker closed: {0}", breaker.IsClosed);
            Console.WriteLine("Circuit breaker state: {0}", States[breaker.State]);
            Console.WriteLine("------------------------------");
        }

        static int SendData()
        {
            return breaker.ExecuteAction<int>(() =>
            {
                var random = new Random();
                var delay = random.Next(0, 5);
                                  
                Thread.Sleep(delay * 1000);

                if (delay >= 4)
                {
                    throw new TimeoutException();
                }

                return delay;
            });
        }
    }
}
