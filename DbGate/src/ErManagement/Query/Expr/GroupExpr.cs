using System;

namespace DbGate.ErManagement.Query.Expr
{
    public class GroupExpr : BaseExpr<GroupExpr>
    {
        public GroupExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public static GroupExpr Build()
        {
            return new GroupExpr();
        }
    }
}