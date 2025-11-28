using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;
using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI;

namespace ArlaNatureConnect.WinUI.Converters;

public sealed class RoleIdToRoleNameConverter : IValueConverter
{
    private readonly IRoleRepository? _roleRepository;

    public RoleIdToRoleNameConverter()
    {
        // Resolve repository from DI via App.HostInstance if available; fallback to null
        try
        {
            _roleRepository = App.HostInstance?.Services.GetService(typeof(IRoleRepository)) as IRoleRepository;
        }
        catch
        {
            _roleRepository = null;
        }
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Guid guid) return string.Empty;

        try
        {
            if (_roleRepository == null) return string.Empty;

            var roles = _roleRepository.GetAllAsync().GetAwaiter().GetResult();
            foreach (var r in roles)
            {
                if (r.Id == guid) return r.Name ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // Not needed for display-only conversion
        return null!;
    }
}
