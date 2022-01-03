using Aurora.Application.Models;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface INotificator
    {
        Task NotifyAboutScrapFinishing(string userId, SearchResultDto result);
    }
}
