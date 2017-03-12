using juba.tfs.interfaces;

using Moq;


namespace sybi_utest
{
    public class MockedChange
    {
        private Mock<IChange> myMock;

        public MockedChange()
        {
            myMock = new Mock<IChange>();
        }

        public IChange Object { get { return myMock.Object; } }

        public IItem Item { set { myMock.Setup(foo => foo.Item).Returns(value); } }
    }
}