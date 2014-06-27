using DbGate;

namespace DbGateTestApp.One2ManyExample.Entities
{
    public abstract class One2ManyChildEntity : DefaultEntity
    {
        public abstract string Name { get; set; }
    }
}