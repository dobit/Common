using System;

namespace LFNet.Licensing
{
    public class LicensingException : Exception
    {
        public LicensingException(string message, Exception inner)
            : base(message, inner)
        {
        }
        public LicensingException(string message)
            : base(message)
        {
        }
    }
}