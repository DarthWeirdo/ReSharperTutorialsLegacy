using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.Application.Settings;
using JetBrains.CommonControls.Browser;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.TextControl;
using JetBrains.Threading;
using JetBrains.UI.ActionsRevised.Shortcuts;
using JetBrains.UI.Application;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Extensions;
using JetBrains.UI.ToolWindowManagement;
using pluginTestW04.tutorialStep;
using Button = System.Windows.Forms.Button;

namespace pluginTestW04.tutorialWindow
{
    public class TutorialWindow : IStepView
    {
        private const string HtmlDoctype = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
        private const string HtmlHead = @"
<HTML>
  <HEAD>
    <TITLE></TITLE>
<SCRIPT LANGUAGE='JavaScript'>
<!--
function toggle(elementid){
if (document.getElementById(elementid).style.display == 'none'){
document.getElementById(elementid).style.display = '';
} else {
document.getElementById(elementid).style.display = 'none';
}
} 
//-->
</SCRIPT>
    <style type='text/css'>

html, body { font-family:tahoma, sans-serif; font-size:90%; }
p { text-align:justify }
A { color: blue; cursor: hand; text-decoration: underline; }
A:link {color: blue; cursor: hand; text-decoration: underline; }
A:visited {color: blue; cursor: hand; text-decoration: underline;}
.symbol {color:darkblue; text-decoration: bold; }

    </style>
  </HEAD>
  ";


        private readonly IPsiServices _psiServices;
        private readonly IActionShortcuts _shortcutManager;
        private readonly ISettingsStore _settingsStore;
        private readonly DataContexts _dataContexts;
        private readonly ISolution _solution;
        private readonly IActionManager _actionManager;
        private readonly IShellLocks _shellLocks;
        private HtmlViewControl _viewControl = new HtmlViewControl(null, null);
        private IDisposable _waitingForCaches;
        private string _stepText;
        private Button _buttonNext = new Button();
        private TutorialStepPresenter _stepPresenter;
        private readonly Lifetime _lifetime;
        public string TutorialText { set { PrepareHtmlContent(value); } }
//        private EventHandler _buttonNextEventHandler;

//        public ISignal<bool> AfterButtonNextClicked { get; private set; }

        public string StepText
        {
            get { return _stepText; }
            set
            {
                _stepText = value;
                _shellLocks.ExecuteOrQueue(_lifetime, "TutorialTextUpdate",
                    () =>
                    {
                        _viewControl.DocumentText = PrepareHtmlContent(_stepText);
                    });
//                _shellLocks.TryExecuteWithReadLock((() => { _viewControl.DocumentText = PrepareHtmlContent(_stepText); }));
            }
        }

        public string ButtonText
        {
            get { return _buttonNext.Text; }
            set { _buttonNext.Text = value; }
        }

        public bool ButtonVisible
        {
            get { return _buttonNext.Visible; }
            set { _buttonNext.Visible = value; }
        }

        public event EventHandler NextStep;


        public TutorialWindow(string contentPath, Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  TextControlManager textControlManager, IShellLocks shellLocks, IEditorManager editorManager,
                                  DocumentManager documentManager, IUIApplication environment, IActionManager actionManager,
                                  ToolWindowManager toolWindowManager, TutorialWindowDescriptor toolWindowDescriptor,
                                  IWindowsHookManager windowsHookManager, IPsiServices psiServices, IActionShortcuts shortcutManager)
        {
            _lifetime = lifetime;
            _solution = solution;
            _actionManager = actionManager;
            _shellLocks = shellLocks;
            _psiServices = psiServices;
            _shortcutManager = shortcutManager;            

            if (solution.GetComponent<ISolutionOwner>().IsRealSolutionOwner)
            {
                toolWindowManager.Classes[toolWindowDescriptor].RegisterInstance(
                    lifetime, null, null,
                    (lt, twi) =>
                    {
//                        AfterButtonNextClicked = new Signal<bool>(lt, "TutorialWindow.AfterButtonNextClicked");

                        var containerControl = new TutorialPanel(environment).BindToLifetime(lt);                        

                        var viewControl = new HtmlViewControl(windowsHookManager, actionManager)
                        {
                            BackColor = Color.White,
                            //DefaultTextControlSchemeManager.Instance.CodeEditorBackground,  
                            //DocumentText = InitialPage(),
                            Dock = DockStyle.Fill,                            
                        }.BindToLifetime(lt);

                        var buttonNext = new Button
                        {
                            Text = "Next",
                            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                        };
                        
                        lt.AddBracket(
                            () => _buttonNext = buttonNext,
                            () => _buttonNext = null);

                        _buttonNext.Top = containerControl.Height - _buttonNext.Height - 10;
                        _buttonNext.Left = containerControl.Width - _buttonNext.Width - 25;

                        lt.AddBracket(
                            () => _viewControl = viewControl,
                            () => _viewControl = null);                                                

                        lt.AddBracket(
                            () => { _buttonNext.Click += NextStep; },
                            () => { _buttonNext.Click -= NextStep; });

                        lt.AddBracket(
                            () => containerControl.Controls.Add(_buttonNext),
                            () => containerControl.Controls.Remove(_buttonNext));

                        lt.AddBracket(
                            () => containerControl.Controls.Add(_viewControl),
                            () => containerControl.Controls.Remove(_viewControl));

                        return new EitherControl(lt, containerControl);
                    });

                _stepPresenter = new TutorialStepPresenter(this, contentPath, lifetime, solution, psiFiles, textControlManager, 
                    shellLocks, editorManager, documentManager, environment, actionManager, psiServices, shortcutManager);
            }
        }


        private static string InitialPage()
        {
            return PrepareHtmlContent("Loading tutorial...");
        }


        private static string PrepareHtmlContent(string content)
        {
            var html = new StringBuilder();
            BuildHeader(html);
            html.Append(content);
            BuildFooter(html);
            return html.ToString();
        }


        private static void BuildHeader(StringBuilder html)
        {
            html.AppendLine(HtmlDoctype);
            html.AppendLine(HtmlHead);
            html.AppendLine("<BODY>");
        }


        private static void BuildFooter(StringBuilder html)
        {
            html.AppendLine("</BODY>");
            html.AppendLine("</HTML>");
        }

        
    }
}
