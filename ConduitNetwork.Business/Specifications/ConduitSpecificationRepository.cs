using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConduitNetwork.Business.Specifications
{
    public class ConduitSpecificationRepository : IConduitSpecificationRepository
    {
        Dictionary<Guid,ConduitSpecification> _specifications = new Dictionary<Guid,ConduitSpecification>();

        public ConduitSpecificationRepository()
        {
            _specifications = ConduitSpecificationMockupBuilder.CreateSpecifications();
        }


        public List<ConduitSpecification> GetConduitSpecifications()
        {
            return _specifications.Values.ToList();
        }

        public ConduitSpecification GetConduitSpecification(Guid specificationId)
        {
            if (_specifications.ContainsKey(specificationId))
                return _specifications[specificationId];
            else
                throw new KeyNotFoundException("Cannot find any specification with id: " + specificationId);
        }
    }
}
