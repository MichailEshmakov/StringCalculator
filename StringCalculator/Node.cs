using System;
using System.Collections.Generic;
using System.Text;

namespace StringCalculator
{
    class Node
    {
        public string CoreSymbol;       
        public Node LeftNode;
        public Node RightNode;

        public Node(string coreSymbol)
        {
            CoreSymbol = coreSymbol;
        }
    }
}
