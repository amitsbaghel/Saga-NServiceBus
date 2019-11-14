using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

namespace TestSaga
{
    public class TestSaga : Saga<TestSagaData>, IAmStartedByMessages<TestMessage>, IHandleTimeouts<TestSagaTimeOut>
    {
        private readonly AppSettings _appSettings;
        public TestSaga()
        {
            _appSettings=new AppSettings();
        }
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
        {
            mapper.ConfigureMapping<TestMessage>(message => message.SagaGuid).ToSaga(saga => saga.SagaId);
        }

        public async Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            LogSagaStart();
            Data.AddTestMessage(message);

            await RequestTimeout<TestSagaTimeOut>(context, _appSettings.BatchTimeout);

            if (CurrentTimeIsAfterBatchCutOffTime())
            {
                Console.WriteLine("After batch cutoff time: {0}, processing {1} messages.",
                    _appSettings.BatchCutOffDateTime, Data.TestMessageCollection.Count);
                await Blah(context);
                return;
            }

            await RequestTimeout<TestSagaTimeOut>(context, _appSettings.BatchCutOffDateTime.ToUniversalTime());

            if (!HasReachedBatchLimit())
            {
                return;
            }

            Console.WriteLine("Batch Limit reached. Processing {0} messages.", Data.TestMessageCollection.Count);

            await Blah(context);
        }

        public async Task Timeout(TestSagaTimeOut state, IMessageHandlerContext context)
        {
            Console.WriteLine("Timeout occurred. Processing {0} messages.", Data.TestMessageCollection.Count);

            await Blah(context);
        }

        void LogSagaStart()
        {
            if (Data.TestMessageCollection.Any())
                return;

            Console.WriteLine("Started new ESurveyCompleted Saga - {0}.", Data.Id);
        }

        bool CurrentTimeIsAfterBatchCutOffTime()
        {
            return _appSettings.BatchCutOffDateTime <= DateTime.Now;
        }

        bool HasReachedBatchLimit()
        {
            return _appSettings.MaximumBatchSize <= Data.TestMessageCollection.Count;
        }

       async Task Blah(IMessageHandlerContext context)
       {
           MarkAsComplete();
           Console.WriteLine("Finished ESurveyCompleted Saga - {0}.", Data.Id);
           await Task.CompletedTask;
       }
    }
}
