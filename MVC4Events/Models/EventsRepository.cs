using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mvc4events.Models
{
    public class EventsRepository : IEventsRepository
    {

        private List<Event> eventList { get; set; }        

        public EventsRepository() {
            eventList = new List<Event>();
            for (int i = 0; i < 10000; i++)
            {
                eventList.Add(new Event()
                {
                    Title = Faker.Company.Name(),
                    Technology = "Technology " + ((i % 2 == 0) ? "I" : "II").ToString(),
                    Date = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"),
                    URL = Faker.Internet.DomainName()
                });
            }
        }

        public IEnumerable<Event> GetEvents()
        {
           return eventList;
        }

    }
}