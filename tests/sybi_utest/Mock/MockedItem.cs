using System;

using juba.tfs.interfaces;

using Moq;


namespace sybi_utest
{
    public class MockedItem
    {
        private Mock<IItem> myMock;

        public MockedItem()
        {
            myMock = new Mock<IItem>();
        }

        public IItem Object { get { return myMock.Object; } }

        public string ServerItem { set { myMock.Setup(foo => foo.ServerItem).Returns(value); } }
        public bool IsBranch { set { myMock.Setup(foo => foo.IsBranch).Returns(value); } }
        public int ChangesetId { set { myMock.Setup(foo => foo.ChangesetId).Returns(value); } }
        public DateTime CheckinDate { set { myMock.Setup(foo => foo.CheckinDate).Returns(value); } }
        public System.IO.Stream DownloadFileRetuns { set { myMock.Setup(foo => foo.DownloadFile()).Returns(value); } }
    }
}