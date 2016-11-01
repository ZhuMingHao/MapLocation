using CheckPointApplication.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace CheckPointApplication.ViewModel
{
    public class PointOfInterestsManager
    {
        public async Task<ObservableCollection<PointOfInterest>> FetchPOIs(Geopoint center)
        {
            ObservableCollection<PointOfInterest> pois = new ObservableCollection<PointOfInterest>();
            pois.Add(new PointOfInterest()
            {
                DisplayName = await GetAddress(center),
                ImageSourceUri = new Uri("ms-appx:///Assets/MapPin.png", UriKind.RelativeOrAbsolute),
                Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = center.Position.Latitude ,
                    Longitude = center.Position.Longitude
                })
            });

            return pois;
        }

        private async Task<string> GetAddress(Geopoint point)
        {
            BasicGeoposition location = new BasicGeoposition();
            location.Latitude = 47.643;
            location.Longitude = -122.131;
            Geopoint pointToReverseGeocode = new Geopoint(location);
            MapLocationFinderResult result =
             await MapLocationFinder.FindLocationsAtAsync(point);
            var address = result.Locations[0].Address;
            return address.Town + address.Street;
        }
    }
}
