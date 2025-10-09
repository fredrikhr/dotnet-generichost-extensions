using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Options.Tests;

public class InheritedOptionsTest
{
    public abstract class OptionsBase
    {
        public int IntOption { get; set; }
    }

    public class OptionsDerived : OptionsBase
    {
        public required string StringOption { get; set; }
    }

    [Fact]
    public void ConfigureInheritAll_inherits_configures_from_base()
    {
        const int expectedIntValue = 42;
        bool hasRun = false;

        ServiceCollection services = new();

        services.ConfigureAll<OptionsBase>(optsBase =>
        {
            hasRun = true;
            optsBase.IntOption = expectedIntValue;
        });
        services.ConfigureInheritAll<OptionsDerived, OptionsBase>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        var optsDerived = serviceProvider.GetRequiredService
            <IOptions<OptionsDerived>>().Value;

        Assert.Equal(expectedIntValue, optsDerived.IntOption);
        Assert.True(hasRun);
    }

    [Fact]
    public void ConfigureInheritAll_called_multiple_times_does_not_register_new_service_descriptors()
    {
        ServiceCollection services = new();

        services.ConfigureInheritAll<OptionsDerived, OptionsBase>();

        int descCount = services.Count;

        services.ConfigureInheritAll<OptionsDerived, OptionsBase>();

        Assert.Equal(descCount, services.Count);
    }

    [Fact]
    public void ConfigureInherit_default_named_inherits_configures_from_base()
    {
        const int expectedIntValue = 42;
        bool hasRun = false;

        ServiceCollection services = new();

        services.Configure<OptionsBase>(optsBase =>
        {
            hasRun = true;
            optsBase.IntOption = expectedIntValue;
        });
        services.ConfigureInherit<OptionsDerived, OptionsBase>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        var optsDerived = serviceProvider.GetRequiredService
            <IOptions<OptionsDerived>>().Value;

        Assert.Equal(expectedIntValue, optsDerived.IntOption);
        Assert.True(hasRun);
    }

    [Fact]
    public void ConfigureInherit_default_named_called_multiple_times_does_not_register_new_service_descriptors()
    {
        ServiceCollection services = new();

        services.ConfigureInherit<OptionsDerived, OptionsBase>();

        int descCount = services.Count;

        services.ConfigureInherit<OptionsDerived, OptionsBase>();

        Assert.Equal(descCount, services.Count);
    }

    [Fact]
    public void PostConfigureInheritAll_inherits_postconfigures_from_base()
    {
        const int expectedIntValue = 42;
        bool hasRun = false;

        ServiceCollection services = new();

        services.PostConfigureAll<OptionsBase>(optsBase =>
        {
            hasRun = true;
            optsBase.IntOption = expectedIntValue;
        });
        services.PostConfigureInheritAll<OptionsDerived, OptionsBase>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        var optsDerived = serviceProvider.GetRequiredService
            <IOptions<OptionsDerived>>().Value;

        Assert.Equal(expectedIntValue, optsDerived.IntOption);
        Assert.True(hasRun);
    }

    [Fact]
    public void PostConfigureInheritAll_called_multiple_times_does_not_register_new_service_descriptors()
    {
        ServiceCollection services = new();

        services.PostConfigureInheritAll<OptionsDerived, OptionsBase>();

        int descCount = services.Count;

        services.PostConfigureInheritAll<OptionsDerived, OptionsBase>();

        Assert.Equal(descCount, services.Count);
    }

    [Fact]
    public void PostConfigureInherit_default_named_inherits_postconfigures_from_base()
    {
        const int expectedIntValue = 42;
        bool hasRun = false;

        ServiceCollection services = new();

        services.PostConfigure<OptionsBase>(optsBase =>
        {
            hasRun = true;
            optsBase.IntOption = expectedIntValue;
        });
        services.PostConfigureInherit<OptionsDerived, OptionsBase>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        var optsDerived = serviceProvider.GetRequiredService
            <IOptions<OptionsDerived>>().Value;

        Assert.Equal(expectedIntValue, optsDerived.IntOption);
        Assert.True(hasRun);
    }

    [Fact]
    public void PostConfigureInherit_default_named_called_multiple_times_does_not_register_new_service_descriptors()
    {
        ServiceCollection services = new();

        services.PostConfigureInherit<OptionsDerived, OptionsBase>();

        int descCount = services.Count;

        services.PostConfigureInherit<OptionsDerived, OptionsBase>();

        Assert.Equal(descCount, services.Count);
    }
}
