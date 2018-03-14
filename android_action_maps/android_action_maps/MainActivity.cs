﻿using Android.App;
using Android.Widget;
using Android.OS;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Net;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace android_action_maps
{
    [Activity(Label = "Action Maps", MainLauncher = true, Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity
    {
        public SceneView MySceneView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            MySceneView = FindViewById<SceneView>(Resource.Id.MySceneView);

            /* set the scene up */
            agSetupScene();

            /* add building */
            //agAddBuilding();

            /* Demo Dots */
            //agAddDemoPoints();

            /* Start twitter stream */
            //twitter_hose();

            /* Grab the plane data */
            plane_data();

            /* Grab the bikeshare data */
            bike_data();

            /* Grab the metro data */
            // metro_data();
        }


        public class AirplaneInnerJSON
        {
            public double Long;
            public double Lat;
            public double Alt;
        }

        public class AirplaneJSON
        {            
            public AirplaneInnerJSON[] acList;

        }

        GraphicsOverlay planes;
        public void plane_data()
        {

            WebClient client = new WebClient();
            var wgs84 = MySceneView.Scene.SpatialReference;

            planes = new GraphicsOverlay();
            planes.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;


            Task.Factory.StartNew(async () => {
                while (true)
                {

                    string downloadString = client.DownloadString("http://public-api.adsbexchange.com/VirtualRadar/AircraftList.json?lat=34&lng=-118.28&fDstL=0&fDstU=100");
                    AirplaneJSON res = JsonConvert.DeserializeObject<AirplaneJSON>(downloadString);
                    planes.Graphics.Clear();
                    foreach (AirplaneInnerJSON plane in res.acList)
                    {
                        
                        var ploc = new MapPoint(plane.Long, plane.Lat, plane.Alt, wgs84);
                        var pmark = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle,System.Drawing.Color.Blue, 10f);
                        var ptest = new Graphic(ploc, pmark);
                        planes.Graphics.Add(ptest);
                    }

                    await Task.Delay(1250);

                }
            });

            MySceneView.GraphicsOverlays.Add(planes);


        }

        public class BikePropJSON
        {
            public int bikesAvailable;
            public int totalDocks;
            public double longitude;
            public double latitude;
        }
        public class BikeInnerJSON
        {
            public BikePropJSON properties;
        }

        public class BikeJSON
        {
            public BikeInnerJSON[] features;
        }
        GraphicsOverlay bikes;
        public void bike_data()
        {

            WebClient client = new WebClient();
            var wgs84 = MySceneView.Scene.SpatialReference;

            bikes = new GraphicsOverlay();
            bikes.SceneProperties.SurfacePlacement = SurfacePlacement.Draped;

            Task.Factory.StartNew(async () => {
                while (true)
                {

                    string downloadString = client.DownloadString("https://bikeshare.metro.net/stations/json/");
                    BikeJSON res = JsonConvert.DeserializeObject<BikeJSON>(downloadString);
                    bikes.Graphics.Clear();
                    foreach (BikeInnerJSON bike in res.features)
                    {
                        var ploc = new MapPoint(bike.properties.longitude, bike.properties.latitude, 0, wgs84);
                        float full = (bike.properties.bikesAvailable + 0.1f) / (bike.properties.totalDocks + 0.1f);
                        var pmark = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.FromArgb(255, 0, (byte)(255 * full), 0), 10);
                        var ptest = new Graphic(ploc, pmark);
                        bikes.Graphics.Add(ptest);
                    }

                    await Task.Delay(25000);

                }
            });

            MySceneView.GraphicsOverlays.Add(bikes);
        }
        public void agSetupScene()
        {
            MySceneView.Scene = new Scene(Basemap.CreateLightGrayCanvasVector());
            MySceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new System.Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));
            //MySceneView.StereoRendering = new SideBySideBarrelDistortionStereoRendering();
            MySceneView.IsAttributionTextVisible = false;

            // USC
            var camera = new Camera(34.02209, -118.2853, 1000, 0, 45, 0);
            MySceneView.SetViewpointCamera(camera);
        }
    }

}

