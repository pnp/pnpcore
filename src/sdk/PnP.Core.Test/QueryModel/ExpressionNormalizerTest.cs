using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System;
using System.Linq.Expressions;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class ExpressionNormalizerTest
    {
        [TestMethod]
        public void SimpleBoolTest()
        {
            Expression<Func<List, bool>> initial = l => l.Hidden;
            Expression<Func<List, bool>> expected = l => l.Hidden == true;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void SimpleBoolSameTest()
        {
            Expression<Func<List, bool>> initial = l => l.Hidden == true;
            Expression<Func<List, bool>> expected = l => l.Hidden == true;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void SimpleBoolFalseTest()
        {
            Expression<Func<List, bool>> initial = l => l.Hidden;
            Expression<Func<List, bool>> expected = l => l.Hidden == false;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsFalse(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void SimpleNotBoolTest()
        {
            Expression<Func<List, bool>> initial = l => !l.Hidden;
            Expression<Func<List, bool>> expected = l => l.Hidden == false;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void ComplexBoolTest()
        {
            Expression<Func<List, bool>> initial = l => l.Hidden && l.Title == "Test";
            Expression<Func<List, bool>> expected = l => l.Hidden == true && l.Title == "Test";

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void ComplexBoolNotTest()
        {
            Expression<Func<List, bool>> initial = l => !l.Hidden && l.Title == "Test";
            Expression<Func<List, bool>> expected = l => l.Hidden == false && l.Title == "Test";

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void ComplexBoolNotReversTest()
        {
            Expression<Func<List, bool>> initial = l => l.Title == "Test" && !l.Hidden;
            Expression<Func<List, bool>> expected = l => l.Title == "Test" && l.Hidden == false;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void SimpleMethodTest()
        {
            Expression<Func<List, bool>> initial = l => l.Title.Contains("Test");
            Expression<Func<List, bool>> expected = l => l.Title.Contains("Test") == true;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void ComplexMethodTest()
        {
            Expression<Func<List, bool>> initial = l => l.Title.Contains("Test") && l.Title == "Test";
            Expression<Func<List, bool>> expected = l => l.Title.Contains("Test") == true && l.Title == "Test";

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }

        [TestMethod]
        public void ComplexMethodChainTest()
        {
            Expression<Func<List, bool>> initial = l => l.Title.Contains("Test") && l.Title == "Test" && !l.Hidden && l.Title.StartsWith("Value");
            Expression<Func<List, bool>> expected = l => l.Title.Contains("Test") == true && l.Title == "Test" && l.Hidden == false && l.Title.StartsWith("Value") == true;

            var actual = (Expression<Func<List, bool>>)new ExpressionNormalizer(initial).Normalize();

            Assert.IsTrue(LambdaCompare.Eq(actual, expected));
        }
    }

    // simplified version from here - https://stackoverflow.com/a/24528357/434967
    internal static class LambdaCompare
    {
        public static bool Eq<TSource, TValue>(
            Expression<Func<TSource, TValue>> x,
            Expression<Func<TSource, TValue>> y)
        {
            return ExpressionsEqual(x, y);
        }

        private static bool ExpressionsEqual(Expression x, Expression y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            if (x.NodeType != y.NodeType
                || x.Type != y.Type)
            {
                return false;
            }

            if (x is LambdaExpression)
            {
                var lx = (LambdaExpression)x;
                var ly = (LambdaExpression)y;
                return ExpressionsEqual(lx.Body, ly.Body);
            }
            if (x is MemberExpression)
            {
                var mex = (MemberExpression)x;
                var mey = (MemberExpression)y;
                return Equals(mex.Member, mey.Member) && ExpressionsEqual(mex.Expression, mey.Expression);
            }
            if (x is BinaryExpression)
            {
                var bx = (BinaryExpression)x;
                var by = (BinaryExpression)y;
                return bx.Method == @by.Method && ExpressionsEqual(bx.Left, @by.Left) &&
                       ExpressionsEqual(bx.Right, @by.Right);
            }
            if (x is UnaryExpression)
            {
                var ux = (UnaryExpression)x;
                var uy = (UnaryExpression)y;
                return ux.Method == uy.Method && ExpressionsEqual(ux.Operand, uy.Operand);
            }

            if (x is MethodCallExpression)
            {
                var cx = (MethodCallExpression)x;
                var cy = (MethodCallExpression)y;
                return cx.Method == cy.Method
                       && ExpressionsEqual(cx.Object, cy.Object);
            }

            if (x is ParameterExpression)
            {
                var px = (ParameterExpression)x;
                var py = (ParameterExpression)y;
                return px.Name.Equals(py.Name);
            }

            if(x is ConstantExpression)
            {
                var cx = (ConstantExpression)x;
                var cy = (ConstantExpression)y;

                return cx.Value.Equals(cy.Value);
            }

            throw new NotImplementedException(x.ToString());
        }
    }

}
