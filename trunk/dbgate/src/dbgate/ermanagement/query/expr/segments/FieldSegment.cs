using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.query.expr.segments
{
    public class FieldSegment : ISegment
    {
        private readonly Type _entityType;
        private readonly string _field;
        private readonly string _alias;
	  	
	  	public FieldSegment(Type entitytType, string field)
	  	{
	  	    _entityType = entitytType;
	  	    _field = field;
	  	}
	  	
	  	public FieldSegment(Type entitytType, string field, string alias)
	  	{
	  	    _entityType = entitytType;
	  	    _field = field;
	  	    _alias = alias;
	  	}
	  	
	  	public SegmentType SegmentType
	  	{
            get {  return SegmentType.Field; }
	  	}
	  	
	  	public Type EntityType
	  	{
            get { return _entityType; }
	  	}
	  	
	  	public String Field
	  	{
            get { return _field; }
	  	}
	  	
	  	public String Alias
	  	{
            get { return _alias; }
	  	}
    }
}
