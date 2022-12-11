namespace MultiRPC;

public class ImageFormat
{
    public ImageFormat(string name, params string[] extensions)
    {
        Name = name;
        Extensions = extensions;
    }

    public string Name { get; }

    public string[] Extensions { get; }
}