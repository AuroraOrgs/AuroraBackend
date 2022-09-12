namespace Aurora.Application.Contracts;

public interface IQueueProvider
{
    void Enqueue(IRequest request);
    void Enqueue(string jobName, IRequest request);
}