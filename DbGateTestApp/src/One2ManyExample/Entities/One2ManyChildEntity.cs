using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2ManyExample.Entities
{
    [WikiCodeBlock("one_2_many_example_child_entity")]
    public abstract class One2ManyChildEntity : DefaultEntity
    {
        public abstract string Name { get; set; }
    }
}