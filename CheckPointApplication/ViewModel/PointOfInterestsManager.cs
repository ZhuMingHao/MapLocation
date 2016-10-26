using CheckPointApplication.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace CheckPointApplication.ViewModel
{
    public class PointOfInterestsManager
    {
        public ObservableCollection<PointOfInterest> FetchPOIs(BasicGeoposition center)
        {
            ObservableCollection<PointOfInterest> pois = new ObservableCollection<PointOfInterest>();
            pois.Add(new PointOfInterest()
            {
                DisplayName = "Place One",
                ImageSourceUri = new Uri("ms-appx:///Assets/MapPin.png", UriKind.RelativeOrAbsolute),
                Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = center.Latitude,
                    Longitude = center.Longitude
                })
            });      
            return pois;
        }
    }
}
