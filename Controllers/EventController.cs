using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using System.Net;
using System.Net.Http;
using System.Web;

using coreevent = SSSCalApp.Core.Entity;
using SSSCalApp.Core.ApplicationService;
using Microsoft.AspNetCore.Authorization;
using SSSCalAppWebAPI.Models;

using System.Linq.Dynamic.Core;
using System.Globalization;
using Newtonsoft.Json;

namespace SSSCalAppWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        
        IEventService _eventService;

       public EventController(IEventService eventService)
        {
            _eventService= eventService;
        }

        /* GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<coreevent.Event>> Get()
        {
            try {
                return Ok(_eventService.GetAllEvents());
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        }
*/
        
        protected DateTime ParseConvertDate(string param){
            var i = param.IndexOf("GMT");
            if (i > 0)
            {
                param = param.Remove(i);
            }
            return DateTime.Parse(param, new CultureInfo("en-US"));
        }

//http://www.schuebelsoftware.com/SSSCalCoreApi/api/event?page=1&pageSize=9999&sort[0][field]=userName&sort[0][dir]=asc&filter[logic]=and&filter[filters][0][field]=Date&filter[filters][0][operator]=gte&filter[filters][0][value]=11-29-2020&filter[logic]=and&filter[filters][1][field]=Date&filter[filters][1][operator]=lte&filter[filters][1][value]=1-2-2021
        [HttpGet("calendarsearch")]
        public ActionResult<List<coreevent.Event>> GetCalendar(DateTime? startDate,DateTime? endDate)
        {
             List<coreevent.Event> evts = null;
             try {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));

                if (startDate.Value.Month>=11 && endDate.Value.Month<3)
                  evts = _eventService.GetAllEvents().Where(x=>
                        x.TopicId==1 && x.Date!=null && (x.Date.Value.Month>=startDate.Value.Month || x.Date.Value.Month <= endDate.Value.Month)).ToList();
                else
                  evts = _eventService.GetAllEvents().Where(x=>
                        x.TopicId==1 && x.Date!=null && x.Date.Value.Month>=startDate.Value.Month && x.Date.Value.Month <= endDate.Value.Month).ToList();

                    var cevt = _eventService.GetCalculatedEventsByDateRange(startDate.Value, endDate.Value);
                    evts.AddRange(cevt);

                    evts.AddRange(_eventService.GetAllEvents().Where(x=> (x.TopicId!=1 && x.Date!=null && x.RepeatYearly==true && x.Date.Value.Month>=startDate.Value.Month && x.Date.Value.Month <= endDate.Value.Month)).ToList());
                    
                    foreach (var item in evts)
                    {
                        if ((item.RepeatYearly==true) || (item.TopicId==1 && item.Date!=null))
                            item.Date= new DateTime(DateTime.Now.Year, item.Date.Value.Month, item.Date.Value.Day);
                    }


                return Ok(evts.ToList());
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        
        //Errors = ModelState.SelectMany(x => x.Value.Errors)            .Select(x => x.ErrorMessage).ToArray();
        }


        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<coreevent.Event>> Get([ModelBinder(binderType: typeof(SearchBinder))]coreevent.SearchRequest srch)
        {
            try {
                //need to pull back all for all request or adjusting birthday dates for queires
                    DateTime? startDate = null;
                    DateTime? endtDate = null;
                    List<coreevent.Event> evts = null;
                    
                    if (srch.FilterObjectWrapper.FilterObjects.Count==2) {
                        if (srch.FilterObjectWrapper.FilterObjects.ElementAt(0).Field1=="Date" && srch.FilterObjectWrapper.FilterObjects.ElementAt(1).Field1=="Date"
                            && srch.FilterObjectWrapper.FilterObjects.ElementAt(0).Value1!="" && srch.FilterObjectWrapper.FilterObjects.ElementAt(1).Value1!="")
                        {
                            startDate = ParseConvertDate(srch.FilterObjectWrapper.FilterObjects.ElementAt(0).Value1);
                            endtDate = ParseConvertDate(srch.FilterObjectWrapper.FilterObjects.ElementAt(1).Value1);

                          evts = _eventService.GetAllEvents().Where(x=>(x.TopicId!=1 && x.Date!=null && x.Date>=startDate && x.Date <= endtDate)).ToList();
//                        (x.TopicId==1 && x.Date!=null && x.Date.Value.Month>=DateTime.Now.Month)).OrderBy(x=>x.Date.Value.Month);


                            var cevt = _eventService.GetCalculatedEventsByDateRange(startDate.Value, endtDate.Value);
                            evts.AddRange(cevt);

                        }   
                        else {
                            evts = _eventService.GetAllEvents().ToList();                            
                        }
                    }   
                    else {
                        evts = _eventService.GetAllEvents().ToList();                            
                    }


              if ((srch==null) || (srch!=null 
                                        && srch.FilterObjectWrapper.FilterObjects.Count==0 
                                        && srch.SortObjects.Count==0 
                                        && srch.Page==0
                                        && srch.PageSize==25))
                     { //return Ok(evts);
                            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-TotalRecords");
                         HttpContext.Response.Headers.Add("Paging-TotalRecords", JsonConvert.SerializeObject (evts.Count));
                        return Ok(evts);
                    }
                else {
                    //Most javascript grids are 1 based page
                    if (srch.Page>0) srch.Page--;
                    var fp = new coreevent.FilterParsing<coreevent.Event>();
                    string filtering="true";
                    try {
                        filtering = fp.GetFiltering(srch);
                    } catch(Exception fex) {
                        ModelState.AddModelError("Person:Get:FilterParse", fex.Message);
                        return BadRequest(ModelState);  
                    }

                    var sorting = "true";
                    try {
                        sorting = coreevent.FilterParsing<coreevent.Event>.GetSorting(srch);
                    } catch(Exception sex) {
                        ModelState.AddModelError("Person:Get:SortParse", sex.Message);
                        return BadRequest(ModelState);  
                    }
                    
                    //IQueryable<coreevent.Event> query =evts.AsQueryable().Where(filtering).OrderBy(sorting);
                    IQueryable<coreevent.Event> query =evts.AsQueryable().Where(filtering);

                    //if filter and date range, handle birthdays
                    if (startDate!=null && endtDate!=null) {
                        evts= query.ToList();
                        evts.AddRange(_eventService.GetAllEvents().Where(x=> (x.TopicId==1 && x.Date!=null && x.Date.Value.Month>=startDate.Value.Month && x.Date.Value.Month <= endtDate.Value.Month)).ToList());
                        
                        foreach (var item in evts)
                        {
                            if ((item.RepeatYearly==true) || (item.TopicId==1 && item.Date!=null))
                                item.Date= new DateTime(DateTime.Now.Year, item.Date.Value.Month, item.Date.Value.Day);
                        }
                        query =evts.AsQueryable();
                    }
                    query =query.OrderBy(sorting);

                    int RowCount = 0;
                    var newList = new List<coreevent.Event>();
                    try {
                        RowCount = query.Count();
                        int skip = srch.Page * srch.PageSize;
                        newList = query.Skip(skip).Take(srch.PageSize).ToList();
                    }
                    catch {}

                    //var newList = query.ToList();
                    HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-TotalRecords");
                    HttpContext.Response.Headers.Add("Paging-TotalRecords", JsonConvert.SerializeObject (RowCount));
                    //new { total = mdl.RowCount, data = res }
                    return Ok(newList);
                }
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<coreevent.Event> Get(int id)
        {
            //return NotFound("Product not found");
            //return BadRequest(ModelState);
            //Person p=null;// = new Person();
            //p.Name="frank smith";
            //return p;
            //return Ok(new { Message="test" });
            try {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                    var np = _eventService.GetEventById(id);
                return np;
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        
        //Errors = ModelState.SelectMany(x => x.Value.Errors)            .Select(x => x.ErrorMessage).ToArray();
        }

        //** /api/event/people/1
        [HttpGet("people/{id}")]
        public ActionResult<List<coreevent.Person>> GetPeopleForEvent(int id)
        {
            try {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                    var np = _eventService.GetEventByIdWithPeople(id);
                return np;
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        
        //Errors = ModelState.SelectMany(x => x.Value.Errors)            .Select(x => x.ErrorMessage).ToArray();
        }

        //** /api/event/currentyearly
        [HttpGet("currentyearly")]
        public ActionResult<List<coreevent.Person>> GetCurrentEvents()
        {
            try {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                  var evts = _eventService.GetAllEvents().Where(x=>
                        (x.TopicId!=1 && x.Date!=null && x.Date>=DateTime.Now) ||
                        (x.TopicId==1 && x.Date!=null && x.Date.Value.Month>=DateTime.Now.Month)).OrderBy(x=>x.Date.Value.Month);
                return Ok(evts.ToList());
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        
        //Errors = ModelState.SelectMany(x => x.Value.Errors)            .Select(x => x.ErrorMessage).ToArray();
        }

        // POST api/values
        [Authorize]
        [HttpPost]
        public ActionResult<coreevent.Event> Post([FromBody] coreevent.Event value)
        {
            try
            {
                //string authorization = Request.Headers["Authorization"]; 
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                var np = _eventService.CreateEvent(value);
                return Ok(np);
            }
            catch (Exception ex)
            {
                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Event:Post", ex.Message);
                    ex=ex.InnerException;
                }
                return BadRequest(ModelState);  
               
            }
        }

        // PUT api/values/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] coreevent.Event item)
        {
            try 
            {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                if (id != item.Id)
                    return NotFound("Event not found"); 
                var np = _eventService.UpdateEvent(item);
                return Ok(np);
            }
            catch (Exception ex)
            {
                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Event:Put", ex.Message);
                    ex=ex.InnerException;
                }
                return BadRequest(ModelState);  
               
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
           try {
                if (! _eventService.DeleteEvent(id))
                    return NotFound("Event not found");
                return Ok();
            }
            catch(Exception ex) {

                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Event:Get", ex.Message);
                    ex=ex.InnerException;
                }
              return BadRequest(ModelState);  
            }
        }
    }
}
