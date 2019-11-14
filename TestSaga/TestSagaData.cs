using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

namespace TestSaga
{
    public class TestSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
        public Guid SagaId { get; set; }

        public virtual void AddTestMessage(TestMessage testMessage)
        {
            TestMessageCollection.Add(testMessage);
        }

        private IList<TestMessage> _testMessage;

        public virtual IList<TestMessage> TestMessageCollection
        {
            get => _testMessage ?? (_testMessage = new List<TestMessage>());

            set => _testMessage = value;
        }
    }
}
