using System;
using NUnit.Framework;
using WellcareGenerationUITest;
using Xamarin.UITest;

namespace UITestExercise
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        private IApp _app;
        private readonly Platform _platform;

        public Tests(Platform platform)
        {
            _platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            _app = AppInitializer.StartApp(_platform);
        }

        [Test]
        public void AppLaunches()
        {
            _app.Screenshot("First screen.");
        }

        [Test]
        public void CompareResult()
        {
            _app.Tap(x => x.Button("ButtonGetResult"));
            _app.Screenshot("Tap Button ButtonGetResult");

            _app.WaitFor(() => true, timeoutMessage: "Waiting for button ButtonGetResult",
                timeout: TimeSpan.FromSeconds(10));
            _app.Screenshot("Result");
        }

        [Test]
        public void CompareResultWithMultiPoint()
        {
            _app.Tap(x => x.Button("ButtonGetResultWithMultiPoint"));
            _app.Screenshot("Tap Button ButtonGetResultWithMultiPoint");

            _app.WaitFor(() => true, timeoutMessage: "Waiting for button ButtonGetResultWithMultiPoint",
                timeout: TimeSpan.FromMinutes(2));
            _app.Screenshot("Result");
        }
    }
}

