﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Resources;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Documents;

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

            clearView();

            setParamsToView(func);


            Button sendButton = new Button {Content = "Отправить запрос", Margin = new Thickness(5) };
            sendButton.Click += (sender, e) => {
                if (isAllRequiredFunctionsFilled(func))
                {
                    ExecuteFunction(func);
                }
            };
            sendButtonPlace.Children.Add(sendButton);

        }

        private bool isAllRequiredFunctionsFilled(FunctionModel func)
        {
            foreach(var child in funcParamsBox.Children)
            {
                if (child is TextBox)
                {
                    var param = func.Params.Find(x => x.Name == ((TextBox) child).Name && x.isRequired == true);
                    if (param != null)
                    {
                        if (((TextBox) child).Text == "") //в будущем можно добавить валидацию данных в полях
                        {
                           ((TextBox) child).Background = Brushes.Red;
                           resultTextBlock.Text = $"Ошибка. Не введён обязательный параметр {param.Name}";
                           return false;
                        }
                    }
                }
            }
            return true;
        }

        private void setParamsToView(FunctionModel func)
        {
            if(func.Params.Count > 0)
            {
                for(int i = 0; i < func.Params.Count; i++)
                {
                    TextBox valueBox = new TextBox { Width = 300, Name = $"{func.Params[i].Name}", Margin = new Thickness(10, 0, 0, 0) };
                    TextBlock paramName = new TextBlock() { Text = func.Params[i].Name };
                    if (func.Params[i].isRequired == true)
                    {
                        paramName.Inlines.Add(new Run("*") {Foreground = Brushes.Red});
                    }
                    paramName.Inlines.Add(new Run(":"));

                    funcParamsBox.Children.Add(paramName);
                    funcParamsBox.Children.Add(valueBox);
                }
                isEmptyParameters.Visibility = Visibility.Collapsed;
            }
            else
            {
                isEmptyParameters.Visibility = Visibility.Visible;
            }
        }

        private void clearView()
        {
            funcParamsBox.Children.Clear();
            funcParamsBox.Children.Clear();
            sendButtonPlace.Children.Clear();
            resultTextBlock.Text = "";
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

        private string getResourcesPath()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(appDirectory).Parent.Parent.FullName;
            string resourcesDirectory = Path.Combine(parentDirectory,"Resources");
            return resourcesDirectory;
        }

        private void ExecuteFunction(FunctionModel func)
        {
            if (func.Method == null)
            {
                restManager.extractMethodFromUrl(func);
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();


            parameters = getParametersFromUserInput(funcParamsBox);


            resultTextBlock.Text = restManager.request(func.URL, func.Method, parameters);

        }
    }
}
