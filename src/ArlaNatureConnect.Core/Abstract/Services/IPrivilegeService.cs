using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract.Services;

public interface IPrivilegeService
{
    public Person? CurrentUser { get; }
}
