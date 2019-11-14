using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSaga
{
    public class TestMessage
    {
        public Guid SagaGuid { get { return SagaIds.HardCodedGuid; } }
    }
}
