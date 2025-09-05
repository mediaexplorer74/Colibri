using VkLib;

namespace Colibri.Services
{
    public class ServiceLocator
    {
        private const string VKAPP_ID = "2274003";
        private const string VKAPP_SECRET = "hHbZxrka2uZ6jB1inYsH";

        public static Vk Vkontakte { get; } = new Vk(
            VKAPP_ID,
            VKAPP_SECRET,
            "5.116",
            "VKAndroidApp/5.52-4543 (Android 5.1.1; SDK 22; x86_64; unknown Android SDK built for x86_64; en; 320x240)");

        public static LongPollHandler LongPollHandler { get; } = new LongPollHandler(Vkontakte);

        public static UserService UserService { get; } = new UserService(Vkontakte);

        public static ExceptionHandler ExceptionHandler { get; } = new ExceptionHandler();

        public static AudioService AudioService { get; } = new AudioService();

        public static UserPresenceService UserPresenceService { get; } = new UserPresenceService(Vkontakte);
    }
}