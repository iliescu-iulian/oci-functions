using System.IO;
using NUnit.Framework;

namespace Function.Tests;

[TestFixture]
public class EventDataTest
{
    [Test]
    public void SuccesfulyParseJson()
    {
        var jsonStr= File.ReadAllText("createObject.json");
        var data = new EventData(jsonStr);
        Assert.IsNotNull(data);
        Assert.AreEqual("ObjectStorage", data.Source);
        Assert.AreEqual("com.oraclecloud.objectstorage.createobject", data.EventType);
        Assert.AreEqual("bucket-2024-06-26-iulian", data.BucketName);
    }
}