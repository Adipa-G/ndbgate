namespace dbgate.ermanagement.impl.utils
{
    public class MiscUtils
    {
        public static void Modify(IEntity entity)
        {
            if (entity.Status == EntityStatus.Unmodified)
            {
                entity.Status = EntityStatus.Modified;
            }
        }
    }
}
