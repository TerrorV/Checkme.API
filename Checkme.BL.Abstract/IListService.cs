using System;
using System.Collections.Generic;

namespace Checkme.BL.Abstract
{
    public interface IListService
    {
        CheckList GetListById(Guid id);
        IEnumerable<CheckList> GetLists();
        void AddList(CheckList list);
        void AddItemToList(Guid listId, string word);
        void RemoveItemFromList(Guid listId, string word);
        void UpdateItem(Guid listId, string word);
        void UpdateItem(Guid listId, string word, ItemState state);

        void EditItem(Guid listId, string oldWord, string newWord);
    }
}