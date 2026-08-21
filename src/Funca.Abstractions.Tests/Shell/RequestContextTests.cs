using Funca.Abstractions.Shell;

namespace Funca.Abstractions.Tests.Shell;

public sealed class RequestContextTests
{
    [Fact]
    public void Attach_replaces_a_value_and_Detach_returns_none_for_a_different_type()
    {
        var context = new RequestContext();
        context.Attach("key", "first");
        context.Attach("key", "second");

        Assert.Equal("second", context.Detach<string>("key").Unwrap());
        Assert.True(context.Detach<UserContext>("key").IsNone);
    }
}
