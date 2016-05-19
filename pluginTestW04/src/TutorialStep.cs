using System.ComponentModel;
using JetBrains.Annotations;

namespace pluginTestW04
{
    public class TutorialStep : INotifyPropertyChanged
    {        
        // TODO: some event? or property subscribed to event that indicates the step is completed.        

        private string _text;                

        public int Id { get; }
        public string ProjectName { get;}
        public string FileName { get; }
        public string TypeName { get; }
        public string MethodName { get; }
        public string TextToFind { get; }
        public string Buttons { get; }

        public TutorialStep(int li, string text, string file, string projectName, string typeName, string methodName, 
            string textToFind, string buttons)
        {
            Id = li;
            _text = text;
            FileName = file;
            TypeName = typeName;
            Buttons = buttons;
            ProjectName = projectName;
            MethodName = methodName;
            TextToFind = textToFind;
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
