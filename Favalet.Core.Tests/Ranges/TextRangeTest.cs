/////////////////////////////////////////////////////////////////////////////////////////////////
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

using System;
using NUnit.Framework;

using static Favalet.Tokens.Token;

namespace Favalet.Ranges
{
    [TestFixture]
    public sealed class TextRangeTest
    {
        [Test]
        public void ToString1()
        {
            var range = TextRange(TextPosition.Create(1, 2), TextPosition.Create(3, 4));

            Assert.AreEqual("(1,2,3,4)", range.ToString());
        }

        [Test]
        public void ToString2()
        {
            var range = TextRange(TextPosition.Create(1, 2));

            Assert.AreEqual("(1,2)", range.ToString());
        }

        [Test]
        public void ToString3()
        {
            var text = new Uri("foo.fv", UriKind.RelativeOrAbsolute);
            var range = TextRange(text, TextPosition.Create(1, 2));

            Assert.AreEqual("foo.fv(1,2)", range.ToString());
        }

        [Test]
        public void ToString4()
        {
            var text = new Uri("foo.fv", UriKind.RelativeOrAbsolute);
            var range = TextRange(text, TextPosition.Create(1, 2), TextPosition.Create(3, 4));

            Assert.AreEqual("foo.fv(1,2,3,4)", range.ToString());
        }

        [Test]
        public void ToString5()
        {
            var range = TextRange("foo.fv", TextPosition.Create(1, 2));

            Assert.AreEqual("foo.fv(1,2)", range.ToString());
        }

        [Test]
        public void ToString6()
        {
            var range = TextRange("foo.fv", TextPosition.Create(1, 2), TextPosition.Create(3, 4));

            Assert.AreEqual("foo.fv(1,2,3,4)", range.ToString());
        }

        [Test]
        public void Contains1()
        {
            var outer = TextRange(TextPosition.Create(1, 1), TextPosition.Create(10, 10));
            var inner = TextRange(TextPosition.Create(5, 5));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains2()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(2, 2));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains3()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(9, 8));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains4()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(1, 4));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains5()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(10, 6));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains6()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(1, 3));

            Assert.IsFalse(outer.Contains(inner));
        }

        [Test]
        public void Contains7()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(10, 7));

            Assert.IsFalse(outer.Contains(inner));
        }

        [Test]
        public void Contains8()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 4));

            Assert.IsFalse(outer.Contains(inner));
        }

        [Test]
        public void Contains9()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(10, 6), TextPosition.Create(10, 7));

            Assert.IsFalse(outer.Contains(inner));
        }

        [Test]
        public void Contains10()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(1, 4), TextPosition.Create(1, 5));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains11()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(10, 5), TextPosition.Create(10, 6));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains12()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(2, 1), TextPosition.Create(9, 11));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Contains13()
        {
            var outer = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));
            var inner = TextRange(TextPosition.Create(1, 4), TextPosition.Create(10, 6));

            Assert.IsTrue(outer.Contains(inner));
        }

        [Test]
        public void Overlaps1()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 5));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps2()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 2));

            Assert.IsFalse(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps3()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 9));

            Assert.IsFalse(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps4()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 3));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps5()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 8));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps6()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps7()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 1), TextPosition.Create(1, 4));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps8()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 6), TextPosition.Create(1, 9));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps9()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 2), TextPosition.Create(1, 9));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

        [Test]
        public void Overlaps10()
        {
            var lhs = TextRange(TextPosition.Create(1, 3), TextPosition.Create(1, 8));
            var rhs = TextRange(TextPosition.Create(1, 4), TextPosition.Create(1, 7));

            Assert.IsTrue(lhs.Overlaps(rhs));
        }

#if !NET40
        [Test]
        public void Tuple1()
        {
            TextRange range = (1, 2);

            Assert.AreEqual("(1,2)", range.ToString());
        }

        [Test]
        public void Tuple2()
        {
            TextRange range = (1, 2, 3, 4);

            Assert.AreEqual("(1,2,3,4)", range.ToString());
        }

        [Test]
        public void Tuple3()
        {
            var text = new Uri("foo.fv", UriKind.RelativeOrAbsolute);
            TextRange range = (text, 1, 2);

            Assert.AreEqual("foo.fv(1,2)", range.ToString());
        }

        [Test]
        public void Tuple4()
        {
            var text = new Uri("foo.fv", UriKind.RelativeOrAbsolute);
            TextRange range = (text, 1, 2, 3, 4);

            Assert.AreEqual("foo.fv(1,2,3,4)", range.ToString());
        }

        [Test]
        public void Tuple5()
        {
            TextRange range = ("foo.fv", 1, 2);

            Assert.AreEqual("foo.fv(1,2)", range.ToString());
        }

        [Test]
        public void Tuple6()
        {
            TextRange range = ("foo.fv", 1, 2, 3, 4);

            Assert.AreEqual("foo.fv(1,2,3,4)", range.ToString());
        }
#endif
    }
}
