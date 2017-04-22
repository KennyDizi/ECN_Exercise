using System;
using Xamarin.UITest;
using Xamarin.UITest.Utils;

namespace WellcareGenerationUITest
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp
                    .Android
                    //.ApkFile("mHealthAndroidReleasePackage.apk")
                    .EnableLocalScreenshots()
                    .WaitTimes(new WaitTimes())
                    .StartApp();
            }

            return ConfigureApp
                .iOS
                //.AppBundle("mHealthiOSReleasePackage.app")
                .WaitTimes(new WaitTimes()).EnableLocalScreenshots()
                .StartApp();
        }
    }

    /// <summary>
    /// Custom implementation of IWaitTimes in order to avoid test failures due to slow emulators.
    /// </summary>
    internal class WaitTimes : IWaitTimes
    {
        public TimeSpan GestureCompletionTimeout => TimeSpan.FromMinutes(1);

        public TimeSpan GestureWaitTimeout => TimeSpan.FromMinutes(1);

        public TimeSpan WaitForTimeout => TimeSpan.FromMinutes(1);
    }
}