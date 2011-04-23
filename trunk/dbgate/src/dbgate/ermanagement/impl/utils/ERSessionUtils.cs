using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;

namespace dbgate.ermanagement.impl.utils
{
    public class ErSessionUtils
    {
        public static void InitSession(IServerRoDbClass roEntity)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession == null)
            {
                roEntity.Context.ErSession = new ErSession();
            }
        }

        public static void TransferSession(IServerRoDbClass parentEntity,IServerRoDbClass childEntity)
        {
            if (parentEntity.Context != null
                && childEntity.Context != null
                && parentEntity.Context.ErSession != null)
            {
                childEntity.Context.ErSession = parentEntity.Context.ErSession;
            }
        }

        public static void DestroySession(IServerRoDbClass roEntity)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null)
            {
                roEntity.Context.ErSession = null;
            }
        }

        public static bool ExistsInSession(IServerRoDbClass roEntity, ITypeFieldValueList typeList)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null
                && typeList != null)
            {
                return roEntity.Context.ErSession.IsProcessed(typeList);
            }
            return false;
        }

        public static void AddToSession(IServerRoDbClass roEntity, IEntityFieldValueList typeList)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null
                && typeList != null)
            {
                roEntity.Context.ErSession.CheckAndAddEntityList(typeList);
            }
        }

        public static IServerRoDbClass GetFromSession(IServerRoDbClass roEntity, ITypeFieldValueList typeList)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null
                && typeList != null)
            {
                return roEntity.Context.ErSession.GetProcessed(typeList);
            }
            return null;
        }
    }
}
