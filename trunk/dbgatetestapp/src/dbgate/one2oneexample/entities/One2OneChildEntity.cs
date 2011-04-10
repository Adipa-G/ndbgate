using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2oneexample.entities
{
    public abstract class One2OneChildEntity : DefaultServerDbClass
    {
        public abstract string Name { get; set; }
    }
}
