using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_system
{
    internal class Timer
    {
        private DateTime startTime;
        private TimeSpan elapsedTime;
        private float activationTime; //in milliseconds
        public Timer(int timerLength)
        {
            activationTime = timerLength;
            Reset();
        }
        public void Reset()
        {
            startTime = DateTime.Now;
            elapsedTime = TimeSpan.Zero;
        }
        public int getTime()
        {
            return (int)(DateTime.Now - startTime).TotalMilliseconds;
        }
        public bool IsActive { get => getTime() > activationTime; }
        public TimeSpan Elapsed
        {
            get
            {
                return elapsedTime + (DateTime.Now - startTime);
            }
        }
    }
}
