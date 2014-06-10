using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.query.expr.segments;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query
{
    public class AbstractExpressionProcessor
    {
        private string AppendAlias(string sql, FieldSegment fieldSegment)
        {
            if (!string.IsNullOrEmpty(fieldSegment.Alias))
            {
                return sql + " AS " + fieldSegment.Alias + " ";
            }
            return sql;
        }

        public IDbColumn GetColumn(FieldSegment segment)
        {
            ICollection<IDbColumn> columns = null;
            try
            {
                columns = CacheManager.FieldCache.GetDbColumns(segment.EntityType);
            }
            catch (FieldCacheMissException e)
            {
                try
                {
                    CacheManager.FieldCache.Register(segment.EntityType);
                    columns = CacheManager.FieldCache.GetDbColumns(segment.EntityType);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.StackTrace);
                }
            }

            if (columns != null)
            {
                foreach (IDbColumn column in columns)
                {
                    if (column.AttributeName.Equals(segment.Field))
                    {
                        return column;
                    }
                }
            }
            return null;
        }

        public string GetFieldName(FieldSegment fieldSegment, bool withAlias, QueryBuildInfo buildInfo)
        {
            string tableAlias = buildInfo.GetAlias(fieldSegment.EntityType);
            if (!string.IsNullOrEmpty(fieldSegment.TypeAlias))
            {
                tableAlias = fieldSegment.TypeAlias;
            }
            tableAlias = (tableAlias == null) ? "" : tableAlias + ".";
            IDbColumn column = GetColumn(fieldSegment);

            if (column != null)
            {
                string sql = tableAlias + column.ColumnName;
                if (withAlias)
                {
                    sql = AppendAlias(sql, fieldSegment);
                }
                return sql;
            }
            else
            {
                return "<incorrect column for " + fieldSegment.Field + ">";
            }
        }

        public string GetGroupFunction(GroupFunctionSegment groupSegment, bool withAlias, QueryBuildInfo buildInfo)
        {
            var fieldSegment = (FieldSegment) groupSegment.SegmentToGroup;
            string sql = GetFieldName(fieldSegment, false, buildInfo);
            switch (groupSegment.GroupFunctionMode)
            {
                case GroupFunctionSegmentMode.Count:
                    sql = " COUNT(" + sql + ") ";
                    break;
                case GroupFunctionSegmentMode.Sum:
                    sql = " SUM(" + sql + ") ";
                    break;
                case GroupFunctionSegmentMode.CustFunc:
                    sql = " " + groupSegment.CustFunction + "(" + sql + ") ";
                    break;
            }
            if (withAlias)
            {
                sql = AppendAlias(sql, fieldSegment);
            }
            return sql;
        }

        public string Process(StringBuilder sb, ISegment segment, QueryBuildInfo buildInfo,IDbLayer dbLayer)
        {
            if (sb == null) sb = new StringBuilder();
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                    ProcessField(sb, (FieldSegment) segment, buildInfo);
                    break;
                case SegmentType.Group:
                    ProcessGroup(sb, (GroupFunctionSegment) segment, buildInfo);
                    break;
                case SegmentType.Value:
                    ProcessValue(sb, (ValueSegment) segment, buildInfo);
                    break;
                case SegmentType.Compare:
                    ProcessCompare(sb, (CompareSegment) segment, buildInfo,dbLayer);
                    break;
                case SegmentType.Merge:
                    ProcessMerge(sb,(MergeSegment)segment,buildInfo,dbLayer);
                    break;
                case SegmentType.Query:
                    ProcessQuery(sb,(QuerySegment)segment,buildInfo,dbLayer);
                    break;
            }
            return sb.ToString();
        }

        private void ProcessField(StringBuilder sb, FieldSegment segment, QueryBuildInfo buildInfo)
        {
            string fieldName = GetFieldName(segment, false, buildInfo);
            sb.Append(fieldName);
        }

        private void ProcessGroup(StringBuilder sb, GroupFunctionSegment segment, QueryBuildInfo buildInfo)
        {
            string groupFunction = GetGroupFunction(segment, false, buildInfo);
            sb.Append(groupFunction);
        }

        private void ProcessValue(StringBuilder sb, ValueSegment segment, QueryBuildInfo buildInfo)
        {
            sb.Append("?");

            var param = new QueryExecParam();
            param.Index = buildInfo.ExecInfo.Params.Count;
            param.Type = segment.Type;
            param.Value =segment.Values[0];
            buildInfo.ExecInfo.Params.Add(param);
        }

        private void ProcessQuery(StringBuilder sb, QuerySegment segment, QueryBuildInfo buildInfo, IDbLayer dbLayer)
        {
            buildInfo = dbLayer.DataManipulate().ProcessQuery(buildInfo, segment.Query.Structure);
            sb.Append(" ( ");
            sb.Append(buildInfo.ExecInfo.Sql);
            sb.Append(" ) ");
        }

        private void ProcessCompare(StringBuilder sb, CompareSegment segment, QueryBuildInfo buildInfo,IDbLayer dbLayer)
        {
            if (segment.Left != null)
            {
                Process(sb, segment.Left, buildInfo, dbLayer);
            }

            switch (segment.Mode)
            {
                case CompareSegmentMode.Between:
                    ProcessBetween(sb, segment, buildInfo, dbLayer);
                    return;
                case CompareSegmentMode.In:
                    switch (segment.Right.SegmentType)
                    {
                        case SegmentType.Value:
                            ProcessInValues(sb, segment, buildInfo);
                            break;
                        case SegmentType.Query:
                            ProcessQueryValues(sb, segment, buildInfo, dbLayer);
                            break;
                    }
                    return;
                case CompareSegmentMode.Exists:
                    sb.Append(" EXISTS "); break;
                case CompareSegmentMode.NotExists:
                    sb.Append(" NOT EXISTS "); break;
                case CompareSegmentMode.Eq:
                    sb.Append(" = ");
                    break;
                case CompareSegmentMode.Ge:
                    sb.Append(" >= ");
                    break;
                case CompareSegmentMode.Gt:
                    sb.Append(" > ");
                    break;
                case CompareSegmentMode.Le:
                    sb.Append(" <= ");
                    break;
                case CompareSegmentMode.Lt:
                    sb.Append(" < ");
                    break;
                case CompareSegmentMode.Like:
                    sb.Append(" like ");
                    break;
                case CompareSegmentMode.Neq:
                    sb.Append(" <> ");
                    break;
                default:
                    break;
            }
            Process(sb, segment.Right, buildInfo,dbLayer);
        }

        private void ProcessBetween(StringBuilder sb, CompareSegment segment, QueryBuildInfo buildInfo, IDbLayer dbLayer)
        {
            sb.Append(" BETWEEN ? AND ? ");
            ValueSegment valueSegment = (ValueSegment)segment.Right;
            object[] values = valueSegment.Values;
            for (int i = 0, valuesLength = 2; i < valuesLength; i++)
            {
                Object value = values[i];
                QueryExecParam param = new QueryExecParam();
                param.Index = buildInfo.ExecInfo.Params.Count;
                param.Type = valueSegment.Type;
                param.Value = value;
                buildInfo.ExecInfo.Params.Add(param);
            }
        }

        private void ProcessInValues(StringBuilder sb, CompareSegment segment, QueryBuildInfo buildInfo)
        {
            sb.Append(" IN (");
            ValueSegment valueSegment = (ValueSegment)segment.Right;
            object[] values = valueSegment.Values;
            for (int i = 0, valuesLength = values.Length; i < valuesLength; i++)
            {
                Object value = values[i];
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append("?");

                QueryExecParam param = new QueryExecParam();
                param.Index = buildInfo.ExecInfo.Params.Count;
                param.Type = valueSegment.Type;
                param.Value = value;
                buildInfo.ExecInfo.Params.Add(param);
            }
            sb.Append(") ");
        }

        private void ProcessQueryValues(StringBuilder sb, CompareSegment segment, QueryBuildInfo buildInfo, IDbLayer dbLayer)
        {
            sb.Append(" IN ");
            QuerySegment querySegment = (QuerySegment)segment.Right;
            ProcessQuery(sb, querySegment, buildInfo, dbLayer);
        }

        private void ProcessMerge(StringBuilder sb, MergeSegment segment, QueryBuildInfo buildInfo, IDbLayer dbLayer)
        {
            int count = 0;
            if (segment.Mode == MergeSegmentMode.ParaAnd
                    || segment.Mode == MergeSegmentMode.ParaOr)
            {
                sb.Append("(");
            }
            foreach (ISegment subSegment in segment.Segments)
            {
                if (count > 0)
                {
                    switch (segment.Mode)
                    {
                        case MergeSegmentMode.And:
                        case MergeSegmentMode.ParaAnd:
                            sb.Append(" AND "); break;
                        case MergeSegmentMode.ParaOr:
                        case MergeSegmentMode.Or:
                            sb.Append(" OR "); break;
                    }
                }
                Process(sb,subSegment,buildInfo,dbLayer);
                count++;
            }
            if (segment.Mode == MergeSegmentMode.ParaAnd
                    || segment.Mode ==MergeSegmentMode.ParaOr)
            {
                sb.Append(")");
            }
        }

        public IDbRelation GetRelation(Type typeFrom,Type typeTo)
        {
            ICollection<IDbRelation> relations = null;
            try
            {
                relations = CacheManager.FieldCache.GetDbRelations(typeFrom);
            }
            catch (FieldCacheMissException e)
            {
                try
                {
                    CacheManager.FieldCache.Register(typeFrom);
                    relations = CacheManager.FieldCache.GetDbRelations(typeFrom);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }

            if (relations != null)
            {
                foreach (IDbRelation relation in relations)
                {
                    if (relation.RelatedObjectType == typeTo)
                    {
                        return relation;
                    }
                }
            }
            return null;
        }
    }
}
