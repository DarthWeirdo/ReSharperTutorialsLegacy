using System.Windows;
using JetBrains.CommonControls.Browser;
using JetBrains.DataFlow;

namespace pluginTestW04.tutorialWindow
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class Animator
    {
        public ISignal<bool> AllAnimationsDone { get; }
        private readonly HtmlViewControl _viewControl;
        private bool _moveOutStepDone = false;

        private bool MoveOutStepDone
        {
            get
            {
                return _moveOutStepDone;
            }
            set
            {                
                _moveOutStepDone = value;
//                if (IsOtherAnimationsDone)                
                    OnAnimationsDone();
            }
        }

        private void OnAnimationsDone()
        {
            AllAnimationsDone.Fire(true);           
        }

        public Animator(Lifetime lifetime, HtmlViewControl viewControl)
        {
            AllAnimationsDone = new Signal<bool>(lifetime, "Animator.AllAnimationsDone");
            _viewControl = viewControl;
            _viewControl.ObjectForScripting = this;
        }

        public void Animate()
        {
            _viewControl.Document?.InvokeScript("moveOutPrevStep");
        }

        public void MoveOutPrevStepDone()
        {
            MoveOutStepDone = true;            
        }
    }
}
