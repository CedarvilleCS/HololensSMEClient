using ARTAPclient;
using Moq;
using Xunit;

namespace WpfApplication1.Unit_Tests
{
    public class ScreenshotAnnotationsWindowTests
    {
        private ScreenshotAnnotationsWindow _screenshotAnnotationsWindow;
        private Mock<VideoStreamWindow> _videoStreamWindow;
        private Mock<AsynchronousSocketListener> _listener;

        public ScreenshotAnnotationsWindowTests()
        {
            _videoStreamWindow = new Mock<VideoStreamWindow>();
            _listener = new Mock<AsynchronousSocketListener>();

            _screenshotAnnotationsWindow = new ScreenshotAnnotationsWindow(_videoStreamWindow.Object, _listener.Object);

            _screenshotAnnotationsWindow.Setup(x => x.CheckMarkerPlacementWrapper).Returns();
        }

        #region SelectThumbnail

        [Fact]
        public void Should_NotThrowException()
        {
            _screenshotAnnotationsWindow.SelectThumbnail(0);
        }

        [Fact]
        public void Should_ThrowException()
        {
            _screenshotAnnotationsWindow.SelectThumbnail(-1);
        }

        #endregion
    }
}
