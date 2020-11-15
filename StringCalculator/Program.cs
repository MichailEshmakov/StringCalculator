using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace StringCalculator
{
    class Program
    {
        static string number = @"\d+(\.\d+|\d*)";
        static string expressionPattern = @$"^(\s*[+-]\s*|\s*){number}\s*(\s*[+/*-]\s*{number}\s*)*$";
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Введите выражение");
                string expression = DeleteDuplicatedPlusesAndMinuses(Console.ReadLine());

                try
                {
                    if (new Regex("[)]").Matches(expression).Count != new Regex("[(]").Matches(expression).Count)
                    {
                        throw new Exception("Количество открывающих скобок не совпадает с количеством закрывающих");
                    }

                    while (expression.IndexOf('(') != -1)
                    {
                        expression = CalculateExpression(expression);
                    }

                    Console.WriteLine(CalculateExpression(expression));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }

                Console.WriteLine();
            }
        }

        private static string CalculateExpression(string expression)
        {
            int firstClosingIndex = expression.IndexOf(')');
            int previousOpeningIndex = firstClosingIndex == -1 ? -1 : expression.LastIndexOf('(', firstClosingIndex);

            if (firstClosingIndex != -1 && previousOpeningIndex != -1)
            {
                string newValue = CalculateExpression(expression.Substring(previousOpeningIndex + 1, firstClosingIndex - previousOpeningIndex - 1));
                expression = expression.Remove(previousOpeningIndex, firstClosingIndex - previousOpeningIndex + 1);
                expression = expression.Insert(previousOpeningIndex, newValue);
                return expression;
            }
            else if (firstClosingIndex == -1 && previousOpeningIndex == -1)
            {
                if (Regex.IsMatch(expression, expressionPattern))
                {
                    string pattern = $@"({number}|[-+*/])";
                    List<Node> nodes = new List<Node>();
                    foreach (Match math in Regex.Matches(expression, pattern))
                    {
                        nodes.Add(new Node(math.Value));
                    }

                    if (nodes[0].CoreSymbol == "-" || nodes[0].CoreSymbol == "+")
                    {
                        nodes[1].CoreSymbol = nodes[0].CoreSymbol + nodes[1].CoreSymbol;
                        nodes.RemoveAt(0);
                    }

                    int nodesAmount = nodes.Count;
                    for (int i = 0; i < nodesAmount; i++)
                    {
                        if (nodes[i].CoreSymbol == "*" || nodes[i].CoreSymbol == "/")
                        {
                            if (i > 0 && i < nodes.Count - 1)
                            {
                                SetLeftAndRightNodes(nodes, i);
                                i--;
                                nodesAmount -= 2;
                            }
                            else
                            {
                                throw new Exception("Начинается или заканчиваеся на знак умножения или деления");
                            }
                        }
                    }

                    for (int i = 0; i < nodesAmount; i++)
                    {
                        if (nodes[i].CoreSymbol == "+" || nodes[i].CoreSymbol == "-")
                        {
                            if (i > 0 && i < nodes.Count - 1)
                            {
                                SetLeftAndRightNodes(nodes, i);
                                i--;
                                nodesAmount -= 2;
                            }
                            else
                            {
                                throw new Exception("Начинается или заканчиваеся на знак + или -");
                            }
                        }
                    }

                    float result = ComputeNodeValue(nodes[0]);
                    return result.ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
                else
                {
                    throw new Exception("Неверно написано выражение");
                }
            }
            else
            {
                throw new Exception("Неверное расположение скобок");
            }
        }

        private static string DeleteDuplicatedPlusesAndMinuses(string expression)
        {
            expression = DeepReplace(expression, @"([+]\s*[-])|([-]\s*[+])", "-");
            expression = DeepReplace(expression, @"[+]\s*[+]", "+");
            expression = DeepReplace(expression, @"[-]\s*[-]", "+");

            return expression;
        }

        private static string DeepReplace(string expression, string fromPattern, string toPattern)
        {
            while (Regex.IsMatch(expression, fromPattern))
            {
                expression = Regex.Replace(expression, fromPattern, toPattern);
            }

            return expression;
        }

        private static float ComputeNodeValue(Node node)
        {
            switch (node.CoreSymbol)
            {
                case "*":
                    return Multiply(ComputeNodeValue(node.LeftNode).ToString(CultureInfo.InvariantCulture.NumberFormat),
                        ComputeNodeValue(node.RightNode).ToString(CultureInfo.InvariantCulture.NumberFormat));
                case "/":
                    return ComputeNodeValue(node.LeftNode) / ComputeNodeValue(node.RightNode);
                case "+":
                    return Summ(ComputeNodeValue(node.LeftNode).ToString(CultureInfo.InvariantCulture.NumberFormat),
                        ComputeNodeValue(node.RightNode).ToString(CultureInfo.InvariantCulture.NumberFormat));
                case "-":
                    return Summ(ComputeNodeValue(node.LeftNode).ToString(CultureInfo.InvariantCulture.NumberFormat),
                        "-" + ComputeNodeValue(node.RightNode).ToString(CultureInfo.InvariantCulture.NumberFormat));
                default:
                    return float.Parse(node.CoreSymbol, CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        static void SetLeftAndRightNodes(List<Node> nodes, int index)
        {
            nodes[index].LeftNode = nodes[index - 1];
            nodes[index].RightNode = nodes[index + 1];
            nodes.RemoveAt(index + 1);
            nodes.RemoveAt(index - 1);
        }

        static float Multiply(string num1, string num2)
        {
            float result = float.Parse(num1, CultureInfo.InvariantCulture.NumberFormat) * float.Parse(num2, CultureInfo.InvariantCulture.NumberFormat);
            if (num1.Contains('.') || num2.Contains('.'))
            {
                int simbolsAfterCommaAmount = Math.Clamp((num1.Length - 1 - num1.IndexOf('.')) + (num2.Length - 1 - num2.IndexOf('.')), 0, 8);
                result = (float)Math.Round(result, simbolsAfterCommaAmount);
            }

            return result;
        }

        static float Summ(string num1, string num2)
        {
            float result = float.Parse(num1, CultureInfo.InvariantCulture.NumberFormat) + float.Parse(num2, CultureInfo.InvariantCulture.NumberFormat);
            if (num1.Contains('.') || num2.Contains('.'))
            {
                int num1SimbolsAfterComma = (num1.Length - 1 - num1.IndexOf('.'));
                int num2SimbolsAfterComma = (num2.Length - 1 - num2.IndexOf('.'));
                int simbolsAfterCommaMax = num1SimbolsAfterComma > num2SimbolsAfterComma ? num1SimbolsAfterComma : num2SimbolsAfterComma;
                result = (float)Math.Round(result, simbolsAfterCommaMax);
            }

            return result;
        }
    }
}
