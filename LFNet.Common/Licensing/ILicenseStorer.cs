using System;

namespace LFNet.Licensing
{
    public interface ILicenseStorer
    {
        string LoadKey();
        string LoadOldKey();
        void SaveKey(string key);
    }
#if NEEDTODO
    internal class LicenseExpirationHandler
    {
        private const int LimitInDays = 7;
        private const int CheckInterval = 43200000;
        private readonly IVisualStudio visualStudio;
        private readonly IProtector protector;
        private Timer checkingTimer;
        public LicenseExpirationHandler(IVisualStudio visualStudio, IProtector protector)
        {
            this.visualStudio = visualStudio;
            this.protector = protector;
        }
        public void StartMonitoring()
        {
            this.checkingTimer = new Timer();
            this.checkingTimer.Interval = CheckInterval;
            this.checkingTimer.Tick += new EventHandler(this.OnCheckTimerTick);
            this.checkingTimer.Start();
            this.CheckLicenseExpiration();
        }
        public void StopMonitoring()
        {
            this.checkingTimer.Tick -= new EventHandler(this.OnCheckTimerTick);
            this.checkingTimer.Stop();
            this.checkingTimer.Dispose();
            this.checkingTimer = null;
            this.visualStudio.SetStatusBarInfoIcon(false);
        }
        private void CheckLicenseExpiration()
        {
            License currentLicense = this.protector.GetCurrentLicense();
            TimeSpan timeSpan = currentLicense.EndTime - DateTime.Now;
            this.visualStudio.SetStatusBarInfoIcon(timeSpan.Days <= 7);
        }
        private void OnCheckTimerTick(object sender, EventArgs e)
        {
            this.CheckLicenseExpiration();
        }
    }
#endif
}