using ArlaNatureConnect.Core.Abstract.Services;
using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Services;

public class PrivilegeService : IPrivilegeService
{
    #region Fields
    #endregion
    #region Properties
    public Person? CurrentUser { get; set; }
    #endregion
    #region Events
    #endregion
    #region Constructor
    public PrivilegeService()
    {
    }
    #endregion
}
