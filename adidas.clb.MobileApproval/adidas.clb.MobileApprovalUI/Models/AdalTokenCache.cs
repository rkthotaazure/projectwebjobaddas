using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace adidas.clb.MobileApprovalUI.Models
{
    public class ADALTokenCache : TokenCache
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private string userId;
        private UserTokenCache Cache;

        public ADALTokenCache(string signedInUserId)
        {
            // associate the cache to the current user of the web app
            userId = signedInUserId;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;
            // look up the entry in the database
            Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == userId);
            // place the entry in memory
            this.Deserialize((Cache == null) ? null : MachineKey.Unprotect(Cache.cacheBits, "ADALCache"));
        }

        // clean up the database
        public override void Clear()
        {
            base.Clear();
            var cacheEntry = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == userId);
            db.UserTokenCacheList.Remove(cacheEntry);
            db.SaveChanges();
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (Cache == null)
            {
                // first time access
                Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == userId);
            }
            else
            {
                // retrieve last write from the DB
                var status = from e in db.UserTokenCacheList
                             where (e.webUserUniqueId == userId)
                             select new
                             {
                                 LastWrite = e.LastWrite
                             };

                // if the in-memory copy is older than the persistent copy
                if (status.First().LastWrite > Cache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == userId);
                }
            }
            this.Deserialize((Cache == null) ? null : MachineKey.Unprotect(Cache.cacheBits, "ADALCache"));
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            //if (this.HasStateChanged)
            //{
            //    Cache = new UserTokenCache
            //    {
            //        webUserUniqueId = userId,
            //        cacheBits = MachineKey.Protect(this.Serialize(), "ADALCache"),
            //        LastWrite = DateTime.Now
            //    };
            //    // update the DB and the lastwrite 
            //    db.Entry(Cache).State = Cache.UserTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
            //    db.SaveChanges();
            //    this.HasStateChanged = false;
            //}
            // if state changed
            if (this.HasStateChanged || Cache == null)
            {
                Cache = Cache ?? new UserTokenCache();
                Cache.webUserUniqueId = userId;
                Cache.cacheBits = MachineKey.Protect(this.Serialize(), "ADALCache");
                Cache.LastWrite = DateTime.Now;
                // update the DB and the lastwrite 
                db.Entry(Cache).State = Cache.UserTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                //delete all the previous tokens for the current user
                //foreach (var cacheEntry in db.UserTokenCacheList)
                //{
                //    if (cacheEntry.webUserUniqueId == Cache.webUserUniqueId)
                //    {
                //        db.UserTokenCacheList.Remove(cacheEntry);
                //    }
                //}
                //foreach (var cacheEntry in db.UserTokenCacheList.Where
                //                (p => p.webUserUniqueId == Cache.webUserUniqueId).ToList())
                //{
                //    if (cacheEntry.LastWrite != Cache.LastWrite)
                //    {
                //        db.UserTokenCacheList.Remove(cacheEntry);
                //    }
                //}
                //db.SaveChanges();
                //db.SaveChanges();
                //db.Database.ExecuteSqlCommand("DELETE FROM UserTokenCaches WHERE webUserUniqueId='" + userId + "' and LastWrite!=" + Cache.LastWrite);

                db.SaveChanges();
                this.HasStateChanged = false;
            }
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
        }
    }
}
