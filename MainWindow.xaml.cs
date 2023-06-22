using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Resources;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;
using System.Windows.Media;

namespace BlockStreamSatAPI
{
    public partial class MainWindow : Window
    {
        private List<FunctionModel> _functions;
        public MainWindow()
        {
            InitializeComponent();

            _functions = JsonConvert.DeserializeObject<List<FunctionModel>>(File.ReadAllText(Path.Combine(getResourcesPath(), "funcAndDesc.json")));

            setFunctions(_functions);

        }

        private void setFunctions(List<FunctionModel> functions)
        {
            foreach (var func in _functions)
            {
                Button button = new Button { Content = func.Name, Margin = new Thickness(5) };
                button.Click += (sender, e) => { MakeViewForFunction(func); };
                funcsName.Children.Add(button);
            }
        }

        private void MakeViewForFunction(FunctionModel func)
        {
            currentFuncName.Text = $"Функция: {func.Name}";
            currentFuncDesc.Text = func.Desc;
            funcReqParamsBox.Children.Clear();
            funcOptParamsBox.Children.Clear();
            sendButtonPlace.Children.Clear();
            resultTextBlock.Text = "";

            if (func.Params.Count != 0)
            {
                for (int i = 0; i < func.Params.Count; i++)
                {
                    TextBlock paramName = new TextBlock() { Text = func.Params[i].Name.Replace("Optional", "") + ":" };
                    TextBox valueBox = new TextBox { Width = 150, Name = $"{func.Params[i]}", Margin = new Thickness(10, 0, 0, 0) };
                    if (func.Params[i].EndsWith("Optional"))
                    {
                        funcOptParamsBox.Children.Add(paramName);
                        funcOptParamsBox.Children.Add(valueBox);
                    }
                    else
                    {
                        funcReqParamsBox.Children.Add(paramName);
                        funcReqParamsBox.Children.Add(valueBox);
                    }
                }
            }

            if(funcReqParamsBox.Children.Count < 1)
            {
                funcReqParamsBox.Children.Add(new TextBlock { Text = "Параметров не принимает" });
            }
            if(funcOptParamsBox.Children.Count < 1)
            {
                funcOptParamsBox.Children.Add(new TextBlock { Text = "Параметров не принимает" });
            }

            Button sendButton = new Button {Content = "Отправить запрос", Margin = new Thickness(5) };
            sendButton.Click += (sender, e) => {
                if (funcReqParamsBox.Children.Count <= 2)
                {
                    funcReqParamsBox.Background = Brushes.Red;
                }
                else
                {
                    funcReqParamsBox.Background = Brushes.White;
                    ExecuteFunction(func, funcReqParamsBox, funcOptParamsBox);
                }
            };
            sendButtonPlace.Children.Add(sendButton);

        }

        private void ExecuteFunction(FunctionModel func, StackPanel funcReqParamsBox, StackPanel funcOptParamsBox)
        {
            if (func.Method == null)
            {
                restManager.extractMethodFromUrl(func);
            }

            if (funcReqParamsBox.Children.Count < 1)
            {
                
            }

            Dictionary<string, string> reqParameters = new Dictionary<string, string>();
            Dictionary<string, string> optParameters = new Dictionary<string, string>();

            if (funcReqParamsBox.Children.Count > 1)
            {
                reqParameters = getParametersFromUserInput(funcReqParamsBox);
            }

            if (funcOptParamsBox.Children.Count > 1)
            {
                optParameters = getParametersFromUserInput(funcOptParamsBox);
            }


            resultTextBlock.Text = restManager.request(func.URL, func.Method, reqParameters, optParameters);

        }

        private Dictionary<string, string> getParametersFromUserInput(StackPanel stackPanel)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            List<string> parameterNames = new List<string>();
            List<string> parameterValues = new List<string>();

            if (stackPanel.Children.Count > 1)
            {

                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBox textBox)
                    {
                        string parameterValue = textBox.Text;
                        parameterValues.Add(parameterValue);
                    }
                    if (child is TextBlock textBlock)
                    {
                        string parameterName = textBlock.Text;
                        parameterName.Remove(parameterName.Length - 1);
                        parameterNames.Add(parameterName);
                    }
                }

                for (int i = 0; i < parameterNames.Count; i++)
                {
                    parameters.Add(parameterNames[i], parameterValues[i]);
                }
                return parameters;
            }
            else
            {
                return null;
            }
        }

        private void FunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private string getResourcesPath()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(appDirectory).Parent.Parent.FullName;
            string resourcesDirectory = Path.Combine(parentDirectory,"Resources");
            return resourcesDirectory;
        }
    }
}
