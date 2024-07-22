using System.IO;

namespace Function;

public class ResourceRetry
{
    public bool ShouldRetry { get; }
    public int RetryIndex { get; }
    public string ResourceName { get; }

    public ResourceRetry(EventData data)
    {
        ResourceName = data.ResourceName;
        if (data.ResourceName.StartsWith("retry-me.txt"))
        {
            ShouldRetry = true;
            var fileExt = Path.GetExtension(data.ResourceName);
            if (fileExt != ".txt" && fileExt?.Length > 1)
            {
                if (int.TryParse(fileExt.Substring(1), out int index))
                {
                    RetryIndex= index;
                    ResourceName = Path.GetFileNameWithoutExtension(ResourceName);
                }
            }
        }
    }
}