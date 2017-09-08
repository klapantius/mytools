using System;
using System.Linq;

using juba.tfs.interfaces;

using Moq;


namespace sybi_utest
{
    public class MockedChangeset : IChangeset
    {
        private Mock<IChangeset> m;

        public MockedChangeset(int id, DateTime creationTime)
        {
            m = new Mock<IChangeset>();
            ChangesetId = id;
            CreationDate = creationTime;
        }

        public MockedChangeset(int id)
            : this(id, DateTime.Now)
        {
        }

        public MockedChangeset(int id, string[] changedFiles)
            : this(id)
        {
            Changes = changedFiles.Select(f =>
            {
                var mockedItem = new MockedItem()
                {
                    ServerItem = f,
                    ChangesetId = this.ChangesetId,
                    CheckinDate = this.CreationDate
                };
                var mockedChange = new MockedChange()
                {
                    Item = mockedItem.Object
                };
                return mockedChange.Object;
            }).ToArray();
        }

        public IChangeset Object { get { return m.Object; } }

        private IChange[] myChanges;

        public IChange[] Changes
        {
            get { return myChanges; }
            set
            {
                m.Setup(foo => foo.Changes).Returns(value);
                myChanges = value;
            }
        }

        private DateTime myCreationDate;

        public DateTime CreationDate
        {
            get { return myCreationDate; }
            set
            {
                m.Setup(foo => foo.CreationDate).Returns(value);
                myCreationDate = value;
            }
        }

        private int myChangesetId;
        public int ChangesetId
        {
            get { return myChangesetId; }
            set
            {
                m.Setup(foo => foo.ChangesetId).Returns(value);
                myChangesetId = value;
            }
        }

        private IWorkItem[] myWorkItems;
        public IWorkItem[] WorkItems
        {
            get { return myWorkItems; }
            set
            {
                m.Setup(foo => foo.WorkItems).Returns(value);
                myWorkItems = value;
            }
        }

        private string myComment;

        public string Comment
        {
            get { return myComment;}
            set
            {
                m.Setup(foo => foo.Comment).Returns(value);
                myComment = value;
            }
        }
    }
}