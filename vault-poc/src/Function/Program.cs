using Fnproject.Fn.Fdk;
using NLog;
using Oci.Common.Auth;
using System;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("Function.Tests")]
namespace Function
{
    class Vault
    {

        private ResourcePrincipalAuthenticationDetailsProvider _provider;
        private string _namespace;
        private string _compartmentId;
        private NLog.Logger _logger;
        private Secrets _secrets;

        public Vault()
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Info, LogLevel.Fatal, 
                new NLog.Targets.ConsoleTarget("consolelog"));
            NLog.LogManager.Configuration = config;
            _logger = NLog.LogManager.GetCurrentClassLogger();
			_provider= ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
            _namespace = Environment.GetEnvironmentVariable("NAMESPACE");
            _compartmentId= Environment.GetEnvironmentVariable("COMPARTMENT");
            try
            {
                _secrets = new Secrets(_provider);
                Console.WriteLine("Vault.Constructor: Key='{0}', Secrets='{1}'",
                    _secrets.ConsumerKey, _secrets.ConsumerSecret);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                Console.WriteLine("Secrets failure: {0} / {1}", e.Message, e.StackTrace);
            }
        }

        public string handleRequest(string eventData)
        {
            if(_secrets != null)
            {
                _logger.Info($"Secrets Key={_secrets.ConsumerKey} Secret={_secrets.ConsumerSecret}");
                return "Secrets Key={_secrets.ConsumerKey} Secret={_secrets.ConsumerSecret}";
            }
            _logger.Warn("Secret instance not available");
            return "Secret instance not available";
        }

        

        static void Main(string[] args) { Fdk.Handle(args[0]); }
	}
}
