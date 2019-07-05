using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using System.Net;
using System.Net.Http;
using System.Web;

using Microsoft.AspNetCore.Authorization;
using coreevent = SSSCalApp.Core.Entity;
using SSSCalApp.Core.ApplicationService;
using SSSCalAppWebAPI.Models;

using System.Linq.Dynamic.Core;

namespace SSSCalAppWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        
        IPersonService _personService;

       public PersonController(IPersonService personService)
        {
            _personService= personService;
        }

        // GET api/values
/****************
        [HttpGet]
        public ActionResult<IEnumerable<Person>> Get()
        {
            try {
                return Ok(_personService.GetAllPersons());
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        }
 */        
        [HttpGet]
        public ActionResult<FilterDTO<IEnumerable<coreevent.Person>>> Get([ModelBinder(binderType: typeof(SearchBinder))]coreevent.SearchRequest srch)
        {
            try {
              var evts = _personService.GetAllPersons().ToList();
               if ((srch==null) || (srch!=null 
                                        && srch.FilterObjectWrapper.FilterObjects.Count==0 
                                        && srch.SortObjects.Count==0 
                                        && srch.Page==0
                                        && srch.PageSize==25))
                    {//return Ok(evts);
                        var newRes = new FilterDTO<IEnumerable<coreevent.Person>>() { total=evts.Count, data=evts };
                        return Ok(newRes);
                    }
                else {

                    //Most javascript grids are 1 based page
                    if (srch.Page>0) srch.Page--;
                    var fp = new coreevent.FilterParsing<coreevent.Person>();
                    var filtering = fp.GetFiltering(srch);
                    var sorting = coreevent.FilterParsing<coreevent.Person>.GetSorting(srch);
                    IQueryable<coreevent.Person> query =evts.AsQueryable().Where(filtering).OrderBy(sorting);

                    int RowCount = query.Count();
                    int skip = srch.Page * srch.PageSize;
                    var newList = query.Skip(skip).Take(srch.PageSize).ToList();
            
                    //var newList = query.ToList();
                    var newRes = new FilterDTO<IEnumerable<coreevent.Person>>() { total=RowCount, data=newList };
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
        public ActionResult<coreevent.Person> Get(int id)
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
                    var np = _personService.FindPersonById(id);
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
        [Authorize]
        public HttpResponseMessage Post([FromBody] coreevent.Person value)
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
        public void Put(int id, [FromBody] coreevent.Person value)
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
