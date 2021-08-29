using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Checkme.BL.Abstract
{
    public interface IListService
    {
        Task<CheckList> GetListById(Guid id);
        Task<CheckList> GetListById(Guid id, DateTime timespan);
        IEnumerable<CheckList> GetLists();
        void AddList(CheckList list);
        void AddItemToList(Guid listId, string word);
        void RemoveItemFromList(Guid listId, string word);
        void UpdateItem(Guid listId, string word);
        void UpdateItem(Guid listId, string word, ItemState state);

        void EditItem(Guid listId, string oldWord, string newWord);
    }
}