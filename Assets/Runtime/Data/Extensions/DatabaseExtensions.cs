using Runtime.Data.Abstract;
using UnityEngine;
using VContainer;
using VitalRouter.VContainer;

namespace Runtime.Data.Extensions
{
    public static class DatabaseExtensions
    {
        public static void RegisterDatabase<TDatabase, TConfig>(this IContainerBuilder builder, string path) 
            where TDatabase : ConfigDatabase<TConfig>
        {
            var database = Resources.Load<TDatabase>($"Databases/{path}");
            builder.RegisterInstance<TDatabase>(database);
        }
        
        public static void RegisterDatabase<TDatabase, TConfig>(this RoutingBuilder builder, string path) 
            where TDatabase : ConfigDatabase<TConfig>
            where TConfig : class
        {
            var database = Resources.Load<TDatabase>($"Databases/{path}");
            builder.Map(database);
        }
    }
}