

public interface IMessageQueueService
{
    void PublishMessage(string message);

    T GetChannel<T>();

    void Dispose();
}