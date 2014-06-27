using DbGate;

namespace DbGateTestApp.One2OneExample.Entities
{
    public abstract class One2OneChildEntity : DefaultEntity
    {
        public abstract string Name { get; set; }
    }
}