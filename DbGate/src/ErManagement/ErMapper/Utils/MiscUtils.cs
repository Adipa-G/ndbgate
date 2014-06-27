namespace DbGate.ErManagement.ErMapper.Utils
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