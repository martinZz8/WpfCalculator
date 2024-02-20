using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variables
        private string _prevBtnContent = "";
        private string _calcValStr = "0"; // str to show in UI
        private string _savedValFirst = ""; // first saved value (also saved value from previous operation)
        private string _savedValSecond = ""; // saved any new value, if "_calcOperator" isn't blank
        private string _calcMemory = ""; // additionnal memory of the calc (user save into it by using "mS" button)
        private string _calcOperator = ""; // current operation on saved values
        private bool _isFirstComputationUsed = false; // flag, to indicate whether user used equation operation before (false - n, true - yes)
        private bool _isError = false; // error while dividing by zero

        // -- Methods --
        public MainWindow()
        {
            InitializeComponent();
            CalcValStr.Text = _calcValStr;
        }

        private void HandleButtonClick(string btnContent)
        {
            // -- Reset displayed zero, when user wants to enter a number --
            if (_calcValStr.Equals("0") && IsButtonANumber(btnContent))
            {
                _calcValStr = "";
            }

            if (!_isError)
            {
                // Enter a number
                if (IsButtonANumber(btnContent))
                {
                    _calcValStr += btnContent;
                }
                // Enter a dot (if available)
                else if (btnContent == "." && _calcValStr.Length > 0 && !_calcValStr.Contains(".") && _calcOperator != "pow")
                {
                    _calcValStr += btnContent;
                }
                // PErform sqrt, save first val and show result of operation to the user
                else if (btnContent == "sqrt")
                {
                    double numVal;

                    if (Double.TryParse(_calcValStr, out numVal))
                    {
                        if (numVal >= 0.0)
                        {
                            _savedValFirst = Math.Sqrt(numVal).ToString();
                            _calcValStr = _savedValFirst;
                            _calcOperator = btnContent;
                        }
                        else
                        {
                            RaiseError("You cannot sqrt negative number!");
                        }
                    }
                }
                // Enter operation sign (chaining operations e.g "2*3*5" doesn't work here)
                else if (btnContent == "/" || btnContent == "*" || btnContent == "+" || btnContent == "-" || btnContent == "pow")
                {
                    // Assign numerous value only when _prevBtnContent was a digit
                    if (!IsButtonAnOperation(_prevBtnContent))
                    {
                        // Save first or second val
                        if (!_isFirstComputationUsed)
                        {
                            _savedValFirst = _calcValStr;
                        }

                        _calcValStr = "0";
                        //_calcOperator = btnContent;
                    }

                    _calcOperator = btnContent;
                }
                // Toggle sign
                else if (btnContent == "+/-")
                {
                    if (CanSwapSign(_calcValStr)) //old: _calcValStr != "0"
                    {
                        char firstChar = _calcValStr[0];
                        if (IsButtonANumber(firstChar.ToString()))
                        {
                            _calcValStr = "-" + _calcValStr;
                        }
                        else if (firstChar == '-')
                        {
                            _calcValStr = _calcValStr.Substring(1);
                        }
                    }
                }
                // Save to memory
                else if (btnContent == "mS")
                {
                    _calcMemory = _calcValStr;
                }
                // Retrieve from memory
                else if (btnContent == "mR")
                {
                    if (_calcMemory != "")
                    {
                        _calcValStr = _calcMemory;
                    }
                }
                // Perform equation
                else if (btnContent == "=")
                {
                    if (_prevBtnContent != "sqrt")
                    {
                        // Save second val (if possible)
                        if (_prevBtnContent != "=" && !IsButtonAnOperation(_prevBtnContent))
                        {
                            _savedValSecond = _calcValStr;
                        }
                        // Operation after "=" sign means corresponding assign
                        else if (IsButtonAnOperation(_prevBtnContent))
                        {
                            _savedValSecond = _savedValFirst;
                        }

                        PerformComputation();
                    }
                    else
                    {
                        RaiseError("Invalid operation!");
                    }
                }
            }
            
            if (btnContent == "CE")
            {
                _savedValSecond = "";
                _calcValStr = "0";
                _isError = false;

                // Delete also first value, when no operation is performed yet.
                if (_calcOperator == "" && !_isFirstComputationUsed)
                {
                    _savedValFirst = "";                  
                }
            }
            else if (btnContent == "C")
            {
                _calcValStr = "0";
                _savedValFirst = "";
                _savedValSecond = "";
                _calcOperator = "";
                _isFirstComputationUsed = false;
                _isError = false;
            }

            Console.WriteLine($"_calcValStr is: {_calcValStr}");
            CalcValStr.Text = _calcValStr;
        }

        private bool IsButtonANumber(string btnContent) {
            return btnContent == "0" ||
                   btnContent == "1" ||
                   btnContent == "2" ||
                   btnContent == "3" ||
                   btnContent == "4" ||
                   btnContent == "5" ||
                   btnContent == "6" ||
                   btnContent == "7" ||
                   btnContent == "8" ||
                   btnContent == "9";
        }

        private bool IsButtonAnOperation(string btnContent)
        {
            return btnContent == "sqrt" ||
                   btnContent == "/" ||
                   btnContent == "*" ||
                   btnContent == "+" ||
                   btnContent == "-" ||
                   btnContent == "pow";
        }

        private bool CanSwapSign(string vl)
        {
            foreach (char c in vl)
            {
                if (IsButtonANumber(c.ToString()) && c != '0')
                {
                    return true;
                }
            }

            return false;
        }

        private void RaiseError(string msg)
        {
            _calcValStr = msg;
            _isError = true;
        }

        private void PerformComputation()
        {
            // Compute data
            double valFirstNum;
            double valSecondNum;
            double resultNum = 0.0;

            if (Double.TryParse(_savedValFirst, out valFirstNum) && Double.TryParse(_savedValSecond, out valSecondNum))
            {
                switch (_calcOperator)
                {
                    case "/":
                        if (valSecondNum != 0.0)
                        {
                            resultNum = valFirstNum / valSecondNum;
                        }
                        else
                        {
                            RaiseError("You can't divide by zero!");
                            return;
                        }
                        break;
                    case "*":
                        resultNum = valFirstNum * valSecondNum;
                        break;
                    case "+":
                        resultNum = valFirstNum + valSecondNum;
                        break;
                    case "-":
                        resultNum = valFirstNum - valSecondNum;
                        break;
                    case "pow":
                        resultNum = Math.Pow(valFirstNum, valSecondNum);
                        break;
                    default:
                        throw new Exception("Unexpected calc operation");
                }

                _savedValFirst = resultNum.ToString();
                _calcValStr = _savedValFirst;

                _isFirstComputationUsed = true;
            }
        }

        // -- Methods called from View -
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string btnContent = btn.Content.ToString();

            HandleButtonClick(btnContent);
            _prevBtnContent = btnContent;
        }
    }

    // List of available buttons:
    //Btn0,
    //Btn1,
    //Btn2,
    //Btn3,
    //Btn4,
    //Btn5,
    //Btn6,
    //Btn7,
    //Btn8,
    //Btn9,
    //BtnPM,
    //BtnDot,
    //BtnAdd,
    //BtnSub,
    //BtnMul,
    //BtnDiv,
    //BtnSqrRt,
    //BtnPow,
    //BtnMemSave,
    //BtnMemDel,
    //BtnCE,
    //BtnC,
    //BtnEq
}
