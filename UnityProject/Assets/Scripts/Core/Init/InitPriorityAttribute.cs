using System;

namespace GomokuGame.Core.Init
{
    /// <summary>
    /// Attach to MonoBehaviours to control initialization order (higher first).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitPriorityAttribute : Attribute
    {
        public int Priority { get; }
        public InitPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
