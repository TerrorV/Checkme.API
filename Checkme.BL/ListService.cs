using Checkme.BL.Abstract;
using Checkme.BL.Abstract.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            list.Timestamp = DateTime.Now;
            if(!Lists.TryAdd(list.Id, list))
            {
                throw new  ItemExistsException("Item already exists");
            }
        }

        public void AddItemToList(Guid listId, string word)
        {
            if (Lists[listId].Outstanding.Contains(word) || Lists[listId].Done.Contains(word))
            {
                return;
            }

            Lists[listId].Outstanding.Add(word);
            Lists[listId].Timestamp = DateTime.Now;
        }

        public async Task<CheckList> GetListById(Guid id,DateTime timespan)
        {
            if (!Lists.ContainsKey(id))
            {
                throw new KeyNotFoundException(id.ToString());
            }

            while(Lists[id].Timestamp<timespan)
            {
                Thread.Sleep(3000);
            }

            return Lists[id];
        }

        public async Task<CheckList> GetListById(Guid id)
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

            Lists[listId].Timestamp = DateTime.Now;
        }

        public void UpdateItem(Guid listId, string word, ItemState state)
        {
            if (state == ItemState.Done && Lists[listId].Outstanding.Contains(word))
            {
                Lists[listId].Outstanding.Remove(word);
                Lists[listId].Done.Add(word);
            }
            else if (state == ItemState.Outstanding && Lists[listId].Done.Contains(word))
            {
                Lists[listId].Done.Remove(word);
                Lists[listId].Outstanding.Add(word);
            }
            else
            {
                throw new KeyNotFoundException($"{listId} - {word}");
            }
            
            Lists[listId].Timestamp = DateTime.Now;
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

            Lists[listId].Timestamp = DateTime.Now;
        }

        public void EditItem(Guid listId, string oldItem, string newItem)
        {
            if(Lists[listId].Outstanding.Contains(oldItem))
            {
                var itemIndex = Lists[listId].Outstanding.IndexOf(oldItem);
                Lists[listId].Outstanding[itemIndex] = newItem;
            }else if (Lists[listId].Done.Contains(oldItem))
            {
                var itemIndex = Lists[listId].Done.IndexOf(oldItem);
                Lists[listId].Done[itemIndex] = newItem;
            }
            else
            {
                throw new KeyNotFoundException($"{listId} - {oldItem}");
            }
        
            Lists[listId].Timestamp = DateTime.Now;
        }
    }
}
