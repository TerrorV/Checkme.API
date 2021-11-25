using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Checkme.BL.Abstract
{
    public interface IListService
    {
        Task<CheckList> GetListById(Guid id);
        Task<CheckList> GetListById(Guid id, DateTime timespan);
        Task<IEnumerable<CheckList>> GetLists();
        Task AddList(CheckList list);
        Task AddItemToList(Guid listId, string word);
        Task RemoveItemFromList(Guid listId, string word);
        Task UpdateItem(Guid listId, string word);
        Task UpdateItem(Guid listId, string word, ItemState state);
        Task EditItem(Guid listId, string oldWord, string newWord);
        Task SubscribeClient(Guid listId, Stream stream);
        Task UpdateList(Guid listId, CheckList list);
    }
}