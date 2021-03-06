﻿/////////////////////////////////////////////////////////////////////////////////////////////////
//
// Favalon - An Interactive Shell Based on a Typed Lambda Calculus.
// Copyright (c) 2018-2020 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

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
                        Memoize()),
                (Expression expr, _) =>
                    new XElement(
                        expr.Type,
                        expr.InternalGetXmlValues(this).
                        Cast<object>().
                        Append(this.GetXmlHigherOrder(expr.HigherOrder)).
                        Memoize()),
                _ => new XElement(expression.Type)
            };

        [DebuggerStepThrough]
        public static XmlRenderContext Create() =>
            new XmlRenderContext();
    }
}
