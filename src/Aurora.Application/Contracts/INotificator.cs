using Aurora.Application.Models;

namespace Aurora.Application.Contracts;

public interface INotificator
{
    Task NotifyAboutScrapFinishing(string userId, SearchResultDto result);
}
