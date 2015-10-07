using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Description;

namespace GPSSim
{
    [ServiceContract]
    public interface IGPSSim
    {
        [OperationContract]
        double[] GetGPSData();
    }

    public class GPSSim : IGPSSim
    {

        public double[] GetGPSData()
        {
            Form1 currentForm = Application.OpenForms[0] as Form1;
            double[] data = currentForm.GetData();
            

            return data;
        }


    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //run the program and visit this URI to confirm the service is working
            Uri baseAddress = new Uri("http://localhost:8000/GPSSim");
            ServiceHost host = new ServiceHost(typeof(GPSSim), baseAddress);
            
            //basicHttpBinding is used because WS binding is currently unsupported
            host.AddServiceEndpoint(typeof(IGPSSim), new BasicHttpBinding(),   "GPS Sim Service");

            try
            {
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                host.Description.Behaviors.Add(smb);
            }
            catch (CommunicationException e)
            {
                Console.WriteLine("An exception occurred: {0}", e.Message);
                host.Abort();
            }

            host.Open();
                

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
