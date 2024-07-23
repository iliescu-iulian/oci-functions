using System;
using System.Collections.Generic;
using System.IO;
using Fnproject.Fn.Fdk;

using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using Oci.Common.Auth;
using Oci.Common.Model;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Requests;
using Oci.ObjectstorageService.Models;

[assembly:InternalsVisibleTo("Function.Tests")]
namespace Function {
	class BucketList
    {

        private ResourcePrincipalAuthenticationDetailsProvider _provider;
        private ObjectStorageClient _client;
        private string _namespace;
        private string _compartmentId;
        private NLog.Logger _logger;
        private Secrets _secrets;

        public BucketList()
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Info, LogLevel.Fatal, 
                new NLog.Targets.ConsoleTarget("consolelog"));
            NLog.LogManager.Configuration = config;
            _logger = NLog.LogManager.GetCurrentClassLogger();
			_provider= ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
            _client = new ObjectStorageClient(_provider);
            _namespace = Environment.GetEnvironmentVariable("NAMESPACE");
            _compartmentId= Environment.GetEnvironmentVariable("COMPARTMENT");
            _secrets = new Secrets(_provider);
            Console.WriteLine("BucketList.Constructor: Key='{0}', Secrets='{1}'",
                _secrets.ConsumerKey, _secrets.ConsumerSecret);
        }

        public string handleRequest(string eventData)
        {
            _logger.Info($"BucketList handle request with input data {eventData} started");
            if (_client == null)
            {
                _logger.Error("Error creating ObjectStorage client");
                return "There was an error creating ObjectStorage client";
            }

            if (string.IsNullOrEmpty(eventData))
            {
                Console.WriteLine($"Listing buckets in compartment {_compartmentId}");
                _logger.Info($"Listing buckets in compartment {_compartmentId}");
                return GetBucketList();
            }

            //Console.WriteLine($"Invoked for bucket '{eventData}'");

            try
            {
                return GetBucketContent(eventData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: {0}\n{1}", ex.Message, ex.StackTrace);
                return ex.Message;
            }
        }

        private string GetBucketList()
        {
            try
            {
                var req = new ListBucketsRequest
                {
                    NamespaceName = _namespace,
                    CompartmentId = _compartmentId
                };
                var resp = _client.ListBuckets(req).Result;

                var result = new StringBuilder();
                foreach (var bucket in resp.Items)
                {
                    result.AppendLine($"{bucket.Name}");
                }

                _logger.Info($"Bucket list: {result}");
                return result.ToString();
            }
            catch (Exception ex)
            {
                var e = $"Failed retrieving bucket list for namespace '{_namespace}' and compartment '{_compartmentId}': {ex}";
                _logger.Error(e);
                return e;
            }
        }

        private string HandleObjectStorageEvent(EventData data)
        {
            var retry = new ResourceRetry(data);
            if (retry.ShouldRetry && retry.RetryIndex < 5)
            {
                // rename file
                _logger.Warn($"Retry file {retry.ResourceName}");
                using(var emptyStream= new MemoryStream())
                {
                    var req = new PutObjectRequest
                    {
                        NamespaceName = _namespace,
                        BucketName = data.BucketName,
                        ObjectName = $"failures/{retry.ResourceName}.{retry.RetryIndex + 1}",
                        ContentLength = 0,
                        PutObjectBody = emptyStream
                    };
                    var resp = _client.PutObject(req).Result;

                    Console.WriteLine("Upload response ETag: {0}", resp.ETag);
                }

                if (retry.IsInFolder)
                {
                    // delete current one
                    var req = new DeleteObjectRequest
                    {
                        NamespaceName = _namespace,
                        BucketName = data.BucketName,
                        ObjectName = data.ResourceName
                    };
                    var resp= _client.DeleteObject(req).Result;
                }
                return $"Object uploaded to {retry.ResourceName}.{retry.RetryIndex + 1}";
            }
            else
            {
                var req = new GetObjectRequest
                {
                    NamespaceName = _namespace,
                    BucketName = data.BucketName,
                    ObjectName = data.ResourceName
                };
                var resp= _client.GetObject(req).Result;
                Console.WriteLine("Object download: ContentType= {0}, ContentLength= {1}, ETag= {2}", resp.ContentType, resp.ContentLength, resp.ETag);
                return new StreamReader(resp.InputStream).ReadToEnd();
                //var reader = new StreamReader(resp.InputStream);
                //reader.ReadToEnd();
            }
        }

        private string GetBucketContent(string eventData)
        {
            var data = new EventData(eventData);
            string bucketName = data.BucketName ?? eventData;

            if (!string.IsNullOrEmpty(data.BucketName))
            {
                return HandleObjectStorageEvent(data);
            }

            try
            {
                var req = new ListObjectsRequest
                {
                    NamespaceName = _namespace,
                    BucketName = bucketName
                };

                var resp = _client.ListObjects(req).Result;
                _logger.Info($"ListObject response: {resp}");
                var result = new StringBuilder();
                foreach (var obj in resp.ListObjects.Objects)
                {
                    result.AppendLine($"{bucketName}: {obj.Name}");
                }

                _logger.Info($"BucketList handle request for bucket {bucketName} completed with result[{result}]");

                return result.ToString();
            }
            catch (OciException e)
            {
                _logger.Error($"Operation failed: {e}");
                return $"Operation failed with error: {e.Message}";
            }
            catch (Exception e)
            {
                _logger.Error($"Error detected: {e}");
                return $"Unexpected error: {e.Message}";
            }
        }

        static void Main(string[] args) { Fdk.Handle(args[0]); }
	}
}
