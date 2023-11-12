using Assimp;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers.ExpressionNodeSpec;
using Org.BouncyCastle.Pqc.Crypto.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class FormatExpressionNodesUsecase
    {
        public IEnumerable<IndexedToken> ToTree(int nodeId, Func<int, ExpressionNode?> getNode)
        {
            var node = getNode(nodeId);
            if (node != null)
            {
                IEnumerable<IndexedToken> Func0(string funcName) =>
                    new IndexedToken[] { new IndexedToken($"{funcName}()", nodeId) };

                IEnumerable<IndexedToken> Func1(string funcName, int car) =>
                    new IndexedToken[] { new IndexedToken($"{funcName}(", nodeId) }
                        .Concat(ToTree(car, getNode))
                        .Concat(new IndexedToken[] { new IndexedToken(")", nodeId) });

                IEnumerable<IndexedToken> Func2(string funcName, int car, int cdr) =>
                    new IndexedToken[] { new IndexedToken($"{funcName}(", nodeId) }
                        .Concat(ToTree(car, getNode))
                        .Concat(new IndexedToken[] { new IndexedToken(", ", nodeId) })
                        .Concat(ToTree(cdr, getNode))
                        .Concat(new IndexedToken[] { new IndexedToken(")", nodeId) });

                IEnumerable<IndexedToken> Mid2(string mark, int car, int cdr) =>
                    new IndexedToken[] { new IndexedToken($"(", nodeId) }
                        .Concat(ToTree(car, getNode))
                        .Concat(new IndexedToken[] { new IndexedToken($" {mark} ", nodeId) })
                        .Concat(ToTree(cdr, getNode))
                        .Concat(new IndexedToken[] { new IndexedToken(")", nodeId) });

                switch ((ExpressionType)node.Type)
                {
                    default:
                        return Func0($"type{node.Type}");
                    case ExpressionType.CONSTANT_NUM:
                        return new IndexedToken[] { new IndexedToken(node.Value.ToString(), nodeId) };
                    case ExpressionType.ELEMENT_NAME:
                        return new IndexedToken[] { new IndexedToken($"ElementName({node.Element})", nodeId) };
                    case ExpressionType.FCURVE_ETRNX:
                        return Func0("FCurveExternX");
                    case ExpressionType.FCURVE_ETRNY:
                        return Func0("FCurveExternY");
                    case ExpressionType.FCURVE_ETRNZ:
                        return Func0("FCurveExternZ");
                    case ExpressionType.FCURVE_ROTX:
                        return Func0("FCurveRotationX");
                    case ExpressionType.FCURVE_ROTY:
                        return Func0("FCurveRotationY");
                    case ExpressionType.FCURVE_ROTZ:
                        return Func0("FCurveRotationZ");
                    case ExpressionType.FCURVE_SCALX:
                        return Func0("FCurveScaleX");
                    case ExpressionType.FCURVE_SCALY:
                        return Func0("FCurveScaleY");
                    case ExpressionType.FCURVE_SCALZ:
                        return Func0("FCurveScaleZ");
                    case ExpressionType.FUNC_ABS:
                        return Func1("abs", node.CAR);
                    case ExpressionType.FUNC_ACOS:
                        return Func1("acos", node.CAR);
                    case ExpressionType.FUNC_ASIN:
                        return Func1("asin", node.CAR);
                    case ExpressionType.FUNC_ATAN:
                        return Func1("atan", node.CAR);
                    case ExpressionType.FUNC_AT_FRAME:
                        return Func0("atFrame");
                    case ExpressionType.FUNC_AT_FRAME_ROT:
                        return Func0("atFrameRotation");
                    case ExpressionType.FUNC_AV:
                        return Func0("av");
                    case ExpressionType.FUNC_COND:
                        return Func1("cond", node.CAR);
                    case ExpressionType.FUNC_COS:
                        return Func1("cos", node.CAR);
                    case ExpressionType.FUNC_CTR_DIST:
                        return Func2("ctrDistance", node.CAR, node.CDR);
                    case ExpressionType.FUNC_EXP:
                        return Func2("exp", node.CAR, node.CDR);
                    case ExpressionType.FUNC_FMOD:
                        return Func2("fmod", node.CAR, node.CDR);
                    case ExpressionType.FUNC_LOG:
                        return Func2("log", node.CAR, node.CDR);
                    case ExpressionType.FUNC_MAX:
                        return Func2("max", node.CAR, node.CDR);
                    case ExpressionType.FUNC_MIN:
                        return Func2("min", node.CAR, node.CDR);
                    case ExpressionType.FUNC_POW:
                        return Func2("pow", node.CAR, node.CDR);
                    case ExpressionType.FUNC_SIN:
                        return Func1("sin", node.CAR);
                    case ExpressionType.FUNC_SQRT:
                        return Func1("sqrt", node.CAR);
                    case ExpressionType.FUNC_TAN:
                        return Func1("tan", node.CAR);
                    case ExpressionType.LIST:
                        return Func2("list", node.CAR, node.CDR);
                    case ExpressionType.OP_AND:
                        return Mid2("&", node.CAR, node.CDR);
                    case ExpressionType.OP_DIV:
                        return Mid2("/", node.CAR, node.CDR);
                    case ExpressionType.OP_EQ:
                        return Mid2("==", node.CAR, node.CDR);
                    case ExpressionType.OP_GE:
                        return Mid2(">=", node.CAR, node.CDR);
                    case ExpressionType.OP_GT:
                        return Mid2(">", node.CAR, node.CDR);
                    case ExpressionType.OP_LE:
                        return Mid2("<=", node.CAR, node.CDR);
                    case ExpressionType.OP_LT:
                        return Mid2("<", node.CAR, node.CDR);
                    case ExpressionType.OP_MINUS:
                        return Mid2("-", node.CAR, node.CDR);
                    case ExpressionType.OP_MOD:
                        return Mid2("%", node.CAR, node.CDR);
                    case ExpressionType.OP_MUL:
                        return Mid2("*", node.CAR, node.CDR);
                    case ExpressionType.OP_OR:
                        return Mid2("|", node.CAR, node.CDR);
                    case ExpressionType.OP_PLUS:
                        return Mid2("+", node.CAR, node.CDR);
                    case ExpressionType.VARIABLE_FC:
                        return Func0("var");
                }
            }
            else
            {
                return new IndexedToken[] { new IndexedToken("null", -1) };
            }
        }
    }
}
