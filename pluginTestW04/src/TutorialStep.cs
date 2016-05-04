using System.ComponentModel;
using JetBrains.Annotations;

namespace pluginTestW04
{
    public class TutorialStep : INotifyPropertyChanged
    {
        // TODO: something with ITreeNode for indicating step position.
        // probably we should save tree to a separate structure
        // right after opening a solution. Then somehow match 
        // node in the tree and each step.

        // TODO: some event? or property subscribed to event that indicates the step is completed.

        

        private string _text;
        private int _li;
        private string _file;
        private string _treeNode; // TODO: this must be of some ITreeNode class
        private string _buttons; // TODO: probably remove this as we'll leave only Next button

        public TutorialStep(int li, string text, string file, string treeNode, string buttons)
        {
            _li = li;
            _text = text;
            _file = file;
            _treeNode = treeNode;
            _buttons = buttons;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text)
                    return;

                _text = value;
                OnPropertyChanged("Text");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrepareIde()
        {
            // TODO: here we must do some preparations for the step: Open file and place cursor to a particular line
            // Probably we can do this not here, but inside Narrator
        }        

        public void CheckStepIsDone()
        {
            // TODO: Here we must check Solution Tree to check whether a user performed the 
            // TODO: required step. We can check it explicitly or it could be some Analyzer that accepts our condition:
            /*
                Code is changed (possible solution: ITreeNode)
		        Shortcut is applied (watch for Action?)
		        Tool window is opened / closed (watch for Action?)
		        Button in a tool window is clicked
		        Menu item is selected
                Context menu item is selected
            */
        }
    }
}
