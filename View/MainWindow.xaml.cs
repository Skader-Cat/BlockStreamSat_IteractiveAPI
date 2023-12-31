﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Documents;
using StreamBlockSat_InterAPI.Model;
using StreamBlockSat_InterAPI.Controller;

namespace BlockStreamSatAPI
{
    public partial class MainWindow : Window
    {
        private List<FunctionModel> _functions;
        public MainWindow()
        {
            InitializeComponent();

            _functions = JsonConvert.DeserializeObject<List<FunctionModel>>(FunctionModel.getFuncAndDescJsonFromResources());

            setFunctions(_functions);

        }

        private void setFunctions(List<FunctionModel> functions)
        {
            foreach (var func in _functions)
            {
                var buttonText = new TextBlock() { Text = func.Name, TextWrapping = TextWrapping.Wrap };
                Button button = new Button { Content = buttonText, Margin = new Thickness(5) };

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


            Button sendButton = new Button { Content = "Отправить запрос", Margin = new Thickness(5) };
            sendButton.Click += (sender, e) => {
                if (Controller.isAllRequiredFunctionsFilled(func, funcParamsBox, resultTextBlock))
                {
                    ExecuteFunction(func);
                }
            };
            sendButtonPlace.Children.Add(sendButton);
        }

        private void setParamsToView(FunctionModel func)
        {
            if (func.Params.Count > 0)
            {
                for (int i = 0; i < func.Params.Count; i++)
                {
                    TextBox valueBox = new TextBox { Width = 300, Name = $"{func.Params[i].Name}", Margin = new Thickness(10, 0, 0, 0) };
                    TextBlock paramName = new TextBlock() { Text = func.Params[i].Name };
                    if (func.Params[i].isRequired == true)
                    {
                        paramName.Inlines.Add(new Run("*") { Foreground = Brushes.Red });
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
            resultTextBlock.Text = string.Empty;
            resultTextBlock.Visibility = Visibility.Hidden;
        }

        private void ExecuteFunction(FunctionModel func)
        {
            Controller.setParamValuesFromInputBox(func, funcParamsBox);
            resultTextBlock.Text = RestManager.request(func);
            resultTextBlock.Visibility = Visibility.Visible;
        }
    }
}
