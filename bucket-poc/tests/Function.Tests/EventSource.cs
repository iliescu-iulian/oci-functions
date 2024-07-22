namespace Function.Tests;

public class BucketData
{
    public string BucketName { get; set; }
}

public class EventSourceData
{
    public  string ResourceName { get; set; }
    public BucketData AdditionalDetails { get;set; }
}

public class EventSource
{
    public string EventType { get; set; }
    public string Source { get; set; }
    public EventSourceData Data { get; set; }
}