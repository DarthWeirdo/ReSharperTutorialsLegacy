using System;

namespace pluginTestW04
{
    enum OnEvent {None, PsiChange, CaretMove, AfterAction}

    [AttributeUsage(AttributeTargets.Method)]
    class RunCheckAttribute : Attribute
    {
        public readonly OnEvent OnEvent;

        public RunCheckAttribute(OnEvent onEvent)
        {
            OnEvent = onEvent;
        }
    }
}
