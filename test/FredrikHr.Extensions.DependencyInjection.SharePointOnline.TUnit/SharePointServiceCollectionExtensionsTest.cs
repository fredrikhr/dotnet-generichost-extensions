using Microsoft.Extensions.DependencyInjection;

namespace FredrikHr.Extensions.DependencyInjection.SharePointOnline.TUnit;

public class SharePointServiceCollectionExtensionsTest
{
    [Test]
    public async Task AddSharePointThrowsIfServiceCollectionNull()
    {
        ServiceCollection services = null!;

        await Assert
            .That(() => _ = services)
            .Throws<ArgumentNullException>();
    }
}
