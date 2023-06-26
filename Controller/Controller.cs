using StreamBlockSat_InterAPI.Model;
using System.Windows.Controls;
using System.Windows.Media;

namespace StreamBlockSat_InterAPI.Controller
{
    internal class Controller
    {
        public static void setParamValuesFromInputBox(FunctionModel func, StackPanel funcParamsBox)
        {
            foreach (var param in funcParamsBox.Children)
            {
                if (param is TextBox)
                {
                    var paramTextBox = (TextBox)param;
                    func.SetParamValue(paramTextBox.Name, paramTextBox.Text.Trim());
                }
            }
        }

        public static bool isAllRequiredFunctionsFilled(FunctionModel func, StackPanel funcParamsBox, TextBox resultTextBlock)
        {
            foreach (var child in funcParamsBox.Children)
            {
                if (child is TextBox)
                {
                    var paramTextBox = (TextBox)child;
                    paramTextBox.Background = null;
                    var param = func.Params.Find(x => x.Name == paramTextBox.Name && x.isRequired == true);
                    if (param != null)
                    {
                        if (paramTextBox.Text.Trim() == "") //в будущем можно добавить валидацию данных в полях
                        {
                            paramTextBox.Background = Brushes.Red;
                            resultTextBlock.Text = $"Ошибка. Не введён обязательный параметр {param.Name}";
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
