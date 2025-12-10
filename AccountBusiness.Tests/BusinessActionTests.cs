using System.Threading.Tasks;
using AccountBusiness.Actions;
using Xunit;

namespace AccountBusiness.Tests
{
    public class BusinessActionTests
    {
        private class SequenceAction : BusinessAction<int>
        {
            public int State { get; private set; }

            protected override Task PreExecuteAsync()
            {
                State = 1; // pre
                return Task.CompletedTask;
            }

            protected override Task<int> RunAsync()
            {
                State = 2; // run
                return Task.FromResult(State);
            }

            protected override Task PostExecuteAsync(int result)
            {
                State = 3; // post
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task ExecuteAsync_Runs_Pre_Run_Post_InOrder()
        {
            var a = new SequenceAction();
            var result = await a.ExecuteAsync();
            Assert.Equal(3, a.State);
            Assert.Equal(2, result);
        }
    }
}
