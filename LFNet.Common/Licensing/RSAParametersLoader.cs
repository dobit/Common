using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace LFNet.Licensing
{
    public class RSAParametersLoader
    {
        public const string CSP_CONTAINER_NAME = "OSTY";
        public static RSAParameters LoadFromResource(Assembly assembly, string resourceName, string password)
        {
            string name = assembly.GetName().Name;
            Stream manifestResourceStream = assembly.GetManifestResourceStream(name + "." + resourceName);
            return RSAParametersLoader.DeserializeRSAKeyFromStream(manifestResourceStream, password);
        }
        public static RSAParameters LoadFromFile(string fileName, string password)
        {
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            return RSAParametersLoader.DeserializeRSAKeyFromStream(stream, password);
        }
        private static RSAParameters DeserializeRSAKeyFromStream(Stream stream, string password)
        {
            RSAParameters rSAParameters = default(RSAParameters);
            RSAParameters result;
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                rSAParameters.Exponent = (byte[])binaryFormatter.Deserialize(stream);
                rSAParameters.Modulus = (byte[])binaryFormatter.Deserialize(stream);
                if (password != null)
                {
                    long num = stream.Length - stream.Position;
                    byte[] array = new byte[num];
                    num = (long)stream.Read(array, 0, (int)num);
                    PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, null);
                    passwordDeriveBytes.HashName = "SHA256";
                    Rijndael rijndael = new RijndaelManaged();
                    rijndael.KeySize = 256;
                    rijndael.Key = passwordDeriveBytes.GetBytes(32);
                    rijndael.IV = new byte[rijndael.BlockSize / 8];
                    ICryptoTransform cryptoTransform = rijndael.CreateDecryptor();
                    array = cryptoTransform.TransformFinalBlock(array, 0, (int)num);
                    rijndael.Clear();
                    MemoryStream memoryStream = new MemoryStream(array, false);
                    rSAParameters.P = (byte[])binaryFormatter.Deserialize(memoryStream);
                    rSAParameters.Q = (byte[])binaryFormatter.Deserialize(memoryStream);
                    rSAParameters.D = (byte[])binaryFormatter.Deserialize(memoryStream);
                    rSAParameters.InverseQ = (byte[])binaryFormatter.Deserialize(memoryStream);
                    rSAParameters.DP = (byte[])binaryFormatter.Deserialize(memoryStream);
                    rSAParameters.DQ = (byte[])binaryFormatter.Deserialize(memoryStream);
                    memoryStream.Close();
                }
                stream.Close();
                result = rSAParameters;
            }
            finally
            {
                if (stream != null)
                {
                    ((IDisposable)stream).Dispose();
                }
            }
            return result;
        }
    }

    public class Protector : IProtector
    {
        private ILicenseStorer publicStorer;
        private ILicenseStorer privateStorer;
        private IDecoder decoder;
        private bool licenseIsCached;
        private License cachedLicense;
        private bool cachedIsRegistered;
        public event RegistrationChangedEventHandler RegistrationChanged;
        public Protector(ILicenseStorer privateStorer, ILicenseStorer publicStorer, IDecoder decoder)
        {
            this.privateStorer = privateStorer;
            this.publicStorer = publicStorer;
            this.decoder = decoder;
        }
        private License LoadOldLicense()
        {
            License result = null;
            try
            {
                string text = this.privateStorer.LoadOldKey();
                if (text != null && text.Length > 0)
                {
                    result = LicenseConverter.KeyToLicense(this.decoder, text);
                }
                else
                {
                    text = this.publicStorer.LoadOldKey();
                    result = LicenseConverter.KeyToLicense(this.decoder, text);
                }
            }
            catch (Exception ex)
            {
                Log.ReportException(ex);
            }
            return result;
        }
        private License LoadLicense()
        {
            License license = null;
            try
            {
                string text = this.privateStorer.LoadKey();
                if (text != null && text.Length > 0)
                {
                    license = LicenseConverter.KeyToLicense(this.decoder, text);
                }
                else
                {
                    text = this.publicStorer.LoadKey();
                    license = LicenseConverter.KeyToLicense(this.decoder, text);
                }
            }
            catch (Exception ex)
            {
                Log.ReportException(ex);
            }
            if (license != null && LicenseVerificator.IsCorrect(license) && LicenseVerificator.IsPregenerated(license))
            {
                License license2 = this.LoadOldLicense();
                if (license2 != null && LicenseVerificator.IsCorrect(license2) && !LicenseVerificator.IsPregenerated(license2))
                {
                    if (LicenseVerificator.IsOutdatedForVersion2x(license2))
                    {
                        license2.UpgradeEvaluation = true;
                        license2.StartTime = license.StartTime;
                        license2.EndTime = license.EndTime;
                        license = license2;
                    }
                    else
                    {
                        license = license2;
                    }
                }
            }
            else
            {
                if (license == null)
                {
                    License license3 = this.LoadOldLicense();
                    if (license3 != null && LicenseVerificator.IsCorrect(license3))
                    {
                        license = license3;
                    }
                }
            }
            return license;
        }
        private void CacheLicense(License license)
        {
            this.cachedLicense = license;
            try
            {
                this.cachedIsRegistered = LicenseVerificator.IsValid(license, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Log.ReportException(ex);
                this.cachedIsRegistered = false;
            }
            this.licenseIsCached = true;
        }
        private void EnsureLicenseCached()
        {
            if (!this.licenseIsCached)
            {
                this.CacheLicense(this.LoadLicense());
            }
        }
        public bool IsRegistered()
        {
            this.EnsureLicenseCached();
            return this.cachedIsRegistered;
        }
        public License GetCurrentLicense()
        {
            this.EnsureLicenseCached();
            return this.cachedLicense;
        }
        public bool IsValidKey(string key)
        {
            bool result;
            try
            {
                key = KeyStringFormatter.ParseKey(key);
                License license = LicenseConverter.KeyToLicense(this.decoder, key);
                result = LicenseVerificator.IsValid(license, DateTime.UtcNow);
            }
            catch (LicensingException)
            {
                result = false;
            }
            return result;
        }
        public License ParseKey(string key)
        {
            key = KeyStringFormatter.ParseKey(key);
            return LicenseConverter.KeyToLicense(this.decoder, key);
        }
        public bool RegisterKey(string key)
        {
            key = KeyStringFormatter.ParseKey(key);
            License license = LicenseConverter.KeyToLicense(this.decoder, key);
            if (!LicenseVerificator.IsValid(license, DateTime.UtcNow))
            {
                return false;
            }
            this.privateStorer.SaveKey(key);
            this.CacheLicense(license);
            if (this.RegistrationChanged != null)
            {
                this.RegistrationChanged(this);
            }
            return true;
        }
    }
}