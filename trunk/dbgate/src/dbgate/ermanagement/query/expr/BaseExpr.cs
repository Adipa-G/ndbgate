using System;
using dbgate.ermanagement.query.expr.segments;

namespace dbgate.ermanagement.query.expr
{
    public class BaseExpr<T> where T : BaseExpr<T>
    {
        public ISegment RootSegment { get; private set; }

        protected T BaseField(string field)
        {
            var segment = new FieldSegment(field);
            return AddSegment(segment);
        }

        protected T BaseField(string field,string alias)
        {
            var segment = new FieldSegment(field,alias);
            return AddSegment(segment);
        }

        protected T BaseField(Type entityType, string field)
        {
            var segment = new FieldSegment(entityType,field);
            return AddSegment(segment);
        }

        protected T BaseField(Type entityType, string field, string alias)
        {
            var segment = new FieldSegment(entityType,field,alias);
            return AddSegment(segment);
        }

        protected T BaseField(Type entityType, string typeAlias, string field, string alias)
        {
            var segment = new FieldSegment(entityType,typeAlias,field,alias);
            return AddSegment(segment);
        }

        protected T BaseValues(ColumnType type, params object[] values)
        {
            var segment = new ValueSegment(type,values);
            return AddSegment(segment);
        }

        protected T BaseQuery(ISelectionQuery query)
        {
            return BaseQuery(query,null);
        }

        protected T BaseQuery(ISelectionQuery query, string alias)
        {
            var segment = new QuerySegment(query,alias);
            return AddSegment(segment);
        }

        protected T BaseSum()
        {
            var segment = new GroupFunctionSegment(GroupFunctionSegmentMode.Sum);
            return AddSegment(segment);
        }

        protected T BaseCount()
        {
            var segment = new GroupFunctionSegment(GroupFunctionSegmentMode.Count);
            return AddSegment(segment);
        }

        protected T BaseCustFunc(string func)
        {
            var segment = new GroupFunctionSegment(func);
            return AddSegment(segment);
        }

        protected T BaseEq()
        {
            var segment = new CompareSegment(CompareSegmentMode.Eq);
            return AddSegment(segment);
        }

        protected T BaseGe()
        {
            var segment = new CompareSegment(CompareSegmentMode.Ge);
            return AddSegment(segment);
        }

        protected T BaseGt()
        {
            var segment = new CompareSegment(CompareSegmentMode.Gt);
            return AddSegment(segment);
        }

        protected T BaseLe()
        {
            var segment = new CompareSegment(CompareSegmentMode.Le);
            return AddSegment(segment);
        }

        protected T BaseLt()
        {
            var segment = new CompareSegment(CompareSegmentMode.Lt);
            return AddSegment(segment);
        }

        protected T BaseNeq()
        {
            var segment = new CompareSegment(CompareSegmentMode.Neq);
            return AddSegment(segment);
        }

        protected T BaseLike()
        {
            var segment = new CompareSegment(CompareSegmentMode.Like);
            return AddSegment(segment);
        }

        protected T BaseBetween()
        {
            var segment = new CompareSegment(CompareSegmentMode.Between);
            return AddSegment(segment);
        }

        protected T BaseIn()
        {
            var segment = new CompareSegment(CompareSegmentMode.In);
            return AddSegment(segment);
        }

        protected T BaseExists()
        {
            var segment = new CompareSegment(CompareSegmentMode.Exists);
            return AddSegment(segment);
        }

        protected T BaseNotExists()
        {
            var segment = new CompareSegment(CompareSegmentMode.NotExists);
            return AddSegment(segment);
        }

        protected T BaseAnd(params T[] expressions)
        {
            MergeSegment mergeSegment;
            if (expressions == null || expressions.Length == 0)
            {
                mergeSegment = new MergeSegment(MergeSegmentMode.And);
                return AddSegment(mergeSegment);
            }

            mergeSegment = new MergeSegment(MergeSegmentMode.ParaAnd);
            foreach (T expression in expressions)
            {
                mergeSegment.AddSub(expression.RootSegment);
            }
            return AddSegment(mergeSegment);
        }

        protected T BaseOr(params T[] expressions)
        {
            MergeSegment mergeSegment;
            if (expressions == null || expressions.Length == 0)
            {
                mergeSegment = new MergeSegment(MergeSegmentMode.Or);
                return AddSegment(mergeSegment);
            }

            mergeSegment = new MergeSegment(MergeSegmentMode.ParaOr);
            foreach (T expression in expressions)
            {
                mergeSegment.AddSub(expression.RootSegment);
            }
            return AddSegment(mergeSegment);
        }

        private T AddSegment(ISegment segment)
        {
            RootSegment = RootSegment == null
                               ? segment
                               : RootSegment.Add(segment);
            return (T)this;
        }
    }
}
