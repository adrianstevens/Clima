﻿using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Gateway.WiFi;
using Meadow.Peripherals.Leds;
using Clima.Meadow.Pro.DataAccessLayer;
using Meadow.Foundation;
using Clima.Meadow.Pro.Models;

namespace Clima.Meadow.Pro
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //==== peripherals
        RgbPwmLed onboardLed;

        //==== controllers and such

        public MeadowApp()
        {
            //==== new up our peripherals
            Initialize().Wait();

            // start our sensor updating
            Console.WriteLine("Here");

            // subscribe to climate updates and save them to the database
            ClimateMonitorAgent.Instance.ClimateConditionsUpdated += (s, e) => {
                // start the DbManager
                Console.WriteLine("Update data");
                DebugOut(e.New);
                LocalDbManager.Instance.SaveReading(e);

                Console.WriteLine("Get reading from DB");
                var reading = LocalDbManager.Instance.GetClimateReading(0);
                Console.WriteLine("Got data");
                DebugOut(reading);
            };

            ClimateMonitorAgent.Instance.StartUpdating(TimeSpan.FromSeconds(10));

            Console.WriteLine("MeadowApp finished ctor.");
        }

        /// <summary>
        /// Initializes the hardware.
        /// </summary>
        async Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            //==== onboard LED
            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                IRgbLed.CommonType.CommonAnode);

            Console.WriteLine("RgbPwmLed up");
            onboardLed.SetColor(WildernessLabsColors.ChileanFire);

            /*

            //==== coprocessor (WiFi and Bluetooth)
            Console.WriteLine("Initializaing coprocessor.");
            await Device.InitCoprocessor();
            onboardLed.SetColor(WildernessLabsColors.PearGreen);

            //==== connect to wifi
            Console.WriteLine($"Connecting to WiFi Network {Secrets.WIFI_NAME}");
            try {
                var connectionResult = await Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD);
                if (connectionResult.ConnectionStatus != ConnectionStatus.Success) {
                    throw new Exception($"Cannot connect to network: {connectionResult.ConnectionStatus}");
                }
                Console.WriteLine($"Connected to {Secrets.WIFI_NAME}.");
                onboardLed.SetColor(WildernessLabsColors.AzureBlue);
            } catch (Exception e) {
                Console.WriteLine($"Err when connecting to WiFi: {e.Message}");
            }

            */

            Console.WriteLine("Hardware initialization complete.");
        }

        protected void DebugOut(Climate climate)
        {
            Console.WriteLine("New climate reading:");
            Console.WriteLine($"Temperature: {climate.Temperature?.Celsius:N2}C");
            Console.WriteLine($"Pressure: {climate.Pressure?.Millibar:N2}millibar");
            Console.WriteLine($"Humidity: {climate.Humidity:N2}%");
            Console.WriteLine($"Wind Direction: {climate.WindDirection?.Compass16PointCardinalName}");
        }
    }
}