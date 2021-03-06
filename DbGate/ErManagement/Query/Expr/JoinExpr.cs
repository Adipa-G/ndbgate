﻿using System;
using System.Linq.Expressions;

namespace DbGate.ErManagement.Query.Expr
{
    public class JoinExpr : BaseExpr<JoinExpr>
    {
        public JoinExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public JoinExpr Field(Type entityType, string typeAlias, string field)
        {
            return BaseField(entityType, typeAlias, field, null);
        }

        public JoinExpr Field<T>(Expression<Func<T, object>> prop)
        {
            return BaseField(prop);
        }

        public JoinExpr Field<T>(Expression<Func<T, object>> prop, string typeAlias)
        {
            return BaseField(prop, typeAlias, null);
        }

        public JoinExpr Eq()
        {
            return BaseEq();
        }

        public JoinExpr And(params JoinExpr[] expressions)
        {
            return BaseAnd(expressions);
        }

        public JoinExpr Or(params JoinExpr[] expressions)
        {
            return BaseOr(expressions);
        }

        public static JoinExpr Build()
        {
            return new JoinExpr();
        }
    }
}