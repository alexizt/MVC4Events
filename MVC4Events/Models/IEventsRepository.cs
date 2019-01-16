using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mvc4events.Models;

namespace mvc4events.Models
{
    public interface IEventsRepository
    {
        IEnumerable<Event> GetEvents();
    }
}