using System;

namespace LFNet.Licensing
{
    public class LicenseVerificator
    {
        private static readonly DateTime VisualStudio2010Beta2ReleaseDate = new DateTime(2009, 10, 15);
        public static bool IsValid(License license, DateTime now)
        {
            return license != null && LicenseVerificator.IsCorrect(license) && LicenseVerificator.IsStarted(license, now) && !LicenseVerificator.IsExpired(license, now) && !LicenseVerificator.IsOutdatedForVersion2x(license);
        }
        public static bool IsCorrect(License license)
        {
            if (license.Version < 1 || license.Version > 2)
            {
                return false;
            }
            switch (license.Type)
            {
                case LicenseType.Evaluation:
                    if (license.EndTime == DateTime.MaxValue)
                    {
                        return false;
                    }
                    break;
                case LicenseType.Personal:
                    if (license.Binding != LicenseBinding.User)
                    {
                        return false;
                    }
                    if (license.Capacity != 1)
                    {
                        return false;
                    }
                    break;
                case LicenseType.Corporate:
                    if (license.Binding != LicenseBinding.Seat)
                    {
                        return false;
                    }
                    if (license.Capacity == 2147483647)
                    {
                        return false;
                    }
                    break;
                case LicenseType.Classroom:
                    if (license.Binding != LicenseBinding.Seat)
                    {
                        return false;
                    }
                    if (license.Capacity == 2147483647)
                    {
                        return false;
                    }
                    break;
                case LicenseType.OpenSource:
                    if (license.Binding != LicenseBinding.User)
                    {
                        return false;
                    }
                    if (license.Capacity != 1 && license.Capacity != 2147483647)
                    {
                        return false;
                    }
                    break;
                case LicenseType.Student:
                    if (license.Binding != LicenseBinding.User)
                    {
                        return false;
                    }
                    if (license.Capacity != 1)
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
        public static bool IsExpired(License license, DateTime now)
        {
            return license.EndTime < now;
        }
        public static bool IsOutdatedForVersion2x(License license)
        {
            return !(license.PurchaseDate == DateTime.MinValue) && !(license.PurchaseDate == DateTime.MaxValue) && license.Type != LicenseType.OpenSource && !LicenseVerificator.IsTimeLimited(license) && license.PurchaseDate < LicenseVerificator.VisualStudio2010Beta2ReleaseDate;
        }
        public static bool IsExpiringSoon(License license, DateTime now)
        {
            return LicenseVerificator.DaysToExpire(license, now) <= 7;
        }
        public static int DaysToExpire(License license, DateTime now)
        {
            return (int)(license.EndTime - now).TotalDays;
        }
        public static bool IsStarted(License license, DateTime now)
        {
            return license.StartTime <= now;
        }
        public static bool IsTimeLimited(License license)
        {
            return license.StartTime != DateTime.MinValue && license.EndTime != DateTime.MaxValue;
        }
        public static bool IsPregenerated(License license)
        {
            return license.IsPregenerated;
        }
    }
}