﻿using System;
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
        public ActionResult<FilterDTO<IEnumerable<coreevent.Event>>> Get([ModelBinder(binderType: typeof(SearchBinder))]coreevent.SearchRequest srch)
        {
            try {
              var evts = _eventService.GetAllEvents().ToList();
              if ((srch==null) || (srch!=null 
                                        && srch.FilterObjectWrapper.FilterObjects.Count==0 
                                        && srch.SortObjects.Count==0 
                                        && srch.Page==0
                                        && srch.PageSize==25))
                     { //return Ok(evts);
                       var newRes = new FilterDTO<IEnumerable<coreevent.Event>>() { total=evts.Count, data=evts };
                        return Ok(newRes);
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
                    var filtering = fp.GetFiltering(srch);
                    var sorting = coreevent.FilterParsing<coreevent.Event>.GetSorting(srch);
                    
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

                    int RowCount = query.Count();
                    int skip = srch.Page * srch.PageSize;
                    var newList = query.Skip(skip).Take(srch.PageSize).ToList();
            
                    //var newList = query.ToList();
                    var newRes = new FilterDTO<IEnumerable<coreevent.Event>>() { total=RowCount, data=newList };
                    //new { total = mdl.RowCount, data = res }
                    return Ok(newRes);
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

        [HttpGet("people/{id}")]
        public ActionResult<List<coreevent.Person>> GetPeopleForEvent(int id)
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
                    var np = _eventService.GetEventByIdWithPeople(id);
                return np;
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        
        //Errors = ModelState.SelectMany(x => x.Value.Errors)            .Select(x => x.ErrorMessage).ToArray();
        }

        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post([FromBody] coreevent.Event value)
        {
              try
            {
                //        var entity = TheModelFactory.Parse(courseModel);

                //        if (entity == null) Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not read subject/tutor from body");

                //        if (TheRepository.Insert(entity) && TheRepository.SaveAll())
                //        {
                //            return Request.CreateResponse(HttpStatusCode.Created, TheModelFactory.Create(entity));
                //        }
                //        else
                //        {
                var resp = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent("[{\"Name\":\"ABC\"},[{\"A\":\"1\"},{\"B\":\"2\"},{\"C\":\"3\"}]]")
                };

                //resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return resp;
                //return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not save to the database.");
        //        }
            }
            catch (Exception ex)
            {
                //****remove build warnings
                var exhld = ex;
                //****remove build warnings

                //return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);

                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] coreevent.Event value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
        }
    }
}