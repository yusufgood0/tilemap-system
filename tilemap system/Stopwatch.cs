using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_system
{
    internal struct Stopwatch
    {
        private DateTime startTime;
        private TimeSpan elapsedTime;
        private readonly float activationTime; //in milliseconds
        public Stopwatch()
        {
            Reset();
        }
        public Stopwatch(int timerLength)
        {
            activationTime = timerLength;
            Reset();
        }
        public void Reset()
        {
            startTime = DateTime.Now;
            elapsedTime = TimeSpan.Zero;
        }
        public readonly int GetTimeMilliseconds() => (int)(DateTime.Now - startTime).TotalMilliseconds;
        public readonly float GetTimeSeconds() => (float)(DateTime.Now - startTime).TotalSeconds;
        public readonly bool IsActive => GetTimeMilliseconds() > activationTime;
        public TimeSpan Elapsed
        {
            get
            {
                return elapsedTime + (DateTime.Now - startTime);
            }
        }
    }
}
