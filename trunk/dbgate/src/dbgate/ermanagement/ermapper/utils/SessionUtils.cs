using dbgate.context;
using dbgate.context.impl;

namespace dbgate.ermanagement.ermapper.utils
{
    public class SessionUtils
    {
        public static void InitSession(IReadOnlyEntity roEntity)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession == null)
            {
                roEntity.Context.ErSession = new ErSession();
            }
        }

        public static void TransferSession(IReadOnlyEntity parentEntity,IReadOnlyEntity childEntity)
        {
            if (parentEntity.Context != null
                && childEntity.Context != null
                && parentEntity.Context.ErSession != null)
            {
                childEntity.Context.ErSession = parentEntity.Context.ErSession;
            }
        }

        public static void DestroySession(IReadOnlyEntity roEntity)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null)
            {
                roEntity.Context.ErSession = null;
            }
        }

        public static bool ExistsInSession(IReadOnlyEntity roEntity, ITypeFieldValueList typeList)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null
                && typeList != null)
            {
                return roEntity.Context.ErSession.IsProcessed(typeList);
            }
            return false;
        }

        public static void AddToSession(IReadOnlyEntity roEntity, IEntityFieldValueList typeList)
        {
            if (roEntity.Context != null
                && roEntity.Context.ErSession != null
                && typeList != null)
            {
                roEntity.Context.ErSession.CheckAndAddEntityList(typeList);
            }
        }

        public static IReadOnlyEntity GetFromSession(IReadOnlyEntity roEntity, ITypeFieldValueList typeList)
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
