using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class Notificator : INotificator
    {
        private readonly IHubContext<NotificationHub> _context;

        public Notificator(IHubContext<NotificationHub> context)
        {
            _context = context;
        }

        public Task NotifyAboutScrapFinishing(string userId, SearchResultDto result) => 
            _context.Clients.User(userId).SendAsync("ScrapFinished", result);
    }
}
