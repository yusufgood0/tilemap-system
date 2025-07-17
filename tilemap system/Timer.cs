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
        public int getTimeMilliseconds()
        {
            return (int)(DateTime.Now - startTime).TotalMilliseconds;
        }
        public float getTimeSeconds()
        {
            return (float)(DateTime.Now - startTime).TotalSeconds;
        }
        public bool IsActive { get => getTimeMilliseconds() > activationTime; }
        public TimeSpan Elapsed
        {
            get
            {
                return elapsedTime + (DateTime.Now - startTime);
            }
        }
    }
}
