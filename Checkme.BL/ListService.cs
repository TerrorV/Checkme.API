using Checkme.BL.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Checkme.BL
{
    public class ListService : IListService
    {
        public static ConcurrentDictionary<Guid, CheckList> Lists { get; private set; }

        public ListService()
        {
            Lists = new ConcurrentDictionary<Guid, CheckList>();
        }

        public void AddList(CheckList list)
        {
            if(!Lists.TryAdd(list.Id, list))
            {
                throw new Exception("Item already exists");
            }
        }

        public void AddItemToList(Guid listId, string word)
        {
            if (Lists[listId].Outstanding.Contains(word) || Lists[listId].Done.Contains(word))
            {
                return;
            }

            Lists[listId].Outstanding.Add(word);
        }

        public CheckList GetListById(Guid id)
        {
            if (!Lists.ContainsKey(id))
            {
                throw new KeyNotFoundException(id.ToString());
            }

            return Lists[id];
            //throw new NotImplementedException();
        }

        public IEnumerable<CheckList> GetLists()
        {
            return Lists.Values;
        }

        public void RemoveItemFromList(Guid listId, string word)
        {
            if (Lists[listId].Outstanding.Contains(word))
            {
                Lists[listId].Outstanding.Remove(word);
            }
            else if (Lists[listId].Done.Contains(word))
            {
                Lists[listId].Done.Remove(word);
            }
            else
            {
                throw new KeyNotFoundException(listId.ToString());
            }
        }

        public void UpdateItem(Guid listId, string word, ItemState state)
        {
            if (state == ItemState.Done)
            {
                Lists[listId].Outstanding.Remove(word);
                Lists[listId].Done.Add(word);
            }
            else if (state == ItemState.Outstanding)
            {
                Lists[listId].Done.Remove(word);
                Lists[listId].Outstanding.Add(word);
            }
        }

        public void UpdateItem(Guid listId, string word)
        {
            if (Lists[listId].Outstanding.Contains(word))
            {
                Lists[listId].Outstanding.Remove(word);
                Lists[listId].Done.Add(word);
            }
            else if (Lists[listId].Done.Contains(word))
            {
                Lists[listId].Done.Remove(word);
                Lists[listId].Outstanding.Add(word);
            }
        }

        public void EditItem(Guid listId, string oldItem, string newItem)
        {
            if(Lists[listId].Outstanding.Contains(oldItem))
            {
                var itemIndex = Lists[listId].Outstanding.IndexOf(oldItem);
                Lists[listId].Outstanding[itemIndex] = newItem;
            }

            if (Lists[listId].Done.Contains(oldItem))
            {
                var itemIndex = Lists[listId].Done.IndexOf(oldItem);
                Lists[listId].Done[itemIndex] = newItem;
            }
        }
    }
}
