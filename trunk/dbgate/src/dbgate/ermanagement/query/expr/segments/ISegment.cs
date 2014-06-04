using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.query.expr.segments
{
    public interface ISegment
    {
        SegmentType SegmentType { get; }

        ISegment Add(ISegment segment);
    }
}
