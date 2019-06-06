using Microsoft.Extensions.Logging;
using smidigprosjekt.Logic.Database;
using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{
    public interface IInterestProviderService
    {
        IList<InterestItem> GetAll();
        Task<InterestItem> Add(string Category, string Tag);
        Task Remove(InterestItem item);
    }
    public class InterestProviderService : List<InterestItem>, IInterestProviderService
    {
        private ILogger logger;

        public InterestProviderService(ILoggerFactory loggerFactory)
        {
            //Copy the current interestlist from database
            logger = loggerFactory.CreateLogger<InterestProviderService>();
            GetInterestList();
        }
        public IList<InterestItem> GetAll() => this;

        private void GetInterestList()
        {
            logger.Log(LogLevel.Information, "Downloading Interest List...");
            Clear();
            AddRange(FirebaseDbConnection.GetInterests().Result);
        }

        public async Task<InterestItem> Add(string Category, string Tag)
        {
            if (string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(Tag)) return null;
            var interest = new InterestItem() { Category = Category, Name = Tag };
            try {
                interest.Key = (await FirebaseDbConnection.AddInterest(interest)).Key;
                Add(interest);
            }
            catch (Exception e)
            {
                logger.LogError("Failed adding interest error message: {0}", e.Message);
            }
            return interest;
        }
        

        public new async Task Remove(InterestItem item)
        {
            base.Remove(item);
            if (string.IsNullOrEmpty(item.Key)) return;
            await FirebaseDbConnection.RemoveInterest(item);
        }
    }
}
