using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition
{
    public interface IAbstractCondition : IQueryCondition
    {
        string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}