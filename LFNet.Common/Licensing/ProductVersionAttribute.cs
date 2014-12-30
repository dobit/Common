using System;

namespace LFNet.Licensing
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class ProductVersionAttribute : Attribute
    {
        private string version;
        public string Version
        {
            get
            {
                return this.version;
            }
        }
        public ProductVersionAttribute(string version)
        {
            this.version = version;
        }
    }
}