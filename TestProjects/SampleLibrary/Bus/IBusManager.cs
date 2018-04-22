namespace SampleLibrary
{
    public interface IBusManager
    {
        bool Publish(object eventObject);
        bool Send(object commandObject);
    }
}