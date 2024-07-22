using System.Text.Json;
using NUnit.Framework;

namespace Function.Tests;

[TestFixture]
public class ResourceRetryTest
{
    private static EventData CreateEventData(string resourceName)
    {
        var data= new EventSource
        {
            EventType = "N/A",
            Source = "N/A",
            Data = new EventSourceData
            {
                
                ResourceName = resourceName,
                AdditionalDetails = new BucketData
                {
                    BucketName = "N/A"
                }
            }
        };
        return new EventData(JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy= JsonNamingPolicy.CamelCase
        }));
    }
    [Test]
    public void NonRetryResourceDoesNotSetIndex()
    {
        string resourceName = "normal.txt";
        var instance = new ResourceRetry(CreateEventData(resourceName));
        Assert.AreEqual(0, instance.RetryIndex);
        Assert.AreEqual(resourceName, instance.ResourceName);
        Assert.IsFalse(instance.ShouldRetry);
    }

    [Test]
    public void RetryResourceFirstTimeSetShouldRetry()
    {
        string resourceName = "retry-me.txt";
        var instance = new ResourceRetry(CreateEventData(resourceName));
        Assert.AreEqual(0, instance.RetryIndex);
        Assert.AreEqual(resourceName, instance.ResourceName);
        Assert.IsTrue(instance.ShouldRetry);
    }

    [TestCase("retry-me.txt.1", 1)]
    [TestCase("retry-me.txt.9", 9)]
    [TestCase("retry-me.txt.11", 11)]
    [TestCase("retry-me.txt.111", 111)]
    public void RetryResourceSubsequentTimeRetryProperIndex(string resourceName, int expectedIndex)
    {
        var instance = new ResourceRetry(CreateEventData(resourceName));
        Assert.AreEqual(expectedIndex, instance.RetryIndex);
        Assert.AreEqual("retry-me.txt", instance.ResourceName);
        Assert.IsTrue(instance.ShouldRetry);
    }
}