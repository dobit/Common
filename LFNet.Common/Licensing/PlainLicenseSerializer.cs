using System;

namespace LFNet.Licensing
{
    public class PlainLicenseSerializer
    {
        private const int EvaluationPeriod = 30;
        private const int xxx = 10;
        private static int dayIndex = 4;
        private static int monthIndex = 18;
        private static int yearIndex = 10;
        public License Deserialize(string key)
        {
            License license = new License();
            license.StartTime = PlainLicenseSerializer.ParsePlainDate(key);
            license.EndTime = license.StartTime.AddDays(30.0);
            license.Type = LicenseType.Evaluation;
            license.LicensedTo = "Evaluation User";
            license.IsPregenerated = true;
            license.Capacity = 1;
            license.Binding = LicenseBinding.Seat;
            return license;
        }
        private static DateTime ParsePlainDate(string plainKey)
        {
            DateTime result;
            try
            {
                if (plainKey.Length < 10)
                {
                    throw new LicensingException("Cannot parse license date.");
                }
                string text = "";
                for (int i = 0; i < plainKey.Length; i++)
                {
                    text += plainKey[i] - '\n';
                }
                int day = int.Parse(text.Substring(PlainLicenseSerializer.dayIndex, 2));
                int month = int.Parse(text.Substring(PlainLicenseSerializer.monthIndex, 2));
                int year = int.Parse(text.Substring(PlainLicenseSerializer.yearIndex, 4));
                result = new DateTime(year, month, day);
            }
            catch (FormatException inner)
            {
                throw new LicensingException("Cannot parse license date. Date format is invalid.", inner);
            }
            catch (OverflowException inner2)
            {
                throw new LicensingException("Cannot parse license date. Date is not in correct range.", inner2);
            }
            return result;
        }
    }
}