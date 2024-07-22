using System.IO;

namespace Function;

public class ResourceRetry
{
    public bool ShouldRetry { get; }
    public int RetryIndex { get; }
    public string ResourceName { get; }
    public bool IsInFolder { get; }

    public ResourceRetry(EventData data)
    {
        ResourceName = data.ResourceName;

        if (data.ResourceName.StartsWith("retry-me.txt") || data.ResourceName.StartsWith("failures/"))
        {
            ShouldRetry = true;
            var fileExt = Path.GetExtension(data.ResourceName);
            if (fileExt != ".txt" && fileExt?.Length > 1)
            {
                IsInFolder = data.ResourceName.StartsWith("failures/");
                if (int.TryParse(fileExt.Substring(1), out int index))
                {
                    RetryIndex= index;
                    ResourceName = Path.GetFileNameWithoutExtension(ResourceName);
                }
            }
        }
    }
}