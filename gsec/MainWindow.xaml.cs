using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Windows;
using System.Collections.Generic;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Symbology;
using System.Linq;
using System.Threading.Tasks;
using System;
using gsec.model;
using gsec.ui;
using gsec.ui.layers;
using System.Windows.Controls;
using System.Timers;
using System.ComponentModel;
using gsec.ui.events;
using System.Windows.Input;

namespace gsec
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel viewModel;
        MajorTimer mouseTimer;

        public MainWindow()
        {
            Loaded += MainWindow_Loaded;

            InitializeComponent();

            viewModel = ViewModel.Instance;
            mouseTimer = new MajorTimer(20);

            Initialize();

            DataContext = viewModel;
        }

        private void SetBasemap(BasemapChoice basemap)
        {
            GsecMapView.Map = viewModel.GetBasemap(basemap);
            GsecMapView.SetViewpointGeometryAsync(viewModel.GetExtent());
        }

        private void Initialize()
        {
            GsecMapView.GraphicsOverlays.Add(viewModel.RoadLayer.GetOverlay());
            GsecMapView.GraphicsOverlays.Add(viewModel.CrossingLayer.GetOverlay());
            GsecMapView.GraphicsOverlays.Add(viewModel.SensorLayer.GetOverlay());
            GsecMapView.GraphicsOverlays.Add(viewModel.RangerLayer.GetOverlay());
            GsecMapView.GraphicsOverlays.Add(viewModel.InterloperLayer.GetOverlay());
            
            GsecMapView.GraphicsOverlays.Add(viewModel.EditOverlay);
            GsecMapView.GraphicsOverlays.Add(viewModel.EditSelectionOverlay);
            GsecMapView.GraphicsOverlays.Add(viewModel.RouteOverlay);

            SetBasemap(BasemapChoice.OpenStreetMap);

            GsecMapView.GeoViewTapped += GsecMapView_GeoViewTapped;
            GsecMapView.ViewpointChanged += GsecMapView_ViewpointChanged;

            GsecMapView.MouseMove += GsecMapView_MouseMove;

            viewModel.RangerSelected += gsecRanger_PoppedUp;
            viewModel.SensorSelected += gsecSensor_PoppedUp;
            Closing += OnWindowClosing;

            viewModel.ShowMessage = this.showMessage;

            AppDomain.CurrentDomain.UnhandledException += LifeSaver;
        }

        private void LifeSaver(object sender, UnhandledExceptionEventArgs e)
        {
            string exmsg = string.Format("Uncaught exception: {0}", e.ToString());
            showMessage(exmsg);
        }

        private string lastMsg = null;
        private void showMessage(string message)
        {
            if (message == null || message.Equals(lastMsg))
            {
                return;
            }

            lastMsg = message;

            string date = DateTime.Now.ToString("H:mm:ss");

            this.Dispatcher.Invoke(() =>
            {
                msgbox.Text = msgbox.Text + date + " " + message + Environment.NewLine;

                var old = FocusManager.GetFocusedElement(this);

                msgbox.Focus();
                msgbox.CaretIndex = msgbox.Text.Length;
                msgbox.ScrollToEnd();

                FocusManager.SetFocusedElement(this, old);
            });
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            expMenu_Expanded(null, null);
            expMenu_Collapsed(null, null);
        }

        private void GsecMapView_ViewpointChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("viewpoint changed");
            // worth to start timer and filter out all geometries for given extent?
        }

        private void GsecMapView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseTimer.Locked || viewModel.GetEditMode().HasValue == false)
                return;
            
            mouseTimer.Start();
            
            MapPoint pt = GsecMapView.ScreenToLocation(e.GetPosition(GsecMapView)).ToWgs84();
            viewModel.HandleEditModeMouseMove(pt);
        }

        private async void GsecMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (viewModel.GetEditMode().HasValue)
            {
                viewModel.HandleEditModeGeoTap();
            }
            else
            {
                double tolerance = 5;
                IReadOnlyList<IdentifyGraphicsOverlayResult> identifyResults = await GsecMapView.IdentifyGraphicsOverlaysAsync(
                    e.Position, tolerance, false);

                viewModel.HandleIdentifyResults(identifyResults);
            }
        }

        #region Checkboxes
        private void chkboxRoads_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.RoadLayer.SetVisibility(isChecked.Value);
        }

        private void chkboxCrossings_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.CrossingLayer.SetVisibility(isChecked.Value);
        }

        private void chkboxRangers_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.RangerLayer.SetVisibility(isChecked.Value);
        }

        private void chkboxInterlopers_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.InterloperLayer.SetVisibility(isChecked.Value);
        }

        private void chkboxSensors_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.SensorLayer.SetVisibility(isChecked.Value);
        }
        
        private void chkboxSensorRange_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.SensorLayer.SetRangeVisibility(isChecked.Value);
        }

        private void chkboxInterloperVisibility_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.InterloperLayer.ShowOnlyWithinSensorRange = isChecked.Value;
        }

        #endregion Checkboxes
        #region AdminButtons
        private void btnSensorAdd_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(EditModeType.SensorAdd);
        }

        private void btnSensorDel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(EditModeType.SensorDel);
        }

        private void btnRangerAdd_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(EditModeType.RangerAdd);
        }

        private void btnRangerDel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(EditModeType.RangerDel);
        }
        
        private void btnSensorDelAll_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveAllSensors();
        }

        private void btnSensorAddRandom_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddSensorRandom();
        }

        private void btnRangerAddRandom_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddRangerRandom();
        }

        private void btnRangerDelAll_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveAllRangers();
        }

        private void btnInterloperAdd_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(EditModeType.InterloperAdd);
        }

        private void btnInterloperAddRandom_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddInterloperRandom();
        }

        private void btnInterloperDel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(EditModeType.InterloperDel);
        }

        private void btnInterloperDelAll_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveAllInterlopers();
        }

        private void btnExtent_Click(object sender, RoutedEventArgs e)
        {
            GsecMapView.SetViewpointGeometryAsync(viewModel.GetExtent());
        }

        private void btnEditCancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetEditMode(null);
        }

        private void btnSelectionClear_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearAllSelections();
        }
        
        private void btnSimulationStart_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StartSimulation();
        }

        private void btnSimulationStop_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StopSimulation();
        }

        Random rand = new Random();
        private void btnTrespassersFlow_Click(object sender, RoutedEventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Elapsed += TrespassersTimer_Elapsed;
            timer.Start();
        }

        private void TrespassersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int v = rand.Next();
            if (v % 50 == 0)
            {
                viewModel.AddInterloperRandom();
            }
        }

        private void btnBuildTopology_Click(object sender, RoutedEventArgs e)
        {
            throw new GsecException("not implemented");
        }
        
        private void btnRoutingTest_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //viewModel.TestRouting();
            //viewModel.AddInterloperRandom();
            //Sensor sensor = viewModel.SensorLayer.ByID(57);
            //viewModel.SensorLayer.RaiseAlarm(sensor);
            viewModel.StopSimulation();
            viewModel.AddInterloperRandom();
            viewModel.StartSimulation();
        }

        #endregion AdminButtons
        #region ListBoxes

        private void lstboxPursuits_Selected(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("lstboxPursuits_Selected");
        }

        #endregion Listboxes
        #region Expanders

        private void expMenu_Expanded(object sender, RoutedEventArgs e)
        {
            // I am so bad at this...
            expMenuPanel.IsHitTestVisible = true;
            expMenuPanel.Visibility = Visibility.Visible;

            Thickness newMargin = expMenu.Margin;
            newMargin.Bottom -= expMenuPanel.ActualHeight; // don't know why
            expMenu.Margin = newMargin;
        }

        private void expMenu_Collapsed(object sender, RoutedEventArgs e)
        {
            expMenuPanel.IsHitTestVisible = false;
            expMenuPanel.Visibility = Visibility.Hidden;
                
            Thickness newMargin = expMenu.Margin;
            newMargin.Bottom += expMenuPanel.ActualHeight;
            expMenu.Margin = newMargin;
        }

        #endregion Expanders
        #region Comboboxes
        private void comboBasemap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded == false)
                return;

            var selectedItem = (BasemapChoice) (sender as ComboBox).SelectedItem;
            SetBasemap(selectedItem);
        }
        
        private void comboRoutingAlgo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (string)(sender as ComboBox).SelectedItem;
            viewModel.Routing = viewModel.RoutingAlgorithms[selectedItem];
        }
        #endregion Comboboxes
       
        private void gsecRanger_PoppedUp(object sender, RangerSelectionEventArgs e)
        {
            popupRanger.DataContext = e.Ranger;
            popupRanger.Visibility = Visibility.Visible;
        }

        private void btnPopupRangerDriveTo_Click(object sender, RoutedEventArgs e)
        {
            popupRanger.Visibility = Visibility.Collapsed;
            popupRanger.DataContext = null;

            viewModel.SetEditMode(EditModeType.RangerDriveTo);
        }

        private void btnPopupRangerClose_Click(object sender, RoutedEventArgs e)
        {
            popupRanger.Visibility = Visibility.Collapsed;
            popupRanger.DataContext = null;
        }

        private void gsecSensor_PoppedUp(object sender, SensorSelectionEventArgs e)
        {
            popupSensor.DataContext = e.Sensor;
            popupSensor.Visibility = Visibility.Visible;
        }

        private void btnPopupSensorRaiseAlarm_Click(object sender, RoutedEventArgs e)
        {
            Sensor sensor = popupSensor.DataContext as Sensor;
            viewModel.SensorLayer.RaiseAlarm(sensor);

            popupSensor.Visibility = Visibility.Collapsed;
            popupSensor.DataContext = null;
        }

        private void btnPopupSensorClose_Click(object sender, RoutedEventArgs e)
        {
            popupSensor.Visibility = Visibility.Collapsed;
            popupSensor.DataContext = null;
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            viewModel.Simulation.Stop();
        }
    }
}
