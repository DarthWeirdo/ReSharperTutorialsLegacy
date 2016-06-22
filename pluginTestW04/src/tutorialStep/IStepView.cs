using System;

namespace pluginTestW04.tutorialStep
{
    public interface IStepView
    {
        string StepText { get; set; }
        string ButtonText { get; set; }
        bool ButtonVisible { get; set; }

        event EventHandler NextStep;
    }
    
}
