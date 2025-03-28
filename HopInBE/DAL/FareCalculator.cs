using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using HopInBE.DataAccess;

namespace HopInBE.Helpers
{
    public class FareCalculator
    {
        public double CalculateFare(double distanceInKm, DateTime requestTime)
        {
            double baseFare = ConfigHelper.GetAppSetting<double>("BaseFare", "FareSettings");
            double perKmRate = ConfigHelper.GetAppSetting<double>("PerKmRate", "FareSettings");

            // Default multiplier
            double multiplier = 1.0;

            // Retrieve Peak Hour Multipliers from Config
            var peakHourMultipliers = ConfigHelper.BindSection<Dictionary<string, double>>("FareSettings:PeakHourMultipliers");

            foreach (var peakHour in peakHourMultipliers)
            {
                var times = peakHour.Key.Split('-');
                TimeSpan start = TimeSpan.Parse(times[0]);
                TimeSpan end = TimeSpan.Parse(times[1]);

                if (requestTime.TimeOfDay >= start && requestTime.TimeOfDay <= end)
                {
                    multiplier = peakHour.Value;
                    break;
                }
            }

            // Apply Night Fare Multiplier
            string nightHoursRange = ConfigHelper.GetAppSetting<string>("NightHours", "FareSettings");
            double nightMultiplier = ConfigHelper.GetAppSetting<double>("NightFareMultiplier", "FareSettings");

            var nightTimes = nightHoursRange.Split('-');
            TimeSpan nightStart = TimeSpan.Parse(nightTimes[0]);
            TimeSpan nightEnd = TimeSpan.Parse(nightTimes[1]);

            // Night charge applies if request is between 22:00 - 06:00
            if ((requestTime.TimeOfDay >= nightStart) || (requestTime.TimeOfDay <= nightEnd && nightStart > nightEnd))
            {
                multiplier = Math.Max(multiplier, nightMultiplier);
            }

            return (baseFare + (perKmRate * distanceInKm)) * multiplier;
        }
    }
}
