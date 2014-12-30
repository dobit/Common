using LFNet.Common.Extensions;

namespace LFNet.Common.Security
{
    public class SaltedHashBuilder : HashBuilder
    {
        public SaltedHashBuilder(string salt)
        {
            if (!salt.IsNullOrEmpty())
            {
                base.Writer.Write(salt);
            }
        }
    }
}

