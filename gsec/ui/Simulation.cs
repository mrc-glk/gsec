using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using gsec.model;
using Esri.ArcGISRuntime.Geometry;
using System.Diagnostics;
using Esri.ArcGISRuntime.UI;
using gsec.ui.layers;

namespace gsec.ui
{
    public class Simulation
    {
        private static readonly int INTERVAL = 500;
        private static readonly double approxMeterToDegree = 1.09891458303119E-05; // temporary :)
        public static int TimeScale { get; set; } = 1;

        ViewModel viewModel;
        Model model;

        RangerLayer rangerLayer;
        SensorLayer sensorLayer;
        RoadLayer roadLayer;
        CrossingLayer crossingLayer;
        InterloperLayer interloperLayer;

        bool inProgress;
        Thread simLoop = null;

        public Simulation(ViewModel viewModel)
        {
            this.viewModel = viewModel;
            model = viewModel.Model;

            rangerLayer = viewModel.RangerLayer;
            sensorLayer = viewModel.SensorLayer;
            roadLayer = viewModel.RoadLayer;
            crossingLayer = viewModel.CrossingLayer;
            interloperLayer = viewModel.InterloperLayer;
        }

        public void Start()
        {
            if (inProgress)
                return;

            inProgress = true;
            simLoop = new Thread(MainLoop);
            simLoop.Start();
        }

        public void Stop()
        {
            if (inProgress)
            {
                inProgress = false; // don't care about locking
                Thread.Sleep(1000); // loooove it :)
                simLoop.Abort();
            }
        }
        
        private void MainLoop()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double meters = 0;

            while (inProgress)
            {
                stopwatch.Stop();
                long millisElapsed = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
                
                // convert km/h to m/s
                double rangerSpeed = viewModel.RangerSpeed / 3.6;
                double interloperSpeed = viewModel.InterloperSpeed / 3.6;
                double seconds = TimeScale * millisElapsed / 1000.0;
                                
                Thread.Sleep(INTERVAL);
            }

            Console.WriteLine("thread finished");
        }

        // pause, continue?
    }
}
