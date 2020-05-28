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

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<coreevent.Event>> Get([ModelBinder(binderType: typeof(SearchBinder))]coreevent.SearchRequest srch)
        {
            try {
                //need to pull back all for all request or adjusting birthday dates for queires
              var evts = _eventService.GetAllEvents().ToList();
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
                    foreach (var item in evts)
                    {
                        if ((item.RepeatYearly==true) || (item.TopicId==1 && item.Date!=null))
                            item.Date= new DateTime(DateTime.Now.Year, item.Date.Value.Month, item.Date.Value.Day);
                    }

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
                    
                    //if filter and date range
                    if (srch.FilterObjectWrapper.FilterObjects.Count==2) {
                        if (srch.FilterObjectWrapper.FilterObjects.ElementAt(0).Field1=="Date" && srch.FilterObjectWrapper.FilterObjects.ElementAt(1).Field1=="Date"
                            && srch.FilterObjectWrapper.FilterObjects.ElementAt(0).Value1!="" && srch.FilterObjectWrapper.FilterObjects.ElementAt(1).Value1!="")
                        {
                            var startDate = ParseConvertDate(srch.FilterObjectWrapper.FilterObjects.ElementAt(0).Value1);
                            var endtDate = ParseConvertDate(srch.FilterObjectWrapper.FilterObjects.ElementAt(1).Value1);
                            var cevt = _eventService.GetCalculatedEventsByDateRange(startDate, endtDate);
                            evts.AddRange(cevt);
                        }
                    }

                    IQueryable<coreevent.Event> query =evts.AsQueryable().Where(filtering).OrderBy(sorting);

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
