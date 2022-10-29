using Checkme.BL.Abstract;
using Moq;

namespace Checkme.BL.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var blobStore = new Mock<DAL.Abstract.IBlobStorageRepo>();
            var listStore = new Dictionary<Guid, CheckList>();
            blobStore.Setup(x => x.GetResource<CheckList>(It.IsAny<string>())).ReturnsAsync((string id) => listStore[new Guid(id.Split('_')[1])]);
            blobStore.Setup(x => x.AddResource(It.IsAny<CheckList>(), It.IsAny<string>())).ReturnsAsync((CheckList list, string id) => { listStore[new Guid(id.Split('_')[1])] = list; return id; }); ;
            blobStore.Setup(x => x.SaveBlob(It.IsAny<CheckList>(), It.IsAny<string>())).ReturnsAsync((CheckList list, string id) => { listStore[new Guid(id.Split('_')[1])] = list; return id; }); ;
            var listId = Guid.NewGuid();
            var svc = new ListService(blobStore.Object);
            svc.AddList(new CheckList() { Id = listId, Done = new List<string>(), Outstanding = new List<string>(), Timestamp = DateTime.Now });
            svc.AddItemToList(listId, "item 1");
            svc.AddItemToList(listId, "item 2");
            svc.AddItemToList(listId, "item 3");
            svc.AddItemToList(listId, "item 4");
            svc.AddItemToList(listId, "item 5");

            svc.UpdateItem(listId, "item 3", ItemState.Done);
            svc.UpdateItem(listId, "item 2", ItemState.Done);
            svc.UpdateItem(listId, "item 5", ItemState.Done);
            svc.UpdateItem(listId, "item 2", ItemState.Done);
            svc.UpdateItem(listId, "item 5", ItemState.Done);
            var endListTask = svc.GetListById(listId);
            endListTask.Wait();

            Assert.Equal(2, endListTask.Result.Outstanding.Count);
            Assert.Equal(3, endListTask.Result.Done.Count);
            Assert.Equal(3, listStore[listId].Done.Count);
        }
    }
}