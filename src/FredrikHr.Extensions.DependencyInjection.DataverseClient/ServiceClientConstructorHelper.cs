using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

internal static class ServiceClientConstructorHelper
{
    internal static ServiceClient CreateServiceClient(
        IOrganizationServiceAsync orgSvc,
        HttpClient httpClient,
        string baseConnectUrl,
        Version? targetVersion = null,
        ILogger? logger = null
        )
    {
        var client = ServiceClientCtor(
            orgSvc,
            httpClient,
            baseConnectUrl,
            targetVersion,
            logger
            );
        var connectionService = GetConnectionService(client);
        SetConnectionServiceActiveAuthenticationType(
            connectionService,
            (Microsoft.PowerPlatform.Dataverse.Client.AuthenticationType)(-2)
            );
        SetConnectionServiceOrganizationServiceClient(connectionService, orgSvc);
        return client;
    }

#if NET8_0_OR_GREATER
    [System.Runtime.CompilerServices.UnsafeAccessor(
        System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor
        )]
    private extern static ServiceClient ServiceClientCtor(
        IOrganizationService orgSvc,
        HttpClient httpClient,
        string baseConnectUrl,
        Version? targetVersion = null,
        ILogger? logger = null
        );

    [System.Runtime.CompilerServices.UnsafeAccessor(
        System.Runtime.CompilerServices.UnsafeAccessorKind.Field,
        Name = "_connectionSvc"
        )]
    private extern static object GetConnectionService(ServiceClient serviceClient);
#else
    private static readonly Type[] ServiceClientCtorParamTypes = [
            typeof(IOrganizationService),
            typeof(HttpClient),
            typeof(string),
            typeof(Version),
            typeof(ILogger)
            ];
    private static readonly Lazy<ConstructorInfo> ServiceClientCtorInfo =
        new(() => typeof(ServiceClient).GetConstructors(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
            ).Single(static ctorInfo => ctorInfo.GetParameters()
                .Select(paramInfo => paramInfo.ParameterType)
                .SequenceEqual(ServiceClientCtorParamTypes)
            ));

    private static ServiceClient ServiceClientCtor(
        IOrganizationService orgSvc,
        HttpClient httpClient,
        string baseConnectUrl,
        Version? targetVersion = null,
        ILogger? logger = null
        )
    {
        try
        {
            return (ServiceClient)ServiceClientCtorInfo.Value.Invoke(
                [orgSvc, httpClient, baseConnectUrl, targetVersion, logger]
                );
        }
        catch (TargetInvocationException invokeExcept)
        when (invokeExcept.InnerException is Exception except)
        {
            throw except;
        }
    }

    private static readonly Lazy<FieldInfo> ServiceClientConnectionServiceField =
        new(() => typeof(ServiceClient).GetField(
            "_connectionSvc",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
            ));

    private static object GetConnectionService(ServiceClient serviceClient)
    {
        var fieldInfo = ServiceClientConnectionServiceField.Value;
        return fieldInfo.GetValue(serviceClient);
    }
#endif

    private static readonly Type? ConnectionServiceType = Type.GetType(
        "Microsoft.PowerPlatform.Dataverse.Client.ConnectionService, " +
        typeof(ServiceClient).Assembly.FullName,
        throwOnError: false
        );
    private static readonly Lazy<FieldInfo?> ConnectionServiceActiveAuthenticationTypeField =
        new(() => ConnectionServiceType?.GetField(
            "_eAuthType",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
            ));
    private static readonly Lazy<FieldInfo?> ConnectionServiceOrganizationServiceClientField =
        new(() => ConnectionServiceType?.GetField(
            "_svcWebClientProxy",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
            ));

    private static readonly Type? OrganizationServiceClientType = Type.GetType(
        "Microsoft.PowerPlatform.Dataverse.Client.Connector.OrganizationWebProxyClientAsync" +
        ", " +
        typeof(ServiceClient).Assembly.FullName,
        throwOnError: false
        );

    private static void SetConnectionServiceActiveAuthenticationType(
        object connectionService,
        Microsoft.PowerPlatform.Dataverse.Client.AuthenticationType authenticationType
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(connectionService);
#else
        _ = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
#endif

        Type connectionServiceType = connectionService.GetType();
        connectionServiceType = ConnectionServiceType?.IsAssignableFrom(connectionServiceType) ?? true
            ? ConnectionServiceType ?? connectionServiceType
            : throw new ArgumentException(
                message: $"Argument must be an instance of {ConnectionServiceType}.",
                paramName: nameof(connectionService)
                );

        if (ConnectionServiceActiveAuthenticationTypeField.Value is FieldInfo fieldInfo)
        {
            fieldInfo.SetValue(connectionService, authenticationType);
            return;
        }

        connectionServiceType.InvokeMember(
            "_eAuthType",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.SetField,
            Type.DefaultBinder,
            connectionService,
            [authenticationType],
            System.Globalization.CultureInfo.InvariantCulture
            );
    }

    private static void SetConnectionServiceOrganizationServiceClient(
        object connectionService,
        IOrganizationServiceAsync organizationService
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(connectionService);
        ArgumentNullException.ThrowIfNull(organizationService);
#else
        _ = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        _ = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
#endif

        Type connectionServiceType = connectionService.GetType();
        connectionServiceType = ConnectionServiceType?.IsAssignableFrom(connectionServiceType) ?? true
            ? ConnectionServiceType ?? connectionServiceType
            : throw new ArgumentException(
                message: $"Argument must be an instance of {ConnectionServiceType}.",
                paramName: nameof(connectionService)
                );
        if (!(OrganizationServiceClientType?.IsAssignableFrom(organizationService.GetType()) ?? true))
        {
            throw new ArgumentException(
                message: $"Argument must be an instance of {OrganizationServiceClientType}.",
                paramName: nameof(organizationService)
                );
        }

        if (ConnectionServiceOrganizationServiceClientField.Value is FieldInfo fieldInfo)
        {
            fieldInfo.SetValue(connectionService, organizationService);
            return;
        }

        connectionServiceType.InvokeMember(
            "_svcWebClientProxy",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.SetField,
            Type.DefaultBinder,
            connectionService,
            [organizationService],
            System.Globalization.CultureInfo.InvariantCulture
            );
    }
}