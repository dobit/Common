using System;
using Microsoft.Win32;

namespace LFNet.Licensing
{
    public class RegistryLicenseStorer : ILicenseStorer
    {
        private RegistryKey baseKey;
        private string subKey;
        private string keyName;
        private string oldKeyName;
        public RegistryLicenseStorer(RegistryKey baseKey, string subKey, string keyName, string oldKeyName)
        {
            this.baseKey = baseKey;
            this.subKey = subKey;
            this.keyName = keyName;
            this.oldKeyName = oldKeyName;
        }
        public virtual void SaveKey(string key)
        {
            try
            {
                RegistryKey registryKey = this.baseKey;
                RegistryKey registryKey2 = registryKey.OpenSubKey(this.subKey, true);
                if (registryKey2 == null)
                {
                    registryKey2 = registryKey.CreateSubKey(this.subKey);
                }
                registryKey2.SetValue(this.keyName, key);
            }
            catch (Exception innerException)
            {
                throw new Exception(string.Format("Cannot store license key into registry: '{0}\\{1}'.", this.baseKey.Name, this.subKey), innerException);
            }
        }
        public virtual string LoadKey()
        {
            return this.DoLoadKey(this.keyName);
        }
        public string LoadOldKey()
        {
            return this.DoLoadKey(this.oldKeyName);
        }
        private string DoLoadKey(string name)
        {
            string result;
            try
            {
                RegistryKey registryKey = this.baseKey;
                RegistryKey registryKey2 = registryKey.OpenSubKey(this.subKey);
                if (registryKey2 == null)
                {
                    result = null;
                }
                else
                {
                    result = (string)registryKey2.GetValue(name);
                }
            }
            catch (Exception innerException)
            {
                throw new Exception(string.Format("Cannot read license key from registry: '{0}\\{1}\\{2}'.", this.baseKey.Name, this.subKey, name), innerException);
            }
            return result;
        }
    }
}