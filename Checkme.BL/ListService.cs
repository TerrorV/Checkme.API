using Checkme.BL.Abstract;
using Checkme.BL.Abstract.Exceptions;
using Checkme.DAL.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Checkme.BL
{
    public class ListService : IListService
    {
        public static ConcurrentDictionary<Guid, CheckList> Lists { get; private set; }
        private IBlobStorageRepo _blobStorage;

        public ListService(IBlobStorageRepo blobStorage)
        {
            Lists = new ConcurrentDictionary<Guid, CheckList>();
            _blobStorage = blobStorage;
        }

        public async Task AddList(CheckList list)
        {
            list.Timestamp = DateTime.Now;
            if (!Lists.TryAdd(list.Id, list))
            {
                throw new ItemExistsException("Item already exists");
            }

            await _blobStorage.AddResource(list, list.Id.ToString());
        }

        public async Task AddItemToList(Guid listId, string word)
        {
            if (Lists[listId].Outstanding.Contains(word) || Lists[listId].Done.Contains(word))
            {
                return;
            }

            Lists[listId].Outstanding.Add(word);
            Lists[listId].Timestamp = DateTime.Now;

            await PersistList(Lists[listId], listId.ToString());
        }

        public async Task<CheckList> GetListById(Guid id, DateTime timespan)
        {
            var tempList = _blobStorage.GetResource<CheckList>(id.ToString());
            if (!Lists.ContainsKey(id))
            {
                throw new KeyNotFoundException(id.ToString());
            }

            while (Lists[id].Timestamp < timespan)
            {
                Thread.Sleep(3000);
            }

            return Lists[id];
        }

        public async Task<CheckList> GetListById(Guid id)
        {
            var tempList = _blobStorage.GetResource<CheckList>(id.ToString());
            if (Lists.ContainsKey(id))
            {
                return Lists[id];
            }
            else if (tempList != null)
            {
                Lists[id] = await tempList;
                return Lists[id];
            }

            throw new KeyNotFoundException(id.ToString());
        }

        public async Task<IEnumerable<CheckList>> GetLists()
        {
            await LoadLists();

            return Lists.Values;
        }

        private async Task LoadLists()
        {
            var idList = await _blobStorage.GetAllResourceIds();

            var tempLists = idList.Select(x => _blobStorage.GetResource<CheckList>(x).Result).ToList();
            foreach (var item in tempLists)
            {
                Lists[item.Id] = item;
            }
        }

        private void LoadListById(Guid id)
        {
            Lists[id] = _blobStorage.GetResource<CheckList>(id.ToString()).Result;
        }
        public async Task RemoveItemFromList(Guid listId, string word)
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
            await PersistList(Lists[listId], listId.ToString());
        }

        public async Task UpdateItem(Guid listId, string word, ItemState state)
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

            await PersistList(Lists[listId], listId.ToString());
        }

        public async Task UpdateItem(Guid listId, string word)
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

            await PersistList(Lists[listId], listId.ToString());
        }

        public async Task EditItem(Guid listId, string oldItem, string newItem)
        {
            if (Lists[listId].Outstanding.Contains(oldItem))
            {
                var itemIndex = Lists[listId].Outstanding.IndexOf(oldItem);
                Lists[listId].Outstanding[itemIndex] = newItem;
            }
            else if (Lists[listId].Done.Contains(oldItem))
            {
                var itemIndex = Lists[listId].Done.IndexOf(oldItem);
                Lists[listId].Done[itemIndex] = newItem;
            }
            else
            {
                throw new KeyNotFoundException($"{listId} - {oldItem}");
            }

            Lists[listId].Timestamp = DateTime.Now;

            await PersistList(Lists[listId], listId.ToString());
        }

        public async Task PersistList(CheckList list, string listId)
        {
            _blobStorage.SaveBlob(list, $"checkme_{listId}");
        }
    }
}
