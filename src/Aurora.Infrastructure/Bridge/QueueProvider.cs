using Aurora.Application.Contracts;
using Hangfire;
using MediatR;

namespace Aurora.Infrastructure.Bridge;

public class QueueProvider : IQueueProvider
{
    public void Enqueue(string jobName, IRequest request)
    {
        var client = new BackgroundJobClient();
        client.Enqueue<MediatorHangfireBridge>(bridge => bridge.Send(jobName, request));
    }

    public void Enqueue(IRequest request)
    {
        var client = new BackgroundJobClient();
        client.Enqueue<MediatorHangfireBridge>(bridge => bridge.Send(request));
    }
}