using System;
using System.Threading;

namespace UnityToolkit
{
    public class IncreaseId
    {
        private int _counter = 0;

        public int NextConnectionId()
        {
            int id = Interlocked.Increment(ref _counter);

            // it's very unlikely that we reach the uint limit of 2 billion.
            // even with 1 new connection per second, this would take 68 years.
            // -> but if it happens, then we should throw an exception because
            //    the caller probably should stop accepting clients.
            // -> it's hardly worth using 'bool Next(out id)' for that case
            //    because it's just so unlikely.
            if (id == int.MaxValue)
            {
                throw new Exception("connection id limit reached: " + id);
            }

            return id;
        }
    }
}