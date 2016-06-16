using System;

namespace pluginTestW04.checker
{
    internal enum OnEvent {None, PsiChange, CaretMove, AfterAction}

    [AttributeUsage(AttributeTargets.Method)]
    internal class RunCheckAttribute : Attribute
    {
        public readonly OnEvent OnEvent;

        public RunCheckAttribute(OnEvent onEvent)
        {
            OnEvent = onEvent;
        }
    }
}
