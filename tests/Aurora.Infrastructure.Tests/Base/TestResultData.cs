using Aurora.Application.Models;

namespace Aurora.Infrastructure.Tests.Base
{
    public class TestResultData : SearchResultData
    {
        public TestResultData(int parentProp)
        {
            ParentProp = parentProp;
        }

        public int ParentProp { get; }
    }
}
