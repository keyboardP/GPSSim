using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace GPSSim
{
    
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        HtmlDocument doc;
        double oldLon = 0.0;
        double oldLat = 0.0;
        double oldMeters = 0.0;

        const int THRESHOLD = 20; //adjust how many meters before the 'beyond threshold' call is triggered
        const int timerInterval = 30; //adjust to how often you want to check for updates (milliseconds)



        public Form1()
        {
            InitializeComponent();

            //setup the timer
            timer.Interval = timerInterval;
            timer.Tick += new EventHandler(timer_Tick);
           


        }

        void timer_Tick(object sender, EventArgs e)
        {
            string lat = "0.0";
            string lon = "0.0";

            try
            {

                lon = doc.GetElementById("tbLongitude").GetAttribute("Value"); //get the longitude
                lat = doc.GetElementById("tbLatitude").GetAttribute("Value"); //get the latitude
            }
            catch { } //if the tick event is too fast, the javascript controls haven't initialised so this exception may be caught for the first few ticks


            if (String.IsNullOrEmpty(lon)) lon = "0.0"; //ensure there's a correctly formatted string when app initially launches
            if (String.IsNullOrEmpty(lat)) lat = "0.0";


            tbLatitude.Text = lat.ToString();
            tbLongitude.Text = lon.ToString();

            //display whether the threshold has been reached (if tick count is low, it will flash momentarily)
            tbThreshold.Text = CheckThreshold(oldLon, oldLat, double.Parse(lon), double.Parse(lat)).ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            //Navigate to the HTML file (add your URI)
            webBrowser1.Navigate(new Uri(@"file:///C:\Users\KP\Documents\Visual Studio 2010\Projects\VEMap\VEMap\map.html", UriKind.RelativeOrAbsolute));

            //get the document in order to be able to access the coordinate information
            doc = this.webBrowser1.Document;

            //add event handler to let us know when the webbrowser control has loaded
            webBrowser1.Navigated += new WebBrowserNavigatedEventHandler(webBrowser1_Navigated);             

        }

        void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            timer.Start();
        }


        public bool CheckThreshold(double clon, double clat, double nLon, double nLat)
        {


            //thanks to code found here (http://forums.silverlight.net/forums/p/191064/441241.aspx#441241)
            clon *=  Math.PI / 180;
            clat *= Math.PI / 180;
            var dstLongR = nLon * Math.PI / 180;
            var dstLatR = nLat * Math.PI / 180;
            var earthRadius = 3958.75587; 
            var distance = Math.Acos(Math.Sin(clat) * Math.Sin(dstLatR) +
                        Math.Cos(clat) * Math.Cos(dstLatR) * Math.Cos((dstLongR - clat))) * earthRadius;
            distance = Math.Floor(distance * 10) / 10;


            //convert miles to meters since WP7 emulator has meters threshold
            double meters = distance * 1609.344f;

            
            
            //assign the old values as the current value, so in the next loop it represents the previous value
                oldLat = nLat;
                oldLon = nLon;

            
            
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



        public double[] GetData()
        {
            double[] data = new double[2];
            data[0] = double.Parse(tbLatitude.Text);
            data[1] = double.Parse(tbLongitude.Text);

            return data;
        }


     
    }
}
