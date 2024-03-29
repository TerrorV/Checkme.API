﻿using Checkme.BL.Abstract;
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
        public static ConcurrentDictionary<Guid, List<System.IO.Stream>> ListSubscribers { get; private set; }
        private IBlobStorageRepo _blobStorage;
        event EventHandler<Guid> OnListUpdated;

        public ListService(IBlobStorageRepo blobStorage)
        {
            Lists = new ConcurrentDictionary<Guid, CheckList>();
            ListSubscribers = new ConcurrentDictionary<Guid, List<System.IO.Stream>>();
            _blobStorage = blobStorage;
            OnListUpdated += ListUpdateHandler;

        }

        public async Task AddList(CheckList list)
        {
            list.Timestamp = DateTime.Now;
            if (!Lists.TryAdd(list.Id, list))
            {
                throw new ItemExistsException("Item already exists");
            }

            ListSubscribers.TryAdd(list.Id, new List<System.IO.Stream>());

            await _blobStorage.AddResource(list, list.Id.ToString());
            OnListUpdated?.Invoke(null, list.Id);
        }

        public async Task AddItemToList(Guid listId, string word)
        {
            if (Lists[listId].Outstanding.Contains(word) || Lists[listId].Done.Contains(word))
            {
                throw new ArgumentException(word);
            }

            Lists[listId].Outstanding.Add(word);
            Lists[listId].Timestamp = DateTime.Now;

            await PersistList(Lists[listId], listId.ToString());
            try
            {
                OnListUpdated?.Invoke(null, listId);
            }
            catch (Exception ex)
            {

                throw ex;
            }
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
                ListSubscribers.TryAdd(id, new List<System.IO.Stream>());
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
                if (!ListSubscribers.ContainsKey(item.Id))
                {
                    ListSubscribers[item.Id] = new List<System.IO.Stream>();
                }
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
            OnListUpdated?.Invoke(null, listId);
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
            OnListUpdated?.Invoke(null, listId);
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
            OnListUpdated?.Invoke(null, listId);
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
            OnListUpdated?.Invoke(null, listId);
        }

        void ListUpdateHandler(object? sender, Guid id)
        {
            try
            {
                var subs = ListSubscribers[id];

                foreach (var stream in subs)
                {
                    stream.WriteAsync(Encoding.UTF8.GetBytes($"data: {System.Text.Json.JsonSerializer.Serialize(Lists[id],new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy= new LowerCaseNamingPolicy() })}\n\n")).AsTask().Wait();
                    stream.FlushAsync().Wait();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task SubscribeClient(Guid listId, System.IO.Stream stream)
        {
            ListSubscribers[listId].Add(stream);
            ////OnListUpdated += ListUpdateHandler;
            ////OnListUpdated += (sender, id) =>
            ////{
            ////    ListUpdateHandler(sender, id);

            ////};

            await Task.Run(() =>
            {
                for (int i = 0; i < 40; i++)
                {
                    stream.WriteAsync(Encoding.UTF8.GetBytes("data: ping\n"));
                    stream.WriteAsync(UTF8Encoding.UTF8.GetBytes("\n"));
                    stream.FlushAsync();
                    Thread.Sleep(30000);
                }
            });
        }

        public async Task PersistList(CheckList list, string listId)
        {
            //await _blobStorage.SaveBlob(list, $"checkme_{listId}");
            await _blobStorage.SaveBlob(list, listId.ToString());
        }

        public async Task UpdateList(Guid listId, CheckList list)
        {
            CheckList newList = MergeList(Lists[listId], list);
            await PersistList(list, listId.ToString());

            OnListUpdated?.Invoke(null, listId);
        }

        public CheckList MergeList(CheckList oldList, CheckList newList)
        {
            if (oldList.Timestamp > newList.Timestamp)
            {
                var tempList = new CheckList();
                tempList.Done = new List<string>();
                tempList.Outstanding = new List<string>();
                tempList.Done.AddRange(oldList.Done);
                tempList.Outstanding.AddRange(oldList.Outstanding);
                tempList.Id = oldList.Id;
                tempList.Done.AddRange(newList.Done.Where(e => !oldList.Done.Contains(e) && !oldList.Outstanding.Contains(e)));
                tempList.Outstanding.AddRange(newList.Outstanding.Where(e => !oldList.Outstanding.Contains(e) && !oldList.Done.Contains(e)));
                tempList.Timestamp = DateTime.UtcNow;

                return tempList;
            }
            else
            {
                return newList;
            }

            //throw new NotImplementedException();
        }
    }
}
