using System;
using System.Text;

namespace LFNet.Licensing
{
    public class LicenseInformationFormatter
    {
        public static string Format(License license, DateTime now)
        {
            if (license == null)
            {
                return "No license";
            }
            if (!LicenseVerificator.IsCorrect(license))
            {
                return "Unknown license";
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (license.UpgradeEvaluation)
            {
                LicenseInformationFormatter.AppendLicenseOutdatedInfo(stringBuilder);
                LicenseInformationFormatter.AppendUpgradeEvaluationExpirationInfo(stringBuilder, license, now);
            }
            else
            {
                if (LicenseVerificator.IsOutdatedForVersion2x(license))
                {
                    LicenseInformationFormatter.AppendLicenseOutdatedInfo(stringBuilder);
                }
                else
                {
                    LicenseInformationFormatter.AppendLicenseTypeInfo(stringBuilder, license);
                    LicenseInformationFormatter.AppendLicensedToInfo(stringBuilder, license);
                    if (LicenseVerificator.IsTimeLimited(license))
                    {
                        LicenseInformationFormatter.AppendLicenseExpirationInfo(stringBuilder, license, now);
                    }
                }
            }
            return stringBuilder.ToString().TrimEnd(new char[]
                                                        {
                                                            '\r',
                                                            '\n'
                                                        });
        }
        private static void AppendLicenseTypeInfo(StringBuilder builder, License license)
        {
            switch (license.Type)
            {
                case LicenseType.Evaluation:
                    builder.Append("License type: Evaluation\r\n");
                    return;
                case LicenseType.Personal:
                    if (license.Version > 1)
                    {
                        builder.Append("License type: Personal\r\n");
                        return;
                    }
                    break;
                case LicenseType.Corporate:
                    builder.Append("License type: Corporate\r\n");
                    return;
                case LicenseType.Classroom:
                    builder.Append("License type: Classroom\r\n");
                    return;
                case LicenseType.OpenSource:
                    builder.Append("License type: Open Source Developer\r\n");
                    break;
                case LicenseType.Student:
                    builder.Append("License type: Student\r\n");
                    return;
                default:
                    return;
            }
        }
        private static void AppendLicensedToInfo(StringBuilder builder, License license)
        {
            switch (license.Type)
            {
                case LicenseType.Evaluation:
                    if (license.LicensedTo != "Evaluation User")
                    {
                        builder.AppendFormat("Licensed to: {0}\r\n", license.LicensedTo);
                        return;
                    }
                    break;
                case LicenseType.Personal:
                case LicenseType.Student:
                    builder.AppendFormat("Licensed to: {0}\r\n", license.LicensedTo);
                    return;
                case LicenseType.Corporate:
                case LicenseType.Classroom:
                    builder.AppendFormat("Licensed to: {0} ({1} seats)\r\n", license.LicensedTo, license.Capacity);
                    return;
                case LicenseType.OpenSource:
                    if (license.Capacity == 1)
                    {
                        builder.AppendFormat("Licensed to: {0}\r\n", license.LicensedTo);
                        return;
                    }
                    builder.AppendFormat("Licensed to: {0} (all members)\r\n", license.LicensedTo);
                    break;
                default:
                    return;
            }
        }
        private static void AppendLicenseExpirationInfo(StringBuilder builder, License license, DateTime now)
        {
            if (LicenseVerificator.IsExpired(license, now))
            {
                builder.AppendFormat("License expired on {0:d}\r\n", license.EndTime);
                return;
            }
            if (!LicenseVerificator.IsStarted(license, now))
            {
                builder.AppendFormat("License will be valid from {0:d}\r\n", license.StartTime);
                return;
            }
            int num = LicenseVerificator.DaysToExpire(license, now);
            if (num <= 30)
            {
                builder.AppendFormat("License expires in {0} day(s)\r\n", num);
                return;
            }
            builder.AppendFormat("License expires on {0:d}\r\n", license.EndTime);
        }
        private static void AppendLicenseOutdatedInfo(StringBuilder builder)
        {
            builder.AppendLine("Your VisualSVN 1.x license is not valid for VisualSVN 2.0.");
        }
        private static void AppendUpgradeEvaluationExpirationInfo(StringBuilder builder, License license, DateTime now)
        {
            if (!LicenseVerificator.IsExpired(license, now))
            {
                int num = LicenseVerificator.DaysToExpire(license, now);
                if (num <= 30)
                {
                    builder.AppendFormat("Temporary evaluation license expires in {0} day(s)\r\n", num);
                    return;
                }
                builder.AppendFormat("Temporary evaluation license expires on {0:d}\r\n", license.EndTime);
            }
        }
    }
}