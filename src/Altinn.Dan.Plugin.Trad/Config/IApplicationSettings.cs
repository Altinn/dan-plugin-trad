using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using System;

namespace Altinn.Dan.Plugin.Trad.Config
{
    public interface IApplicationSettings
    {
        string RedisConnectionString { get; }
        TimeSpan Breaker_RetryWaitTime { get; }
        TimeSpan Breaker_OpenCircuitTime { get; }
        bool IsTest { get; }
        string RegistryURL { get; }
        CertificateClient certificateClient { get; }
        SecretClient secretClient { get; }
        string KeyVaultSslCertificate { get; }
        string KeyVaultName { get; }
        string ApiKeySecret { get; }
    }
}
