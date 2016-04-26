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
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using RaspberrypiTest1;

namespace RaspberryPI_SensorTest
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private I2cDevice I2CAccel;
        private DispatcherTimer timer;
        private ConnectTheDotsHelper ctdHelper;
        private List<ConnectTheDotsSensor> sensors; //dummy Sensor

        public MainPage()
        {
            this.InitializeComponent();
            Text_X_Axis.Text = "AA";

            InitSensor();
            StartScenarioAsync();
            
        }

        private async Task StartScenarioAsync()
        {            
            try
            {
                var settings = new I2cConnectionSettings(0x49);
                settings.BusSpeed = I2cBusSpeed.FastMode;                       /* 400KHz bus speed */

                string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller devices with our selector string             */
                I2CAccel = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings    */
                if (I2CAccel == null)
                {
                    System.Diagnostics.Debug.WriteLine("I2CAccel Not found");
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to communicate with device: " + ex.Message);
                return;
            }

            
            // Start the polling timer.
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };
            timer.Tick += Timer_Tick;
            timer.Start();            
        }

        private void Timer_Tick(object sender, object e)
        {
            // Read data from I2C.
            var command = new byte[1];
            var temperatureData = new byte[2];
            
            //command[0] = 0xE3;
            command[0] = 0x00;            

           // *Read and format teperature data */
            try
            {
                // If this next line crashes, then there was an error accessing the sensor.
                I2CAccel.WriteRead(command, temperatureData);
                

                //string result = BitConverter.ToString(temperatureData);
                //System.Diagnostics.Debug.WriteLine("temperatureData: " + result);

                //short temperature = BitConverter.ToInt16(temperatureData, 0);                
                //System.Diagnostics.Debug.WriteLine("temperatureData: " + temperature);

                int temperature = temperatureData[0];
                System.Diagnostics.Debug.WriteLine("temperatureData: " + temperature);

                String xText = String.Format("{0:F3}", temperature);
                Text_X_Axis.Text = xText;

                ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
                sensor.value = temperature;
                ctdHelper.SendSensorData(sensor);
                                   
            }
            catch (Exception ex)
            {
                String xText = String.Format("X Axis: Error");
                System.Diagnostics.Debug.WriteLine("X Axis: Error : " + ex);
            }
        }

        private void InitSensor()
        {
            // Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
            List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
                new ConnectTheDotsSensor("ace60e7c-a6aa-4694-ba86-c3b66952558e", "Temperature", "F"),
            };

            ctdHelper = new ConnectTheDotsHelper(serviceBusNamespace: "capstone-ns",
                eventHubName: "ehdevices",
                keyName: "D1",
                key: "YFzi8Hbg70bTumwPNP9NWuxD514RojH8ThhtRrlGwVU=",
                displayName: "aaaaa",
                organization: "bbb",
                location: "cccc",
                sensorList: sensors);
        }
    }

    
}
