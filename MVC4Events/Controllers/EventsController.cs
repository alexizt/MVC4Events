using mvc4events.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Unconnected.Mvc.Outputcache;

namespace MvcMovie.Controllers
{
    public class EventsController : Controller
    {

        private EventsRepository _eventList = null;

        public EventsController(EventsRepository eventList)
        {
            this._eventList = eventList;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Event List";
            return View();
        }

        //[OutputCache(CacheProfile = "EventsCache")]
        //[OutputCache(Duration =3600, VaryByParam = "iDisplayStart; iDisplayLength; sSearch")]
        //[ActionResultCache(Duration = 3600, VaryByParam = "iDisplayStart; iDisplayLength; sSearch")]
        //[ActionResultCache(CacheProfile = "EventsCache")]
        [ParameterizedOutputCacheAttribute(CacheProfile = "EventsCache")]
        [HttpPost]
        public JsonResult getEvents(int iDisplayStart, int iDisplayLength, string sSearch)
        {

            sSearch = sSearch.ToLower(); // Force No case sensitive
            List<Event> eventList = this._eventList.GetEvents().ToList();

            int totalRecord = eventList.Count();
            if (iDisplayLength == -1) { iDisplayLength = totalRecord; }



            if (!string.IsNullOrEmpty(sSearch))
                eventList = eventList.Where(x => x.Title.ToLower().Contains(sSearch)
                || x.Technology.ToLower().Contains(sSearch)
                ).ToList();

            int TotalDisplayRecords = eventList.Count();

            eventList = eventList.OrderBy(x => x.Title).Skip(iDisplayStart).Take(iDisplayLength).ToList();            

            var data = new {
                sEcho = 0,
                iTotalRecords = totalRecord,
                iTotalDisplayRecords = TotalDisplayRecords,
                aaData = eventList
            };

            return Json(data);
        }


        #region ConfigurationPropertyGets
        [System.Configuration.ConfigurationProperty("varyByParam")]
        public string VaryByParam { get; set; }
        [System.Configuration.ConfigurationProperty("outputCacheSettings")]
        public System.Web.Configuration.OutputCacheSettingsSection OutputCacheSettings { get; } 
        #endregion

    }
}
