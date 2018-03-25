using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace gsec.ui
{
    // I'm tired - https://www.youtube.com/watch?v=fmAWIDI4ZgY
    public class MajorTimer
    {
        Timer timer;
        public bool Locked { get; private set; }

        public MajorTimer(double interval)
        {
            timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = interval;
            Locked = true;
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Locked = false;
            timer.Enabled = false;
        }

        public void Start()
        {
            Locked = true;
            timer.Enabled = true;
        }
    }
}
