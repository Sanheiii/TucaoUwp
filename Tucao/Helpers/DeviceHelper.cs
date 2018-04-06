namespace Tucao.Helpers
{
    class DeviceHelper
    {
        public static bool IsMobile => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
    }
}
