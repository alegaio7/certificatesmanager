using Microsoft.Extensions.Logging;
using Moq;

namespace CertificateManager.Tests
{
    public class FixtureCommon<TController>
    {
        public FixtureCommon()
        {
            // Mock logger
            Logger = new Mock<ILogger<TController>>().Object;
        }

        public ILogger<TController> Logger { get; private set; }
    }
}