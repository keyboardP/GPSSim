using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Threading;
using System.Windows.Threading;

namespace GPSSim_Consumer
{
    public partial class MainPage : PhoneApplicationPage
    {
        //used to turn the readings on and off
        private bool toggleStart = false;

        //Create the client to consume the WCF data
        ServiceReference1.GPSSimClient client = new ServiceReference1.GPSSimClient();

        //this timer will determine how often the readings should be downloaded
        DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        //next three variables are used to calculate the threshold distance
        double oldLon = 0.0;
        double oldLat = 0.0;
        double oldMeters = 0.0;

        //adjust how many meters before the 'beyond threshold' call is triggered
        const int THRESHOLD = 1; 
        


        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //assign the event handler to the tick event
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            //choose how often should the readings be taken
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);

            //what to do when the reading has been taken (don't forget, all Silverlight calls are ASync, so you need to handle
            //the completed event to get the result
            client.GetGPSDataCompleted += new EventHandler<ServiceReference1.GetGPSDataCompletedEventArgs>(client_GetGPSDataCompleted);

            //start the timer
            dispatcherTimer.Start();

        }


        void client_GetGPSDataCompleted(object sender, ServiceReference1.GetGPSDataCompletedEventArgs e)
        {
            //get the result
            var data = e.Result;

            //data[0] = latitude, data[1] = longitude
            tblockLat.Text = data[0].ToString(); 
            tblockLong.Text = data[1].ToString();

            //display whether the threshold has been reached (i.e. a certain distance has been travelled)
            tblockThreshold.Text = CheckThreshold(oldLon, oldLat, data[0], data[1]).ToString();
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //if the user has started the reading
            if (toggleStart)
            {
                //download the reading whenever the interval is called
                client.GetGPSDataAsync();              
            }
           
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //toggle the boolean value to turn readings on and off
            toggleStart = !toggleStart;          
        }


        public bool CheckThreshold(double clon, double clat, double nLon, double nLat)
        {


            //thanks to code found here (http://forums.silverlight.net/forums/p/191064/441241.aspx#441241)
            clon *= Math.PI / 180;
            clat *= Math.PI / 180;
            var dstLongR = nLon * Math.PI / 180;
            var dstLatR = nLat * Math.PI / 180;
            var earthRadius = 3958.75587;
            var distance = Math.Acos(Math.Sin(clat) * Math.Sin(dstLatR) +
                        Math.Cos(clat) * Math.Cos(dstLatR) * Math.Cos((dstLongR - clat))) * earthRadius;
            distance = Math.Floor(distance * 10) / 10;


            //convert miles to meters since WP7 emulator has meters threshold, so probably more useful :)
            double meters = distance * 1609.344;


            //assign the old values as the current value, so in the next loop it represents the previous value
            oldLat = nLat;
            oldLon = nLon;


            //if the distance travelled is greater than the threshold return true, otherwise return false
            if (Math.Abs(meters - oldMeters) > THRESHOLD)
            {
                oldMeters = meters;
                return true;
            }
            else
            {
                oldMeters = meters;
                return false;
            }

                     
            
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            client.CloseAsync();
            base.OnNavigatedFrom(e);
        }
    }
}
