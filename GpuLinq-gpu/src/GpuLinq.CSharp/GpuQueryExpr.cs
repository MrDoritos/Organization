﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nessos.LinqOptimizer.Core;
using Nessos.GpuLinq.Core;
using Nessos.GpuLinq.Base;

namespace Nessos.GpuLinq.CSharp
{
    /// <summary>
    /// Provides a set of static methods for querying objects that implement IGpuQueryExpr.
    /// </summary>
    public static class GpuQueryExpr
    {
        #region Expression<Func<_>>.Invoke
        /// <summary>
        /// Dummy Invoke call (Required for splicing expression trees)
        /// </summary>
        public static R Invoke<T, R>(this Expression<Func<T, R>> f, T arg)
        {
            throw new NotSupportedException("Callable only from inside a gpu kernel");
        }

        /// <summary>
        /// Dummy Invoke call (Required for splicing expression trees)
        /// </summary>
        public static R Invoke<T1, T2, R>(this Expression<Func<T1, T2, R>> f, T1 arg1, T2 arg2)
        {
            throw new NotSupportedException("Callable only from inside a gpu kernel");
        }

        /// <summary>
        /// Dummy Invoke call (Required for splicing expression trees)
        /// </summary>
        public static R Invoke<T1, T2, T3, R>(this Expression<Func<T1, T2, T3, R>> f, T1 arg1, T2 arg2, T3 arg3)
        {
            throw new NotSupportedException("Callable only from inside a gpu kernel");
        }

        /// <summary>
        /// Dummy Invoke call (Required for splicing expression trees)
        /// </summary>
        public static R Invoke<T1, T2, T3, T4, R>(this Expression<Func<T1, T2, T3, T4, R>> f, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            throw new NotSupportedException("Callable only from inside a gpu kernel");
        }

        /// <summary>
        /// Dummy Invoke call (Required for splicing expression trees)
        /// </summary>
        public static R Invoke<T1, T2, T3, T4, T5, R>(this Expression<Func<T1, T2, T3, T4, T5, R>> f, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            throw new NotSupportedException("Callable only from inside a gpu kernel");
        }
        #endregion

        /// <summary>
        /// Enables a gpu query.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source array.</typeparam>
        /// <param name="source">An array to convert to an IGpuQueryExpr.</param>
        /// <returns>A query that returns the elements of the source array.</returns>
        public static IGpuQueryExpr<IGpuArray<TSource>> AsGpuQueryExpr<TSource>(this IGpuArray<TSource> source) 
        {
            return new GpuQueryExpr<IGpuArray<TSource>>(QueryExpr.NewSource(Expression.Constant(source), typeof(TSource), QueryExprType.Gpu));
        }

        #region Combinators
        /// <summary>
        /// Creates a new query that projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the query.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="query">A query whose values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A query whose elements will be the result of invoking the transform function on each element of source.</returns>
        public static IGpuQueryExpr<IGpuArray<TResult>> Select<TSource, TResult>(this IGpuQueryExpr<IGpuArray<TSource>> query, Expression<Func<TSource, TResult>> selector) 
        {
            return new GpuQueryExpr<IGpuArray<TResult>>(QueryExpr.NewTransform(selector, query.Expr));
        }

        /// <summary>
        /// Creates a query that projects each element of a sequence to an GpuArray, flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TCol">The type of the intermediate elements collected by collectionSelector.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by selector.</typeparam>
        /// <param name="query">A query whose values to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>A query whose elements are the result of invoking the one-to-many transform function on each element of the input sequence and the result selector function on each element therein.</returns>
        public static IGpuQueryExpr<IGpuArray<TResult>> SelectMany<TSource, TCol, TResult>(this IGpuQueryExpr<IGpuArray<TSource>> query, Expression<Func<TSource, IGpuArray<TCol>>> collectionSelector, Expression<Func<TSource, TCol, TResult>> resultSelector)
        {
            var paramExpr = collectionSelector.Parameters.Single();
            var bodyExpr = collectionSelector.Body;
            if (bodyExpr.NodeType == ExpressionType.MemberAccess )
            {
                var nested = Tuple.Create(paramExpr, QueryExpr.NewSource((MemberExpression)bodyExpr, typeof(TCol), QueryExprType.Gpu));
                return new GpuQueryExpr<IGpuArray<TResult>>(QueryExpr.NewNestedQueryTransform(nested, resultSelector, query.Expr));
            }
            else
            { 
                throw new InvalidOperationException("Not supported " + bodyExpr.ToString());
            }
        }

        /// <summary>
        /// Creates a new query that filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="query">An query whose values to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A query that contains elements from the input query that satisfy the condition.</returns>
        public static IGpuQueryExpr<IGpuArray<TSource>> Where<TSource>(this IGpuQueryExpr<IGpuArray<TSource>> query, Expression<Func<TSource, bool>> predicate) 
        {
            return new GpuQueryExpr<IGpuArray<TSource>>(QueryExpr.NewFilter(predicate, query.Expr));
        }

        /// <summary>
        /// Creates a new query that computes the sum of a sequence of int values.
        /// </summary>
        /// <param name="query">A query whose sequence of int values to calculate the sum of.</param>
        /// <returns>A query that returns the sum of the values in the gpu array.</returns>
        public static IGpuQueryExpr<int> Sum(this IGpuQueryExpr<IGpuArray<int>> query) 
        {
            return new GpuQueryExpr<int>(QueryExpr.NewSum(query.Expr));
        }

        /// <summary>
        /// Creates a new query that computes the sum of a sequence of long values.
        /// </summary>
        /// <param name="query">A query whose sequence of int values to calculate the sum of.</param>
        /// <returns>A query that returns the sum of the values in the gpu array.</returns>
        public static IGpuQueryExpr<long> Sum(this IGpuQueryExpr<IGpuArray<long>> query)
        {
            return new GpuQueryExpr<long>(QueryExpr.NewSum(query.Expr));
        }

        /// <summary>
        /// Creates a new query that computes the sum of a sequence of float values.
        /// </summary>
        /// <param name="query">A query whose sequence of int values to calculate the sum of.</param>
        /// <returns>A query that returns the sum of the values in the gpu array.</returns>
        public static IGpuQueryExpr<float> Sum(this IGpuQueryExpr<IGpuArray<float>> query)
        {
            return new GpuQueryExpr<float>(QueryExpr.NewSum(query.Expr));
        }

        /// <summary>
        /// Creates a new query that computes the sum of a sequence of double values.
        /// </summary>
        /// <param name="query">A query whose sequence of int values to calculate the sum of.</param>
        /// <returns>A query that returns the sum of the values in the gpu array.</returns>
        public static IGpuQueryExpr<double> Sum(this IGpuQueryExpr<IGpuArray<double>> query)
        {
            return new GpuQueryExpr<double>(QueryExpr.NewSum(query.Expr));
        }

        /// <summary>
        /// Creates a new query that returns the number of elements in a gpu array.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="query">A query whose elements will be count.</param>
        /// <returns>A query that returns the number of elements in the gpu array.</returns>
        public static IGpuQueryExpr<int> Count<TSource>(this IGpuQueryExpr<IGpuArray<TSource>> query) 
        {
            return new GpuQueryExpr<int>(QueryExpr.NewCount(query.Expr));
        }

        /// <summary>
        /// A query that returns an array from an gpu array.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="query">The query to create an array from.</param>
        /// <returns>A query that contains elements from the gpu array in an array form.</returns>
        public static IGpuQueryExpr<TSource[]> ToArray<TSource>(this IGpuQueryExpr<IGpuArray<TSource>> query) 
        {
            return new GpuQueryExpr<TSource[]>(QueryExpr.NewToArray(query.Expr));
        }


        /// <summary>
        /// Creates a query that applies a specified function to the corresponding elements of two gpu arrays, producing a sequence of the results.
        /// </summary>
        /// <param name="first">The first gpu array to merge.</param>
        /// <param name="second">The first gpu array to merge.</param>
        /// <param name="resultSelector">A function that specifies how to merge the elements from the two gpu arrays.</param>
        /// <returns>A query that contains merged elements of two gpu arrays.</returns>
        public static IGpuQueryExpr<IGpuArray<TResult>> Zip<TFirst, TSecond, TResult>(IGpuArray<TFirst> first, IGpuArray<TSecond> second, Expression<Func<TFirst, TSecond, TResult>> resultSelector)
        {
            return new GpuQueryExpr<IGpuArray<TResult>>(QueryExpr.NewZipWith(Expression.Constant(first), Expression.Constant(second), resultSelector));
        }

        #endregion
    }
}
