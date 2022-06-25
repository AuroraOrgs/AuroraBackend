using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Bridge
{
    public class MediatorHangfireBridge
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MediatorHangfireBridge> _logger;

        public MediatorHangfireBridge(IMediator mediator, ILogger<MediatorHangfireBridge> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Send(IRequest command)
        {
            _logger.LogInformation("Sending unnamed request to Mediatr from Hangfire queue");
            await _mediator.Send(command);
        }

        [DisplayName("{0}")]
        public async Task Send(string jobName, IRequest command)
        {
            _logger.LogInformation("Sending '{0}' request to Mediatr from Hangfire queue", jobName);
            await _mediator.Send(command);
        }
    }
}