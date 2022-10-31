using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace RealTimeCaptionTranslate
{



    class RealTimeCaptionCapture
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private AutomationElement AutoElement
        {
            get; set;
        }

        private AutomationElement DocumentElement
        {
            get; set;
        }

        private string PrevText
        {
            get; set;
        }

        public void Start()
        {
            try
            {
                IntPtr parentHandle = FindWindow(null, "即時字幕");
                AutoElement = AutomationElement.FromHandle(parentHandle);
                DocumentElement = AutoElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document));
                _ = Task.Run(() => CaptureTextLoop());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        private async void CaptureTextLoop()
        {
            Runtime.PythonDLL = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"python310.dll");
            PythonEngine.Initialize();
            dynamic _ = PythonEngine.BeginAllowThreads();

            DateTime start = DateTime.Now;
            while (true)
            {
                DateTime current = DateTime.Now;
                TimeSpan timeDiff = current - start;
                if (timeDiff.TotalMilliseconds < 250)
                {
                    await Task.Delay(250-(int)timeDiff.TotalMilliseconds);
                }
                start = current;

                try
                {
                    string text = CaptureText();
                    if (text == PrevText)
                    {
                        continue;
                    }
                    PrevText = text;
                    Console.WriteLine(text);
                    Console.WriteLine(timeDiff.TotalMilliseconds);
                    text = Translate(text);
                    InvokeEvent(text);
                } catch (Exception ex)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void InvokeEvent(string text)
        {
            TextReceivedEventArgs args = new TextReceivedEventArgs
            {
                Text = text
            };
            OnTextReceived(args);
        }

        private string CaptureText()
        {
            AutomationElementCollection elementCollection = DocumentElement.FindAll(
                       TreeScope.Descendants,
                       new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text)
                       );
            AutomationElement[] elementArray = new AutomationElement[elementCollection.Count];
            elementCollection.CopyTo(elementArray, 0);
            string text = "";
            foreach (AutomationElement textElement in elementArray.Reverse().Take(2).Reverse())
            {
                text += textElement.Current.Name + "\n";
            }
            return text;
        }

        private string Translate(string text)
        {
            string result = "";
            try
            {
                using (Py.GIL())
                {
                    dynamic translate = Py.Import("translate");
                    result = translate.translate(text);
                }
                //PythonEngine.Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }


        private void OnTextReceived(TextReceivedEventArgs e)
        {
            TextReceived?.Invoke(this, e);
        }

        public event EventHandler<TextReceivedEventArgs> TextReceived;
    }

    public class TextReceivedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
}
