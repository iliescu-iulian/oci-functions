using Fnproject.Fn.Fdk;
using NLog;
using Oci.Common;
using Oci.Common.Auth;
using Oci.SecretsService;
using Oci.SecretsService.Models;
using Oci.SecretsService.Requests;
using System;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("Function.Tests")]
namespace Function
{
    class Vault
    {

        private ResourcePrincipalAuthenticationDetailsProvider _provider;
        private SecretsClient _secretsClient;
        private NLog.Logger _logger;

        public Vault()
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Info, LogLevel.Fatal, 
                new NLog.Targets.ConsoleTarget("consolelog"));
            NLog.LogManager.Configuration = config;
            _logger = NLog.LogManager.GetCurrentClassLogger();
			_provider= ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
            _secretsClient = new SecretsClient(_provider);
            
        }

        public string handleRequest(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    var secret = GetSecret(Environment.GetEnvironmentVariable("CDI_CONSUMERSECRET_OCID"));
                    var key = GetSecret(Environment.GetEnvironmentVariable("CDI_CONSUMERKEY_OCID"));
                    _logger.Info($"Secrets Key={key} Secret={secret}");
                    return key;
                }

                var value = GetSecret(id);
                _logger.Info($"Secret ID={id} value={value}");
                return value;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return e.Message;
            }
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


        static void Main(string[] args) { Fdk.Handle(args[0]); }
	}
}
