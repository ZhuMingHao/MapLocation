using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using System.Diagnostics;
using Windows.UI;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CheckPointApplication.Model;
using CheckPointApplication.ViewModel;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Core;
using Windows.ApplicationModel.Background;
using BackgroundTask.Core;
using Windows.Storage;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CheckPointApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {


        private List<Geopoint> locs = new List<Geopoint>();
        private ObservableCollection<PointOfInterest> list;
        private PointOfInterestsManager viewModel;
        private Geopoint OwnLocation { get; set; }
        private static MapIcon map_focus;
        public MainPage()
        {
            this.InitializeComponent();
            viewModel = new PointOfInterestsManager();
            MapIcon mapIcon1 = new MapIcon();
        }

        private async void GetLcoal_Click(object sender, RoutedEventArgs e)
        {
            MapControl.CustomExperience = null;
            MapControl.Style = MapStyle.Road;
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // Get the current location.
                    Geolocator geolocator = new Geolocator();
                    geolocator.DesiredAccuracy = PositionAccuracy.High;
                   
                   Geoposition pos = await geolocator.GetGeopositionAsync();
                    var accuracyRadiusMeters = pos.Coordinate.Accuracy;
                    MapPolygon mapPolygon = new MapPolygon();

                 //   https://msdn.microsoft.com/zh-cn/library/windows/apps/jj735578(v=vs.105).aspx
                    geolocator.PositionChanged += Geolocator_PositionChanged;
                    geolocator.StatusChanged += Geolocator_StatusChanged;
                    Geopoint myLocation = pos.Coordinate.Point;
                    OwnLocation = myLocation;
                    MapControl.LandmarksVisible = true;
                    await MapControl.TrySetViewAsync(myLocation, 12);

                    //if (map_focus == null)
                    //{
                    //    AddMapIcon(myLocation, MapControl);
                    //}

                    // Set the map location.
                    //MapControl.Center = myLocation;
                    //MapControl.ZoomLevel = 12;
                    //MapControl.LandmarksVisible = true;
                    break;

                case GeolocationAccessStatus.Denied:
                    // Handle the case  if access to location is denied.
                    break;

                case GeolocationAccessStatus.Unspecified:
                    // Handle the case if  an unspecified error occurs.
                    break;

            }
        }
        //位置设置变化事件
        private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            // throw new NotImplementedException();
        }

        //位置信息变化调用事件
        private async void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            // map_focus.Location = args.Position.Coordinate.Point;

            // throw new NotImplementedException();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 int v = MapControl.MapElements.IndexOf(map_focus);
             });
        }
        //获取街景地图
        private async void GetStreetside_Click(object sender, RoutedEventArgs e)
        {
            if (MapControl.IsStreetsideSupported)
            {
                BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 48.858, Longitude = 2.295 };
                Geopoint cityCenter = new Geopoint(cityPosition);
                StreetsidePanorama panoramaNearCity = await StreetsidePanorama.FindNearbyAsync(cityCenter);
                if (panoramaNearCity != null)
                {
                    // Create the Streetside view.
                    StreetsideExperience ssView = new StreetsideExperience(panoramaNearCity);
                    ssView.OverviewMapVisible = true;
                    MapControl.CustomExperience = ssView;
                }
            }
            else
            {
                // If Streetside is not supported
                ContentDialog viewNotSupportedDialog = new ContentDialog()
                {
                    Title = "Streetside is not supported",
                    Content = "\nStreetside views are not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await viewNotSupportedDialog.ShowAsync();
            }

        }

        private async void Map3D_Click(object sender, RoutedEventArgs e)
        {
            MapControl.CustomExperience = null;
            if (MapControl.Is3DSupported)
            {
                // Set the aerial 3D view.
                MapControl.Style = MapStyle.Aerial3DWithRoads;
                // Specify the location.
                BasicGeoposition hwGeoposition = new BasicGeoposition() { Latitude = 34.134, Longitude = -118.3216 };
                Geopoint hwPoint = new Geopoint(hwGeoposition);
                // Create the map scene.
                MapScene hwScene = MapScene.CreateFromLocationAndRadius(hwPoint,
                                                                                 60 /* degrees pitch */);
                // Set the 3D view with animation.
                await MapControl.TrySetSceneAsync(hwScene, MapAnimationKind.Bow);
            }
            else
            {
                // If 3D views are not supported, display dialog.
                ContentDialog viewNotSupportedDialog = new ContentDialog()
                {
                    Title = "3D is not supported",
                    Content = "\n3D views are not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await viewNotSupportedDialog.ShowAsync();
                Point t = MapControl.RenderTransformOrigin;

            }


        }

        private async void MapControl_MapTapped(MapControl sender, MapInputEventArgs args)
        {

            list = await viewModel.FetchPOIs(args.Location);
            this.DataContext = list;
            //if (locs.Count == 2)
            //{
            //    locs.Clear();
            //}
            //MapIcon mapicon1 = new MapIcon();
            //mapicon1.Location = args.Location;
            //mapicon1.NormalizedAnchorPoint = new Point(0.5, 1.0);
            //mapicon1.Title = "space needle";
            //mapicon1.ZIndex = 0;
            //locs.Add(args.Location);
            //  sender.MapElements.Add(mapicon1);
            await MapControl.TrySetViewAsync(args.Location);
            //if (locs.Count == 2)
            //{
            //    DrawLineWith(locs.First().Position, locs.Last().Position);
            //}

        }
        private void AddMapIcon(Geopoint point, MapControl sender)
        {
            map_focus = new MapIcon();
            map_focus.Location = point;
            map_focus.NormalizedAnchorPoint = new Point(0.5, 1.0);
            map_focus.ZIndex = 1;
            sender.MapElements.Add(map_focus);

        }

        private void MapStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MapStyleSelect.SelectedIndex)
            {
                case 0:
                    AddMapPolygon();
                    break;
                case 1:
                    AddMapLine();
                    break;
                case 2:
                    RegisterTask();
                    break;
                default:

                    break;

            }

        }

        //注册后台任务
        private async void RegisterTask()
        {
            var trigger = new LocationTrigger(LocationTriggerType.Geofence);
            var task = await App.RegisterBackgroundTask(typeof(BackgroundCore), "LocationTask", trigger, null);
            task.Progress += Task_Progress;
            task.Completed += Task_Completed;

        }
        //处理通知
        private async void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            if (sender != null)
            {
                // Update the UI with progress reported by the background task.
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        // If the background task threw an exception, display the exception in
                        // the error text box.
                        args.CheckResult();

                        // Update the UI with the completion status of the background task.
                        // The Run method of the background task sets the LocalSettings. 
                        // var settings = ApplicationData.Current.LocalSettings;

                        // Get the status.

                        // Do your app work here.

                        Debug.Write("------++++++++++++++++++-------");

                    }
                    catch (Exception ex)
                    {
                        // The background task had an error.
                        //  rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
                        Debug.Write("---------------------");
                    }
                });
            }

        }

        private void Task_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            throw new NotImplementedException();
        }

        private async void AddMapPolygon()
        {
            double centerLatitude = MapControl.Center.Position.Latitude;
            double centerLongitude = MapControl.Center.Position.Longitude;
            MapPolygon mapPolygon = new MapPolygon();
            mapPolygon.Path = new Geopath(new List<BasicGeoposition>() {
         new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude-0.001 },
         new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude-0.001 },
         new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude+0.001 },
         new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude+0.001 },

   });

            mapPolygon.ZIndex = 1;
            mapPolygon.FillColor = Colors.Red;
            mapPolygon.StrokeColor = Colors.Blue;
            mapPolygon.StrokeThickness = 3;
            mapPolygon.StrokeDashed = false;
            MapControl.MapElements.Add(mapPolygon);
            await MapControl.TryZoomToAsync(17);

        }
        private async void AddMapLine()
        {
            double centerLatitude = MapControl.Center.Position.Latitude;
            double centerLongitude = MapControl.Center.Position.Longitude;
            MapPolyline mapPolyline = new MapPolyline();
            mapPolyline.Path = new Geopath(new List<BasicGeoposition>() {
         new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude-0.001 },
         new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude+0.001 },
   });

            mapPolyline.StrokeColor = Colors.Black;
            mapPolyline.StrokeThickness = 3;
            mapPolyline.StrokeDashed = true;
            MapControl.MapElements.Add(mapPolyline);
            await MapControl.TryZoomToAsync(17);

        }
        private async Task<Geopoint> GetGeopoint(string address)
        {

            //获取最近的坐标点
            BasicGeoposition queryHint = new BasicGeoposition();
            queryHint.Latitude = 47.643;
            queryHint.Longitude = -122.131;
            Geopoint hintPoint = new Geopoint(queryHint);
            MapLocationFinderResult result =
             await MapLocationFinder.FindLocationsAsync(
                           address,
                           hintPoint,
                           3);
            //if (result.Status == MapLocationFinderStatus.Success)
            var location = result.Locations[0];
            return location.Point;
        }
        //通过坐标获取地名
        private async Task<string> GetAddress(Geopoint point)
        {
            BasicGeoposition location = new BasicGeoposition();
            location.Latitude = 47.643;
            location.Longitude = -122.131;
            Geopoint pointToReverseGeocode = new Geopoint(location);
            MapLocationFinderResult result =
             await MapLocationFinder.FindLocationsAtAsync(point);
            var address = result.Locations[0].Address;
            return address.Town;
        }

        private async void DrawLineWith(BasicGeoposition start, BasicGeoposition end)
        {
            MapRouteFinderResult routeResult =
                     await MapRouteFinder.GetDrivingRouteAsync(
                     new Geopoint(start),
                     new Geopoint(end),
                     MapRouteOptimization.Time,
                     MapRouteRestrictions.None);
            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                // Use the route to initialize a MapRouteView.
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                double length = routeResult.Route.LengthInMeters / 1000;
                LengthLable.Text = length.ToString();
                viewOfRoute.RouteColor = Colors.Blue;
                viewOfRoute.OutlineColor = Colors.Black;

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.
                MapControl.Routes.Add(viewOfRoute);

                // Fit the MapControl to the route.
                await MapControl.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null, MapAnimationKind.None);
                StringBuilder routeInfo = new StringBuilder();

                //Driving route leg 

                foreach (MapRouteLeg leg in routeResult.Route.Legs)
                {
                    foreach (MapRouteManeuver maneuver in leg.Maneuvers)
                    {
                        routeInfo.AppendLine(maneuver.InstructionText);
                    }
                }

                // Load the text box.
               // tbOutputText.Text = routeInfo.ToString();
            }
            else
            {
              //  tbOutputText.Text =
                     // "A problem occurred: " + routeResult.Status.ToString();
            }

        

    }

    private void mapItemButton_Click(object sender, RoutedEventArgs e)
    {

    }
    private void MapDataSource()
    {
        HttpMapTileDataSource dataSource = new HttpMapTileDataSource("http://map.baidu.com/z={4}&x={0}&y={0}");
        MapTileSource titleSource = new MapTileSource(dataSource);
        MapControl.TileSources.Add(titleSource);
    }
    private async void weilan_Click(object sender, RoutedEventArgs e)
    {
        var accessStatus = await Geolocator.RequestAccessAsync();
        switch (accessStatus)
        {
            case GeolocationAccessStatus.Allowed:
                Geofence geofence = CreatGeofence(OwnLocation.Position);
                IList<Geofence> geos = GeofenceMonitor.Current.Geofences;
                geos.Add(geofence);
                //  FillRegisteredGeofenceListBoxWithExistingGeofences();
                // FillEventListBoxWithExistingEvents();

                // Register for state change events.
                //围栏触发事件
                GeofenceMonitor.Current.GeofenceStateChanged += Current_GeofenceStateChanged;

                //位置权限更改
                GeofenceMonitor.Current.StatusChanged += Current_StatusChanged;
                break;

            case GeolocationAccessStatus.Denied:
                // _rootPage.NotifyUser("Access denied.", NotifyType.ErrorMessage);
                break;

            case GeolocationAccessStatus.Unspecified:
                //_rootPage.NotifyUser("Unspecified error.", NotifyType.ErrorMessage);
                break;

        }
    }
    private Geofence CreatGeofence(BasicGeoposition position)
    {
        string fenceId = "fence8";

        // Define the fence location and radius.
        //BasicGeoposition position;
        //position.Latitude = 47.6510;
        //position.Longitude = -122.3473;
        //position.Altitude = 0.0;
        double radius = 100000; // in meters
                                // Set the circular region for geofence.
        Geocircle geocircle = new Geocircle(position, radius);
        // Remove the geofence after the first trigger.
        bool singleUse = true;
        // Set the monitored states.
        MonitoredGeofenceStates monitoredStates =
                        MonitoredGeofenceStates.Entered |
                        MonitoredGeofenceStates.Exited |
                        MonitoredGeofenceStates.Removed;

        // Set how long you need to be in geofence for the enter event to fire.
        TimeSpan dwellTime = TimeSpan.FromMinutes(1);

        // Set how long the geofence should be active.
        TimeSpan duration = TimeSpan.FromDays(1);

        // Set up the start time of the geofence.
        DateTimeOffset startTime = DateTime.Now;

        // Create the geofence.
        Geofence geofence = new Geofence(fenceId, geocircle, monitoredStates, singleUse, dwellTime, startTime, duration);
        return geofence;
    }

    //位置权限更改委托
    private async void Current_StatusChanged(GeofenceMonitor sender, object args)
    {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            // Show the location setting message only if the status is disabled.
            // LocationDisabledMessage.Visibility = Visibility.Collapsed;

            switch (sender.Status)
            {
                case GeofenceMonitorStatus.Ready:
                    // _rootPage.NotifyUser("The monitor is ready and active.", NotifyType.StatusMessage);
                    break;

                case GeofenceMonitorStatus.Initializing:
                    //  _rootPage.NotifyUser("The monitor is in the process of initializing.", NotifyType.StatusMessage);
                    break;

                case GeofenceMonitorStatus.NoData:
                    //  _rootPage.NotifyUser("There is no data on the status of the monitor.", NotifyType.ErrorMessage);
                    break;

                case GeofenceMonitorStatus.Disabled:
                    //  _rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);

                    // Show the message to the user to go to the location settings.
                    //   LocationDisabledMessage.Visibility = Visibility.Visible;
                    break;

                case GeofenceMonitorStatus.NotInitialized:
                    //  _rootPage.NotifyUser("The geofence monitor has not been initialized.", NotifyType.StatusMessage);
                    break;

                case GeofenceMonitorStatus.NotAvailable:
                    //   _rootPage.NotifyUser("The geofence monitor is not available.", NotifyType.ErrorMessage);
                    break;

                default:
                    //  ScenarioOutput_Status.Text = "Unknown";
                    // _rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                    break;
            }
        });

    }
    //围栏被触发委托
    private async void Current_GeofenceStateChanged(GeofenceMonitor sender, object args)
    {
        var reports = sender.ReadReports();

        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            foreach (GeofenceStateChangeReport report in reports)
            {
                GeofenceState state = report.NewState;

                Geofence geofence = report.Geofence;

                if (state == GeofenceState.Removed)
                {
                    // Remove the geofence from the geofences collection.
                    GeofenceMonitor.Current.Geofences.Remove(geofence);
                }
                else if (state == GeofenceState.Entered)
                {
                    // Your app takes action based on the entered event.

                    // NOTE: You might want to write your app to take a particular
                    // action based on whether the app has internet connectivity.

                }
                else if (state == GeofenceState.Exited)
                {
                    // Your app takes action based on the exited event.

                    // NOTE: You might want to write your app to take a particular
                    // action based on whether the app has internet connectivity.
                }
            }
        });
    }

    //离开界面是注销服务
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        GeofenceMonitor.Current.GeofenceStateChanged -= Current_GeofenceStateChanged;
        GeofenceMonitor.Current.StatusChanged -= Current_StatusChanged;

        base.OnNavigatingFrom(e);
    }
}
}
