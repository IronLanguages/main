/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Net;
using MapPointWebServiceProject.net.mappoint.staging;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace MapPointWebServiceProject
{

    public class MapPointWebServiceHelper
    {

        public const double MAPPOINT_DEFAULT_ZOOM = 1.0;
        public const int SECOND = 1000;
        public const int MAPPOINT_DEFAULT_TIMEOUT = 15 * 1000;

        const string MAPPOINT_NA = "MapPoint.NA";
        private static FindServiceSoap theMapPointFindService;
        private static RouteServiceSoap theMapPointRouteService;
        private static RenderServiceSoap theMapPointRenderService;


        // here is the url for the WSDL http://staging.mappoint.net/standard-30/mappoint.wsdl
        private NetworkCredential netCreds;

        private WebProxy webProxy = null;
        public WebProxy WebProxy
        {
            get { return WebProxy; }
            set
            {
                if (value == null)
                    throw new System.ArgumentNullException("WebProxy is null");

                theMapPointFindService.Proxy = value;
                theMapPointRenderService.Proxy = value;
                theMapPointRouteService.Proxy = value;
            }
        }


        /// <summary>
        /// Make the constructor private becasuse we want the GetInstance be used.
        /// </summary>
		private MapPointWebServiceHelper() {}

        /// <summary>
        /// Make the constructor private becasuse we want the GetInstance be used.
        /// </summary>
        /// <param name="userid">MapPoint userid</param>
        /// <param name="password">MapPoint password</param>
		private MapPointWebServiceHelper(string userid, string password) 
		{
			try
			{
			
				netCreds = new NetworkCredential(userid, password);

				theMapPointFindService = new FindServiceSoap();
				theMapPointFindService.Credentials = netCreds;
				theMapPointFindService.Timeout = MAPPOINT_DEFAULT_TIMEOUT;
				if (webProxy != null) theMapPointFindService.Proxy = webProxy;

				theMapPointRouteService = new RouteServiceSoap();
				theMapPointRouteService.Credentials = netCreds;
				theMapPointRouteService.Timeout = MAPPOINT_DEFAULT_TIMEOUT;
				if (webProxy != null) theMapPointRouteService.Proxy = webProxy;

                // setup route distances to be in miles
                UserInfoRouteHeader routeHeader = new UserInfoRouteHeader();
                // set distance in miles
                routeHeader.DefaultDistanceUnit = DistanceUnit.Mile;
                theMapPointRouteService.UserInfoRouteHeaderValue = routeHeader;

				theMapPointRenderService = new RenderServiceSoap();
				theMapPointRenderService.Credentials = netCreds;
				theMapPointRenderService.Timeout = MAPPOINT_DEFAULT_TIMEOUT;
				if (webProxy != null) theMapPointRenderService.Proxy = webProxy;
			}
			catch (Exception e)
			{
				throw e; // app can handle it

			}

		}

#region "Singleton initialization"

        /// <summary>
        /// This is used to hold onto an instance of MapPointHelper if its already been created
        /// </summary>
		private static MapPointWebServiceHelper MapPointHelperInstance; 

        /// <summary>
        /// This method is used to create and return an instance of the MapPointWebServiceHelper.  Need a MapPoint
        /// account?  Get a developer account here https://mappoint-css.partners.extranet.microsoft.com/MwsSignup/Eval.aspx
        /// </summary>
        /// <param name="userid">MapPoint userid</param>
        /// <param name="password">MapPoint password</param>
        /// <returns></returns>
		public static MapPointWebServiceHelper GetInstance(string userid, string password)
		{
			if (MapPointHelperInstance == null)
                	 MapPointHelperInstance = new MapPointWebServiceHelper(userid, password);

			return MapPointHelperInstance;
																																													 
		}

        /// <summary>
        /// This method is used to create and return an instance of the MapPointWebServiceHelper.  This method 
        /// supports the use of a WebProxy, which maybe needed in environments where Proxy Servers are
        /// in place.
        /// </summary>
        /// <param name="userid">MapPoint userid</param>
        /// <param name="password">MapPoint password</param>
        /// <param name="proxy">an instance of the WebProxy class set to use the appropriate Proxy</param>
        /// <returns></returns>
		public static MapPointWebServiceHelper GetInstance(string userid, string password, string proxy)
		{
			GetInstance(userid, password);
			MapPointHelperInstance.WebProxy = new WebProxy(proxy);

			return MapPointHelperInstance;
		}
	
#endregion

		/// <summary>
		/// Find a location like an address (1500 Central rd., Chicago, IL) or landmark (Sears tower or Navy Pier)
		/// </summary>
		/// <param name="locationString">address or landmark</param>
		/// <returns>Location</returns>
		public Location FindLocation(string locationString)
        {
            Location[] location = null;

            try
            {
                if (locationString == "")
                {
                    throw new System.ArgumentNullException("Location cannot be empty");
                }

                FindSpecification myFindSpec = new FindSpecification();
                myFindSpec.InputPlace = locationString;
                myFindSpec.DataSourceName = "MapPoint.NA";
                FindResults results = theMapPointFindService.Find(myFindSpec);

                // if there is no result found try it as an address instead
                if (results.NumberFound == 0)
                {

                    // if you want to use addresses instead you can use the code below
                    Address address = theMapPointFindService.ParseAddress(locationString, "USA");
                    FindAddressSpecification myFindASpec = new FindAddressSpecification();
                    myFindASpec.DataSourceName = "MapPoint.NA";
                    myFindASpec.InputAddress = address;
                    results = theMapPointFindService.FindAddress(myFindASpec);

                }


                // at this point a place (e.g. Sears Tower) or an address was not found so
                // return an error
                if (results.NumberFound == 0)
                {
                    throw new System.ArgumentNullException("Location cannot be found");
                }

                return results.Results[0].FoundLocation;


            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle

            }

        }

        /// <summary>
        /// Version of FindLocation that searches using a LatLong object
        /// </summary>
        /// <param name="aLatLong">a LatLong</param>
        /// <returns></returns>
        public Location FindLocation(LatLong aLatLong)
        {

            Location[] location = null;

            try
            {

                if (aLatLong == null)
                {
                    throw new System.ArgumentNullException("LatLong cannot be null");
                }

                //OK find something
                FindSpecification fs = new FindSpecification();
                fs.DataSourceName = "MapPoint.NA";

                LatLong locationLatLong = aLatLong;
                GetInfoOptions infoOptions = new GetInfoOptions();

                location = theMapPointFindService.GetLocationInfo(locationLatLong,
                    MAPPOINT_NA, infoOptions);

            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle

            }


            return location[0];  // zero position should be the best match
        }

        /// <summary>
        /// This method turns a LatLong into a zip code
        /// </summary>
        /// <param name="aLatLong">LatLong object</param>
        /// <returns>string</returns>
        public string GetPostalCode(LatLong aLatLong)
        {
            Address address;

            try
            {
                address = GetAddress(aLatLong);
                if (address != null)
                    return address.PostalCode;
                else
                    return null;
            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle
            }


        }

        /// <summary>
        /// This method approximates an address from a LatLong
        /// </summary>
        /// <param name="aLatLong">a LatLong</param>
        /// <returns></returns>
        public Address GetAddress(LatLong aLatLong)
        {

            Location location = null;

            try
            {
                location = FindLocation(aLatLong);

                return location.Address;

            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle
            }
        }

        /// <summary>
        /// This method will return a image representing a map
        /// </summary>
        /// <param name="location">result from the FindLocation method</param>
        /// <param name="imageWidth">width of image used to display the map</param>
        /// <param name="imageHeight">height of the image used to display the map</param>
        /// <param name="zoom">floating point number > 0 respresent amount of zoom on the returned map</param>
        /// <returns>Address</returns>
        public System.Drawing.Image GetMap(Location location, int imageWidth, int imageHeight, double zoom)
        {

            MapImage[] mapImage;
            System.Drawing.Bitmap theImage = null;

            try
            {

                if (location == null)
                {
                    throw new System.ArgumentNullException("Location cannot be null");
                }

                if (imageWidth <= 0)
                {
                    throw new System.ArgumentNullException("Image width should be > then 0");
                }

                if (imageHeight <= 0)
                {
                    throw new System.ArgumentNullException("Image Height should be > then 0");
                }
                if (zoom <= 0)
                {
                    throw new System.ArgumentNullException("Zoom should be great then 0");
                }

                // now get a map
                MapSpecification mapSpec = new MapSpecification();

                //ViewByScale[] myViews = new ViewByScale[1];
                MapView[] myViews = new MapView[1];
                myViews[0] = location.BestMapView.ByHeightWidth;

                mapSpec.Views = myViews;
                mapSpec.DataSourceName = "MapPoint.NA";
                mapSpec.Options = new MapOptions();
                mapSpec.Options.Format = new ImageFormat();
                mapSpec.Options.Format.Height = imageHeight;
                mapSpec.Options.Format.Width = imageWidth;
                mapSpec.Options.Zoom = zoom;

                //	setup pushpin
                Pushpin[] ppArray = new Pushpin[1];

                // set up a sample push pin
                ppArray[0] = new Pushpin();
                ppArray[0].IconDataSource = "MapPoint.Icons";
                ppArray[0].IconName = "168";
                ppArray[0].LatLong = location.LatLong;
                ppArray[0].Label = "You are here";
                ppArray[0].ReturnsHotArea = true;
                // PinID contains the db key and the ADE NAME seperated by a tilde
                ppArray[0].PinID = "where I am now";

                mapSpec.Pushpins = ppArray;

                mapImage = theMapPointRenderService.GetMap(mapSpec);

                // let sure an MapImage was returned
                if (mapImage != null && mapImage.Length > 0)
                {
                    theImage = new System.Drawing.Bitmap(new System.IO.MemoryStream(mapImage[0].MimeData.Bits));
                }
                else
                    throw new System.Exception("Unable to build a map for this route");


            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle
            }

            return theImage;

        }

        /// <summary>
        /// Methods for looking up points of interest based on a location.  Points of interest could 
        /// be things like gas, food and lodging.  Standard Industry Codes (SIC) are used to identify the type
        /// of point of interest you are interested in.  Some example sic codes are SICInd554 for Gas, SIC5800 for 
        /// Food and SICInd701 for lodging.
        /// 
        /// The data source is NavTech.NA
        /// 
        /// </summary>
        /// <param name="location">result from the FindLocation method</param>
        /// <param name="POI">Entity code - like the SIC example above</param>
        /// <returns>FindResults</returns>
        public FindResults FindNearByPlaces(Location location, string POI)
        {
            return this.FindNearByPlaces(location, POI, 50.0);
        }
        /// <summary>
        /// Methods for looking up points of interest based on a location.  Points of interest could 
        /// be things like gas, food and lodging.  Standard Industry Codes (SIC) are used to identify the type
        /// of point of interest you are interested in.  Some example sic codes are SICInd554 for Gas, SIC5800 for 
        /// Food and SICInd701 for lodging.
        /// 
        /// Search radius is assumed to be 50 miles.
        /// 
        /// The data source is NavTech.NA
        /// 
        /// <param name="location">result from the FindLocation method</param>
        /// <param name="POI">Entity code - like the SIC example above</param>
        /// <param name="searchRadius">float allowing of specify the search radius</param>
        /// <returns>FindResults</returns>
        public FindResults FindNearByPlaces(Location location, string POI, double searchRadius)
        {
            return this.FindNearByPlaces(location, POI, "Navtech.NA", searchRadius);
        }

        /// <summary>
        /// Methods for looking up points of interest based on a location.  Points of interest could 
        /// be things like gas, food and lodging.  Standard Industry Codes (SIC) are used to identify the type
        /// of point of interest you are interested in.  Some example sic codes are SICInd554 for Gas, SIC5800 for 
        /// Food and SICInd701 for lodging.
        /// 
        /// You must specify the datasource like NavTech.NA
        /// 
        /// </summary>
        /// <param name="location">result from the FindLocation method</param>
        /// <param name="POI">Entity code - like the SIC example above</param>
        /// <param name="dataSource"></param>
        /// <param name="searchRadius">float allowing of specify the search radius</param>
        /// <returns>FindResults</returns>
        public FindResults FindNearByPlaces(Location location, string POI,string dataSource, double searchRadius)
        {

            FindResults foundResults = null;

            try
            {

                if (location == null)
                {
                    throw new System.ArgumentNullException("Location cannot be null");
                }

                if (POI == null)
                {
                    throw new System.ArgumentNullException("POI cannot be null");
                }

                if (dataSource == null)
                {
                    throw new System.ArgumentNullException("dataSource cannot be null");
                }


                string POIDatasourceName = dataSource;
                //string POIEntityTypeName = PointsOfInterest.GetInstance().GetPointOfInterestSICCODE(POI);

                FindNearbySpecification findNearbySpec = new FindNearbySpecification();
                findNearbySpec.DataSourceName = POIDatasourceName;
                findNearbySpec.Distance = searchRadius;  // (myViews(0).Width / 2) * 1.2
                findNearbySpec.LatLong = location.LatLong;
                findNearbySpec.Filter = new FindFilter();
                findNearbySpec.Filter.EntityTypeName = POI;
                findNearbySpec.Options = new FindOptions();
                findNearbySpec.Options.Range = new FindRange();
                findNearbySpec.Options.Range.Count = 99;

                foundResults = theMapPointFindService.FindNearby(findNearbySpec);

                // make sure NumberFound and size of results array are the same
                foundResults.NumberFound = foundResults.Results.GetLength(0);
            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle
            }

            return foundResults;
        }

        /// <summary>
        /// Return a Route given a start and end location.
        /// </summary>
        /// 
        /// <param name="startLocation">result from the FindLocation method</param>
        /// <param name="endLocation">result from the FindLocation method</param>
        /// <returns>Route</returns>
        public Route GetRoute(Location startLocation, Location endLocation)
        {
            return this.GetRoute(startLocation, endLocation, SegmentPreference.Quickest);
        }

        /// <summary>
        /// Return a Route given a start and end location.
        /// </summary>
        /// <param name="startLocation">result from the FindLocation method</param>
        /// <param name="endLocation">result from the FindLocation method</param>
        /// <param name="segPref">specify if you want the shortest or quickest route</param>
        /// <returns>Route</returns>
        public Route GetRoute(Location startLocation, Location endLocation, SegmentPreference segPref)
        {

            Route myRoute;

            try
            {
                if (startLocation == null)
                {
                    throw new System.ArgumentNullException("Start location cannot be null");
                }
                if (endLocation == null)
                {
                    throw new System.ArgumentNullException("End location cannot be null");
                }


                SegmentSpecification[] routeSegmentsSpec = new SegmentSpecification[2];
                routeSegmentsSpec[0] = new SegmentSpecification();
                routeSegmentsSpec[0].Waypoint = new Waypoint();
                routeSegmentsSpec[0].Waypoint.Name = startLocation.Entity.Name;
                routeSegmentsSpec[0].Waypoint.Location = startLocation;
                routeSegmentsSpec[0].Options = new SegmentOptions();
                routeSegmentsSpec[0].Options.Preference = segPref;
                routeSegmentsSpec[1] = new SegmentSpecification();
                routeSegmentsSpec[1].Waypoint = new Waypoint();
                routeSegmentsSpec[1].Waypoint.Name = endLocation.Entity.Name;
                routeSegmentsSpec[1].Waypoint.Location = endLocation;
                routeSegmentsSpec[1].Options = new SegmentOptions();
                routeSegmentsSpec[1].Options.Preference = segPref;

                RouteSpecification routeSpec = new RouteSpecification();
                routeSpec.DataSourceName = "MapPoint.NA";
                routeSpec.Segments = routeSegmentsSpec;
				

                myRoute = theMapPointRouteService.CalculateRoute(routeSpec);
	
            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle

            }

            return myRoute;

        }

		/// <summary>
		/// This method returns the direction (itinerary) between a starting and ending location
		/// </summary>
		/// <param name="startLocation">Starting location</param>
		/// <param name="endLocation">Ending location</param>
        /// <param name="segPref">specify the shortest or quickest route</param>
		/// <returns>a RouteItinerary - just the directions portion of a route</returns>
        public RouteItinerary GetRouteDirections(Location startLocation, Location endLocation, SegmentPreference segPref)
		{

			Route myRoute;

			try
			{
				if (startLocation == null)
				{
					throw new System.ArgumentNullException("Start location cannot be null");
				}
				if (endLocation == null)
				{
					throw new System.ArgumentNullException("End location cannot be null");
				}


				SegmentSpecification[] routeSegmentsSpec = new SegmentSpecification[2];
				routeSegmentsSpec[0] = new SegmentSpecification();
				routeSegmentsSpec[0].Waypoint = new Waypoint();
				routeSegmentsSpec[0].Waypoint.Name = startLocation.Entity.Name;
				routeSegmentsSpec[0].Waypoint.Location = startLocation;
                routeSegmentsSpec[0].Options = new SegmentOptions();
                routeSegmentsSpec[0].Options.Preference = segPref;
				routeSegmentsSpec[1] = new SegmentSpecification();
				routeSegmentsSpec[1].Waypoint = new Waypoint();
				routeSegmentsSpec[1].Waypoint.Name = endLocation.Entity.Name;
				routeSegmentsSpec[1].Waypoint.Location = endLocation;
                routeSegmentsSpec[1].Options = new SegmentOptions();
                routeSegmentsSpec[1].Options.Preference = segPref;

				RouteSpecification routeSpec = new RouteSpecification();
				routeSpec.DataSourceName = "MapPoint.NA";
				routeSpec.ResultMask = RouteResultMask.Itinerary;
				routeSpec.Segments = routeSegmentsSpec;
				

				myRoute = theMapPointRouteService.CalculateRoute(routeSpec);
	
			}
			catch (ArgumentNullException e)
			{
				throw e;  // rethrow for app to handle
			}
			catch (Exception e)
			{
				throw e;  // rethrow for app to handle

			}

			return myRoute.Itinerary;

		}

        /// <summary>
        /// This method returns the direction (itinerary) between a starting and ending location
        /// </summary>
        /// <param name="startLocation">Starting location</param>
        /// <param name="endLocation">Ending location</param>
        /// <returns>a RouteItinerary - just the directions portion of a route</returns>
        public RouteItinerary GetRouteDirections(Location startLocation, Location endLocation)
        {
            return this.GetRouteDirections(startLocation, endLocation, SegmentPreference.Quickest);
        }

        /// <summary>
        /// Utility method to strip down the RouteItinerary and just return the directions
        /// as an array of string
        /// </summary>
        /// <param name="theRouteItin">a RouteItenary</param>
        /// <returns>Array of string that is the text portion of the route</returns>
        public string[] RouteItineraryToText(RouteItinerary theRouteItin)
        {
            List<string> directions = new List<string>(100);

            try 
	        {
                if (theRouteItin == null)
                {
                    throw new System.ArgumentNullException("Start location cannot be null");
                }

                foreach (Segment s in theRouteItin.Segments)
                {
                    foreach (Direction d in s.Directions)
                    {
                        directions.Add(d.Instruction);
                    }
                }

        	}
	        catch (Exception ex)
	        {
        		
                throw ex;  // rethrow for app to handle
	        }

            return directions.ToArray();
        }

        /// <summary>
        /// Return a image representing a map showing the start and ending points and an annotated line for
        /// the route between those points
        /// </summary>
        /// <param name="route">a Route</param>
        /// <param name="imageWidth">width of image used to display the map</param>
        /// <param name="imageHeight">height of the image used to display the map</param>
        /// <returns>Image</returns>
        public System.Drawing.Image GetRouteMap(Route route, int imageWidth, int imageHeight)
        {

            MapImage[] mapImage;
            System.Drawing.Bitmap theImage = null;

            try
            {

                if (route == null)
                {
                    throw new System.ArgumentNullException("Route cannot be null");
                }

                //if (location == null)
                //{
                //    throw new System.ArgumentNullException("Location cannot be null");
                //}

                if (imageWidth <= 0)
                {
                    throw new System.ArgumentNullException("Image width should be > then 0");
                }

                if (imageHeight <= 0)
                {
                    throw new System.ArgumentNullException("Image Height should be > then 0");
                }

                // now get a map
                MapSpecification mapSpec = new MapSpecification();

                ViewByHeightWidth[] myViews = new ViewByHeightWidth[1];
                myViews[0] = route.Itinerary.View.ByHeightWidth;

                mapSpec.Views = myViews;
                mapSpec.DataSourceName = "MapPoint.NA";
                mapSpec.Options = new MapOptions();
                mapSpec.Options.Format = new ImageFormat();
                mapSpec.Options.Format.Height = imageHeight;
                mapSpec.Options.Format.Width = imageWidth;

                //	setup pushpin
                Pushpin[] ppArray = new Pushpin[2];

                // set up start
                ppArray[0] = new Pushpin();
                ppArray[0].IconDataSource = "MapPoint.Icons";
                ppArray[0].IconName = "31";
                ppArray[0].LatLong = route.Itinerary.Segments[0].Waypoint.Location.LatLong;
                ppArray[0].Label = "Start";
                ppArray[0].ReturnsHotArea = true;
                // PinID 
                ppArray[0].PinID = "Start";

				// set up end
				ppArray[1] = new Pushpin();
				ppArray[1].IconDataSource = "MapPoint.Icons";
				ppArray[1].IconName = "29";
				ppArray[1].LatLong = route.Itinerary.Segments[route.Itinerary.Segments.GetUpperBound(0)].Waypoint.Location.LatLong;
				ppArray[1].Label = "End";
				ppArray[1].ReturnsHotArea = true;
				// PinID 
				ppArray[1].PinID = "End";


                mapSpec.Pushpins = ppArray;

                mapSpec.Route = route;

                mapImage = theMapPointRenderService.GetMap(mapSpec);

                // let sure an MapImage was returned
                if (mapImage != null && mapImage.Length > 0)
                {
                    theImage = new System.Drawing.Bitmap(new System.IO.MemoryStream(mapImage[0].MimeData.Bits));
                }
                else
                    throw new System.Exception("Unable to build a map for this route");

            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle

            }

            return theImage;

        }

        /// <summary>
        /// Return the Traffic Incidents along a given route
        /// </summary>
        /// <param name="route">a route</param>
        /// <param name="distance">a double representing the distance from the route for which
        /// you want incidents reports</param>
        /// <returns>FindResults</returns>
        public FindResults GetTrafficIncident(Route route, Double distance)
        {

            FindResults foundResults;

            try
            {

                if (route == null)
                {
                    throw new System.ArgumentNullException("End location cannot be null");
                }

                FindFilter ff = new FindFilter();
                ff.EntityTypeName = "TrafficIncident";

                FindNearRouteSpecification spec = new FindNearRouteSpecification();
                spec.DataSourceName = "MapPointTravel.TrafficIncidents";
                spec.Distance = distance; //show all incidents within 1 mile of the route
                spec.Route = route; //arg passed in
                spec.Filter = ff;

                foundResults = new FindResults();
                foundResults = theMapPointFindService.FindNearRoute(spec);
            }
            catch (ArgumentNullException e)
            {
                throw e;  // rethrow for app to handle
            }
            catch (Exception e)
            {
                throw e;  // rethrow for app to handle

            }

            return foundResults;
        }
    }
}
