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
            InitializeComponent();

            viewModel = new ViewModel();
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
            Closing += OnWindowClosing;
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
                viewModel.RoadLayer.SetVisible(isChecked.Value);
        }

        private void chkboxCrossings_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.CrossingLayer.SetVisible(isChecked.Value);
        }

        private void chkboxRangers_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.RangerLayer.SetVisible(isChecked.Value);
        }

        private void chkboxInterlopers_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.InterloperLayer.SetVisible(isChecked.Value);
        }

        private void chkboxSensors_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = (sender as CheckBox).IsChecked;
            if (IsLoaded && isChecked.HasValue)
                viewModel.SensorLayer.SetVisible(isChecked.Value);
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
            viewModel.SensorLayer.RemoveAll();
        }

        private void btnSensorAddRandom_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddRandomElement(viewModel.SensorLayer, viewModel.RoadLayer);
        }

        private void btnRangerAddRandom_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddRandomElement(viewModel.RangerLayer, viewModel.RoadLayer);
        }

        private void btnRangerDelAll_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RangerLayer.RemoveAll();
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
        
        private void btnBuildTopology_Click(object sender, RoutedEventArgs e)
        {
           // do stuff
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
            if (e.OriginalSource == expMenu)
            {
                expMenuPanel.IsHitTestVisible = true;
                expMenuPanel.Visibility = Visibility.Visible;

                Thickness newMargin = expMenu.Margin;
                newMargin.Bottom -= expMenuPanel.ActualHeight; // don't know why
                expMenu.Margin = newMargin;
            }
        }

        private void expMenu_Collapsed(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == expMenu)
            {
                expMenuPanel.IsHitTestVisible = false;
                expMenuPanel.Visibility = Visibility.Hidden;
                
                Thickness newMargin = expMenu.Margin;
                newMargin.Bottom += expMenuPanel.ActualHeight;
                expMenu.Margin = newMargin;
            }
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
            viewModel.CalculateRoute = viewModel.RoutingAlgorithms[selectedItem];
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

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            viewModel.Simulation.Stop();
        }
    }
}
