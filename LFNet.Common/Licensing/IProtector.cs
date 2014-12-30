using LFNet.Licensing;

namespace LFNet.Licensing
{
    public delegate void RegistrationChangedEventHandler(IProtector sender);

    public interface IProtector
    {
        event RegistrationChangedEventHandler RegistrationChanged;
        License GetCurrentLicense();
        bool IsRegistered();
        bool RegisterKey(string key);
        bool IsValidKey(string key);
        License ParseKey(string key);
    }
}