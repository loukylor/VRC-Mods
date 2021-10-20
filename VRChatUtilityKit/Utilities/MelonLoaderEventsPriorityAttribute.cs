using System;

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// An attribute to dictate the priority of the MelonLoaderEvents attribute/class.
    /// Lower is higher priority.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MelonLoaderEventsPriorityAttribute : Attribute
    {
        /// <summary>
        /// The priority.
        /// Lower is higher priority.
        /// </summary>
        public readonly int priority;

        /// <summary>
        /// Creates a new MelonLoaderEventsPrority Attribute.
        /// </summary>
        /// <param name="priority">The priority of the attribute</param>
        public MelonLoaderEventsPriorityAttribute(int priority)
        {
            this.priority = priority;
        }
    }
}
