using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2manyexample.entities
{
    public abstract class One2ManyChildEntity : DefaultServerDbClass
    {
        public abstract string Name { get; set; }
    }
}