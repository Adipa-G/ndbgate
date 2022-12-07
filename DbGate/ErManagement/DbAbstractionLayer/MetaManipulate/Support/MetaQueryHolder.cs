using System;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Support
{
    public class MetaQueryHolder : IComparable<MetaQueryHolder>
    {
        public const int ObjectTypeTable = 1;
        public const int ObjectTypeColumn = 2;
        public const int ObjectTypePrimaryKey = 3;
        public const int ObjectTypeForeignKey = 4;

        public const int OperationTypeAdd = 1;
        public const int OperationTypeAlter = 2;
        public const int OperationTypeDelete = 3;

        public MetaQueryHolder()
        {
        }

        public MetaQueryHolder(int itemType, int queryType, string queryString)
        {
            ItemType = itemType;
            QueryType = queryType;
            QueryString = queryString;
        }

        public int ItemType { get; set; }
        public int QueryType { get; set; }
        public string QueryString { get; set; }

        #region IComparable<MetaQueryHolder> Members

        
        public int CompareTo(MetaQueryHolder metaQueryHolder)
        {
            if (metaQueryHolder == null)
                return 1;
            if (ItemType != metaQueryHolder.ItemType)
            {
                return ItemType.CompareTo(metaQueryHolder.ItemType);
            }
            else
            {
                if (QueryType == OperationTypeAdd)
                    return 1;
                if (QueryType == OperationTypeDelete)
                    return -1;
                return 0;
            }
        }

        #endregion
    }
}