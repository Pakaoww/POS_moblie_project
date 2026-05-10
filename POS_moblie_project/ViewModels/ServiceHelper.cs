namespace POS_moblie_project.ViewModels;

public static class ServiceHelper
{
    public static T GetService<T>() where T : class
    {
        if (Application.Current?.Handler?.MauiContext?.Services
                .GetService(typeof(T)) is T service)
        {
            return service;
        }
        throw new Exception($"Unable to resolve type {typeof(T).Name}");
    }
}