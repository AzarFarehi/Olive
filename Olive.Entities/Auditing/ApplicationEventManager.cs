using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Olive.Entities
{
    /// <summary>
    /// Provides services for application events and general logging.
    /// </summary>
    public static class ApplicationEventManager
    {
        internal static DefaultApplicationEventManager CurrentApplicationEventManager;

        static ApplicationEventManager()
        {
            var provider = Config.Get("Olive.ApplicationEventManager");
            if (provider.HasValue())
                CurrentApplicationEventManager = (DefaultApplicationEventManager)Type.GetType(provider).CreateInstance();
            else
                CurrentApplicationEventManager = new DefaultApplicationEventManager();
        }

        public static async Task RecordSave(IEntity entity, SaveMode saveMode) =>
            await CurrentApplicationEventManager.RecordSave(entity, saveMode);

        /// <summary>
        /// Gets the changes XML for a specified object. That object should be in its OnSaving event state.
        /// </summary>
        public static async Task<string> GetChangesXml(IEntity entityBeingSaved) =>
            await CurrentApplicationEventManager.GetChangesXml(entityBeingSaved);

        /// <summary>
        /// Gets the changes applied to the specified object.
        /// Each item in the result will be {PropertyName, { OldValue, NewValue } }.
        /// </summary>
        public static IDictionary<string, Tuple<string, string>> GetChanges(IEntity original, IEntity updated) =>
            CurrentApplicationEventManager.GetChanges(original, updated);

        public static Task RecordDelete(IEntity entity) => CurrentApplicationEventManager.RecordDelete(entity);

        public static Dictionary<string, string> GetDataToLog(IEntity entity) =>
            CurrentApplicationEventManager.GetDataToLog(entity);

        public static string ToChangeXml(IDictionary<string, Tuple<string, string>> changes) =>
            CurrentApplicationEventManager.ToChangeXml(changes);

        /// <summary>
        /// Records the execution result of a scheduled task. 
        /// </summary>
        /// <param name="task">The name of the scheduled task.</param>
        /// <param name="startTime">The time when this task was started.</param>
        public static async Task RecordScheduledTask(string task, DateTime startTime) =>
            await CurrentApplicationEventManager.RecordScheduledTask(task, startTime);

        /// <summary>
        /// Records the execution result of a scheduled task. 
        /// </summary>
        /// <param name="task">The name of the scheduled task.</param>
        /// <param name="startTime">The time when this task was started.</param>
        /// <param name="error">The Exception that occurred during the task execution.</param>
        public static async Task RecordScheduledTask(string task, DateTime startTime, Exception error) =>
            await CurrentApplicationEventManager.RecordScheduledTask(task, startTime, error);

        /// <summary>
        /// Loads the item recorded in this event.
        /// </summary>
        public static async Task<IEntity> LoadItem(this IApplicationEvent applicationEvent) =>
            await CurrentApplicationEventManager.LoadItem(applicationEvent);

        /// <summary>
        /// Gets the current user id.
        /// </summary>
        public static string GetCurrentUserId(IPrincipal principal) => CurrentApplicationEventManager.GetCurrentUserId(principal);

        /// <summary>
        /// Gets the IP address of the current user.
        /// </summary>
        public static string GetCurrentUserIP() => CurrentApplicationEventManager.GetCurrentUserIP();
    }
}