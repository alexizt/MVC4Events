using mvc4events.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Runtime.Caching;

namespace mvc4events.Controllers
{
    public class EventsMemCacheController : Controller
    {

        private EventsRepository _eventList = null;

        public EventsMemCacheController(EventsRepository eventList)
        {
            this._eventList = eventList;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Event List";
            return View();
        }


        [HttpPost]
        public JsonResult getEvents(int sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            string querykey = JsonConvert.SerializeObject(new { iDisplayStart, iDisplayLength, sSearch });

            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
            cacheItemPolicy.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddHours(1));

            dynamic list=cache.Get(querykey);
            if (list != null) // There is data in the cache for given parameters
            {                
                return Json(new {
                    sEcho = sEcho,
                    iTotalRecords = list.iTotalRecords,
                    iTotalDisplayRecords = list.iTotalDisplayRecords,
                    aaData = list.aaData
                });
            }
            else // There is no data in the cache for given parameters
            { 
                sSearch = sSearch.ToLower(); // Force No case sensitive
                
                var eventList = this._eventList.GetEvents().AsQueryable();
                
                int totalRecord = eventList.Count();
                if (iDisplayLength == -1) { iDisplayLength = totalRecord; }


                if (!string.IsNullOrEmpty(sSearch))
                    eventList = eventList.Where(x => x.Title.ToLower().Contains(sSearch)
                    || x.Technology.ToLower().Contains(sSearch)
                    );

                int TotalDisplayRecords = eventList.Count();

                eventList = eventList.OrderBy(x => x.Title).Skip(iDisplayStart).Take(iDisplayLength);            

                var data = new {
                    sEcho = sEcho,
                    iTotalRecords = totalRecord,
                    iTotalDisplayRecords = TotalDisplayRecords,
                    aaData = eventList.ToList()
                };
                

                cache.Add(querykey, data, cacheItemPolicy);
                return Json(data);
            }            
        }


        #region ConfigurationPropertyGets
        [System.Configuration.ConfigurationProperty("varyByParam")]
        public string VaryByParam { get; set; }
        [System.Configuration.ConfigurationProperty("outputCacheSettings")]
        public System.Web.Configuration.OutputCacheSettingsSection OutputCacheSettings { get; } 
        #endregion

    }
}
