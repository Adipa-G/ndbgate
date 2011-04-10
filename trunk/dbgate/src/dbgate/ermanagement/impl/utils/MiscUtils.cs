namespace dbgate.ermanagement.impl.utils
{
    public class MiscUtils
    {
        public static void Modify(IServerDbClass serverDbClass)
        {
            if (serverDbClass.Status == DbClassStatus.Unmodified)
            {
                serverDbClass.Status = DbClassStatus.Modified;
            }
        }
    }
}
