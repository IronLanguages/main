using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;
using IronPython.Compiler;

namespace Microsoft.PyAnalysis.Values {
    class NumericInstanceInfo : BuiltinInstanceInfo {

        public NumericInstanceInfo(BuiltinClassInfo klass)
            : base(klass) {
        }

        public override ISet<Namespace> BinaryOperation(Node node, AnalysisUnit unit, PythonOperator operation, ISet<Namespace> rhs) {
            switch (operation) {
                case PythonOperator.GreaterThan:
                case PythonOperator.LessThan:
                case PythonOperator.LessThanOrEqual:
                case PythonOperator.GreaterThanOrEqual:
                case PythonOperator.Equal:
                case PythonOperator.NotEqual:
                case PythonOperator.Is:
                case PythonOperator.IsNot:
                    return ProjectState._boolType.Instance;
            }
            return base.BinaryOperation(node, unit, operation, rhs);
        }
    }
}
