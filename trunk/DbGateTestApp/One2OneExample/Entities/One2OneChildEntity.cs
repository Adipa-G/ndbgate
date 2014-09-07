using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2OneExample.Entities
{
    [WikiCodeBlock("one_2_one_example_child_entity")]
    public abstract class One2OneChildEntity : DefaultEntity
    {
        public abstract string Name { get; set; }
    }
}