using System;

namespace LFNet.Licensing
{
    public class License
    {
        public byte Version;
        public Guid LicenseId;
        public string PurchaseId;
        public LicenseType Type;
        public LicenseBinding Binding;
        public string LicensedTo;
        public int Capacity;
        public DateTime StartTime;
        public DateTime EndTime;
        public DateTime PurchaseDate;
        public bool UpgradeEvaluation;
        public bool IsPregenerated;
        public License()
        {
            this.Version = 2;
            this.LicenseId = Guid.NewGuid();
            this.UpgradeEvaluation = false;
            this.IsPregenerated = false;
        }
    }
}