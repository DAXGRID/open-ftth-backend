using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Specifications
{
    public interface IConduitSpecificationRepository
    {
        List<ConduitSpecification> GetConduitSpecifications();

        ConduitSpecification GetConduitSpecification(Guid specificationId);
    }
}
