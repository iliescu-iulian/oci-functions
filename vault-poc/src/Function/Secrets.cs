using Oci.Common.Auth;
using Oci.SecretsService;
using System;
using Oci.Common;
using Oci.SecretsService.Requests;
using Oci.SecretsService.Models;

namespace Function;

public class Secrets
{
    private SecretsClient _secretsClient;
    
    public string ConsumerSecret { get; }
    public string ConsumerKey { get; }
    //public string 
    public Secrets(IBasicAuthenticationDetailsProvider provider)
    {
        _secretsClient = new SecretsClient(provider, new ClientConfiguration());
        ConsumerSecret = GetSecret(Environment.GetEnvironmentVariable("CDI_CONSUMERSECRET_OCID"));
        ConsumerKey = GetSecret(Environment.GetEnvironmentVariable("CDI_CONSUMERKEY_OCID"));
    }

    private string GetSecret(string ocid)
    {
        var req = new GetSecretBundleRequest
        {
            SecretId = ocid
        };
        var resp = _secretsClient.GetSecretBundle(req).Result;
        Base64SecretBundleContentDetails secretValue = resp.SecretBundle.SecretBundleContent as Base64SecretBundleContentDetails;
        return secretValue?.Content;
    }
}