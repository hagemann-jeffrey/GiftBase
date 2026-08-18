using Microsoft.AspNetCore.Components;

namespace GiftBase.Tests.Helper;

public class TestNavigationManager : NavigationManager
{
    public TestNavigationManager(string baseUri = "http://localhost/")
    {
        Initialize(baseUri, baseUri);
    }

    protected override void NavigateToCore(string uri, bool forceLoad) { }
}
