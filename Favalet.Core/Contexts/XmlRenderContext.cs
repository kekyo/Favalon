using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Favalet.Contexts
{
    public interface IXmlRenderContext
    {
        XElement GetXml(IExpression expression);
    }

    public sealed class XmlRenderContext :
        IXmlRenderContext
    {
        [DebuggerStepThrough]
        private XmlRenderContext()
        {
        }

        private XObject GetXmlHigherOrder(IExpression higherOrder) =>
            this.GetXml(higherOrder) switch
            {
                XElement element when !(element.Nodes().Any() || element.Attributes().Any()) =>
                    new XAttribute("higherOrder", element.Name),
                XElement element =>
                    new XElement("HigherOrder", element)
            };

        public XElement GetXml(IExpression expression) =>
            (expression, expression.HigherOrder) switch
            {
                (Expression expr, DeadEndTerm _) =>
                    new XElement(
                        expr.Type,
                        expr.InternalGetXmlValues(this).
                        Cast<object>().
                        ToArray()),
                (Expression expr, _) =>
                    new XElement(
                        expr.Type,
                        expr.InternalGetXmlValues(this).
                        Cast<object>().
                        Append(this.GetXmlHigherOrder(expr.HigherOrder)).
                        ToArray()),
                _ => new XElement(expression.Type)
            };

        [DebuggerStepThrough]
        public static XmlRenderContext Create() =>
            new XmlRenderContext();
    }
}
