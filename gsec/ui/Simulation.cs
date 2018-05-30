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
using System.Windows.Threading;

namespace gsec.ui
{
    public class Simulation
    {
        private static readonly int INTERVAL = 200;
        private static readonly int ANIM_INTERVAL = 50;

        public static int TimeScale { get; set; } = 10;

        DispatcherTimer animationTimer;
        Stopwatch stopwatch;

        ViewModel viewModel;
        Model model;

        RangerLayer rangerLayer;
        SensorLayer sensorLayer;
        RoadLayer roadLayer;
        CrossingLayer crossingLayer;
        InterloperLayer interloperLayer;

        bool inProgress;
        Thread simLoop = null;
        CancellationTokenSource cancelSource;

        public Simulation(ViewModel viewModel)
        {
            this.viewModel = viewModel;
            model = viewModel.Model;

            rangerLayer = viewModel.RangerLayer;
            sensorLayer = viewModel.SensorLayer;
            roadLayer = viewModel.RoadLayer;
            crossingLayer = viewModel.CrossingLayer;
            interloperLayer = viewModel.InterloperLayer;

            stopwatch = new Stopwatch();
        }

        public void Start()
        {
            if (inProgress)
                return;

            inProgress = true;
            cancelSource = new CancellationTokenSource();

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(ANIM_INTERVAL);
            //animationTimer.Tick += AnimationTimer_Tick;

            stopwatch.Start();

            simLoop = new Thread(() => MainLoop(cancelSource.Token));
            simLoop.Start();
        }

        public void Stop()
        {
            if (inProgress)
            {
                inProgress = false;
                cancelSource.Cancel();
                animationTimer.Stop();
                stopwatch.Stop();

                lock (sensorLayer.DataLock)
                {
                    sensorLayer.CancelAlarms();
                }
            }
        }

        // REWORK ALL THIS LOGIC
        private void handleAlarm(Sensor sensor)
        {
            sensorLayer.RaiseAlarm(sensor);

            Crossing nearestCrossing = crossingLayer.GetNearestTo(sensor.EsriPosition);

            lock (rangerLayer.DataLock)
            {
                List<Ranger> freeRangers = rangerLayer.Elements.Where(x => x.State != MobileUnitState.EN_ROUTE).ToList();
                if (freeRangers.Count > 0)
                {
                    List<Graphic> freeRangersGraphics = freeRangers.Select(x => x.Graphic).ToList();

                    MapPoint pt = GeoUtil.GetNearestCoordinateInGraphicsCollection(nearestCrossing.EsriPosition, freeRangersGraphics);
                    Ranger ranger = rangerLayer.ByPosition(pt);

                    viewModel.LeadMobileToCrossing(ranger, nearestCrossing);
                }
            }
        }

        // REWORK ALL THIS LOGIC
        private void doFrameInterloper(double elapsedSeconds)
        {
            double interloperSpeed = viewModel.InterloperSpeed / 3.6;

            lock (interloperLayer.DataLock)
            {
                foreach (Interloper interloper in interloperLayer.Elements)
                {
                    MapPoint newPos = interloper.UpdateUiPosition(interloperSpeed, elapsedSeconds);

                    Sensor nearestSensor = sensorLayer.GetNearestTo(newPos);
                    if (nearestSensor == null)
                    {
                        continue;
                    }

                    if (GeometryEngine.Within(newPos, nearestSensor.RangeGraphic.Geometry) == true)
                    {
                        handleAlarm(nearestSensor);

                        interloper.Graphic.IsVisible = true;
                    }
                    else
                    {
                        interloper.Graphic.IsVisible = (interloperLayer.ShowOnlyWithinSensorRange == false);
                    }
                }
            }
        }

        // REWORK ALL THIS LOGIC
        private void doFrameRanger(double elapsedSeconds)
        {
            // convert km/h to m/s
            double rangerSpeed = viewModel.RangerSpeed / 3.6;

            lock (rangerLayer.DataLock)
            {
                foreach (Ranger ranger in rangerLayer.Elements)
                {
                    MapPoint newPos = ranger.UpdateUiPosition(rangerSpeed, elapsedSeconds);

                    Geometry buf = GeoUtil.GetBuffer(newPos, Ranger.FineRange);

                    // catch interlopers nearby
                    lock (interloperLayer.DataLock)
                    {
                        List<Interloper> caught = interloperLayer.Elements.Where(x => GeometryEngine.Intersects(buf, x.EsriPosition) == true).ToList();
                        if (caught.Count > 0)
                        {
                            rangerLayer.ShowFineAnimation(ranger);
                            caught.ForEach(x => interloperLayer.RemoveElement(x));
                            ranger.Route = null;
                        }
                    }
                }
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            stopwatch.Stop();
            long millisElapsed = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            double seconds = TimeScale * millisElapsed / 1000.0;

            doFrameInterloper(seconds);
            doFrameRanger(seconds); // that's not right
        }













        private void updateRangersPositions(double elapsedSeconds)
        {
            lock (rangerLayer.DataLock)
            {
                foreach (var ranger in rangerLayer.Elements)
                {
                    ranger.UpdateUiPosition(viewModel.RangerSpeed / 3.6, elapsedSeconds);
                }
            }
        }

        private void updateInterlopersPositions(double elapsedSeconds)
        {
            lock (interloperLayer.DataLock)
            {
                foreach (var interloper in interloperLayer.Elements)
                {
                    interloper.UpdateUiPosition(viewModel.InterloperSpeed / 3.6, elapsedSeconds);
                }
            }
        }

        private void updateInterlopersVisibility()
        {
            foreach (var interloper in interloperLayer.Elements)
            {
                interloper.Graphic.IsVisible = (interloperLayer.ShowOnlyWithinSensorRange != true);
            }

            foreach (var sensor in sensorLayer.Elements)
            {
                List<Interloper> ints = interloperLayer.ListByPosition(sensor.EsriPosition, Sensor.Range);
                ints.ForEach(x => x.Graphic.IsVisible = true);
            }
        }

        private Dictionary<Ranger, List<Interloper>> getInterlopersWithinRangersRange()
        {
            Dictionary<Ranger, List<Interloper>> caught = new Dictionary<Ranger, List<Interloper>>();

            lock (rangerLayer.DataLock)
            {
                foreach (var ranger in rangerLayer.Elements)
                {
                    List<Interloper> ints = interloperLayer.ListByPosition(ranger.EsriPosition, Ranger.FineRange);
                    if (ints.Count > 0)
                    {
                        caught.Add(ranger, ints);
                    }
                }
            }

            return caught;
        }

        private ISet<Sensor> getAlarms()
        {
            ISet<Sensor> alarmed = new HashSet<Sensor>();

            lock (interloperLayer.DataLock)
            {
                foreach (var interloper in interloperLayer.Elements)
                {
                    Sensor sensor = sensorLayer.ByPosition(interloper.EsriPosition, Sensor.Range);
                    if (sensor == null)
                    {
                        interloper.RaisedAlarm = null;
                        continue;
                    }

                    if (interloper.RaisedAlarm != null && interloper.RaisedAlarm.ID == sensor.ID)
                    {
                        continue;
                    }

                    interloper.RaisedAlarm = sensor;
                    alarmed.Add(sensor);
                }
            }
            return alarmed;
        }

        private List<Interloper> getFinishedInterlopers()
        {
            return interloperLayer.Elements.Where(x => x.State == MobileUnitState.FINISHED).ToList();
        }

        private List<Ranger> getFinishedRangers()
        {
            return rangerLayer.Elements.Where(x => x.State == MobileUnitState.FINISHED).ToList();
        }

        private List<Ranger> getFreeRangers()
        {
            return rangerLayer.Elements.Where(x => x.State != MobileUnitState.EN_ROUTE).ToList();
        }

        private void catchInterloper(Interloper interloper)
        {
            throw new NotImplementedException();
        }

        private List<Crossing> getSensorCrossings(Sensor sensor)
        {
            List<Crossing> crossings = new List<Crossing>();

            Road road = roadLayer.ByPosition(sensor.EsriPosition);
            crossings.Add(crossingLayer.ByID(road.Source.ID));
            crossings.Add(crossingLayer.ByID(road.Target.ID));

            return crossings;
        }

        private Crossing getRandomCrossing(Sensor sensor)
        {
            List<Crossing> crossings = getSensorCrossings(sensor);

            Random random = new Random();
            return crossings[random.Next(0, crossings.Count)];
        }

#if true
        private void MainLoop(CancellationToken cancelTok)
        {
            int i = 1;
            int db = INTERVAL / ANIM_INTERVAL;

            while (true)
            {
                if (cancelTok.IsCancellationRequested)
                {
                    inProgress = false;
                    Console.WriteLine("cancel requested");
                    break;
                }

                stopwatch.Stop();
                long millisElapsed = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();

                double seconds = TimeScale * millisElapsed / 1000.0;
                
                updateRangersPositions(seconds);
                updateInterlopersPositions(seconds);

                updateInterlopersVisibility();

                Dictionary<Ranger, List<Interloper>> dictCaught = getInterlopersWithinRangersRange();
                lock (rangerLayer.DataLock)
                {
                    foreach (Ranger ranger in dictCaught.Keys)
                    {
                        rangerLayer.ShowFineAnimation(ranger);
                        ranger.Route = null;
                    }
                }

                List<Interloper> caught = dictCaught.Values.SelectMany(x => x).Distinct().ToList();
                lock (interloperLayer.DataLock)
                {
                    foreach (Interloper interloper in caught)
                    {
                        viewModel.Stats.NrCaught++;
                        interloperLayer.RemoveElement(interloper);
                    }

                    foreach (Interloper finished in getFinishedInterlopers())
                    {
                        viewModel.Stats.NrEscaped++;
                        interloperLayer.RemoveElement(finished);
                    }
                }

                lock (rangerLayer.DataLock)
                {
                    foreach (Ranger finished in getFinishedRangers())
                    {
                        finished.Route = null;
                    }
                }

                ISet<Sensor> alarms = getAlarms();
                foreach (Sensor alarm in alarms)
                {
                    sensorLayer.RaiseAlarm(alarm);
                }
                
                // XXXXXXXXXXXXXXXXXXX POLICY DEPENDENT
                foreach (Sensor alarm in alarms)
                {
                    Crossing destination = getRandomCrossing(alarm);
                    
                    if (viewModel.Policy == PursuitPolicy.NEAREST)
                    {
                        List<Ranger> freeRangers = getFreeRangers();
                        Ranger ranger = rangerLayer.GetNearestTo(destination.EsriPosition);
                        if (ranger != null)
                        {
                            viewModel.LeadMobileToCrossing(ranger, destination);
                        }
                    }
                }

                if (i % db == 0)
                {
                    lock (interloperLayer.DataLock)
                    {
                        interloperLayer.Elements.ForEach(x => x.UpdateDbPosition());
                    }

                    lock (rangerLayer.DataLock)
                    {
                        rangerLayer.Elements.ForEach(x => x.UpdateDbPosition());
                    }
                    i = 0;
                }

                i++;

                Thread.Sleep(ANIM_INTERVAL);
            }
        }
    }
#else
        private void MainLoop(CancellationToken cancelToken)
        {
            //animationTimer.Start();

            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    inProgress = false;
                    Console.WriteLine("cancel requested");
                    break;
                }

                //animationTimer.Stop();

                lock (interloperLayer.DataLock)
                {
                    List<Interloper> doneInterlopers = interloperLayer.Elements.Where(x => x.State == MobileUnitState.FINISHED).ToList();
                    foreach (Interloper interloper in doneInterlopers)
                    {
                        interloperLayer.RemoveElement(interloper);
                    }
                    interloperLayer.Elements.ForEach(x => x.UpdateDbPosition());
                }

                lock (rangerLayer.DataLock)
                {
                    List<Ranger> finishedRangers = rangerLayer.Elements.Where(x => x.State == MobileUnitState.FINISHED).ToList();
                    foreach (Ranger ranger in finishedRangers)
                    {
                        ranger.Route = null;
                    }
                    rangerLayer.Elements.ForEach(x => x.UpdateDbPosition());
                }

                //animationTimer.Start();

                Thread.Sleep(INTERVAL);
            }

            Console.WriteLine("thread finished");
        }

        // pause, continue?
    }
}
#endif
}