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
using Newtonsoft.Json;

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
        public ActionResult<IEnumerable<coreevent.Person>> Get([ModelBinder(binderType: typeof(SearchBinder))]coreevent.SearchRequest srch)
        {
            try {
              var evts = _personService.GetAllPersons().ToList();
               if ((srch==null) || (srch!=null 
                                        && srch.FilterObjectWrapper.FilterObjects.Count==0 
                                        && srch.SortObjects.Count==0 
                                        && srch.Page==0
                                        && srch.PageSize==25))
                    {//return Ok(evts);
                        HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-TotalRecords");
                         HttpContext.Response.Headers.Add("Paging-TotalRecords", JsonConvert.SerializeObject (evts.Count));
                        return Ok(evts);
                    }
                else {

                    //Most javascript grids are 1 based page
                    if (srch.Page>0) srch.Page--;
                    var fp = new coreevent.FilterParsing<coreevent.Person>();
                    string filtering="true";
                    try {
                        filtering = fp.GetFiltering(srch);
                    } catch(Exception fex) {
                        ModelState.AddModelError("Person:Get:FilterParse", fex.Message);
                        return BadRequest(ModelState);  
                    }

                    var sorting = "true";
                    try {
                        sorting = coreevent.FilterParsing<coreevent.Person>.GetSorting(srch);
                    } catch(Exception sex) {
                        ModelState.AddModelError("Person:Get:SortParse", sex.Message);
                        return BadRequest(ModelState);  
                    }
                    IQueryable<coreevent.Person> query =evts.AsQueryable().Where(filtering).OrderBy(sorting);

                    int RowCount = 0;
                    var newList = new List<coreevent.Person>();
                    try {
                        RowCount = query.Count();
                        int skip = srch.Page * srch.PageSize;
                        newList = query.Skip(skip).Take(srch.PageSize).ToList();
                    }
                    catch {}

                    // Setting Header  
                    HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-TotalRecords");
                    HttpContext.Response.Headers.Add("Paging-TotalRecords", JsonConvert.SerializeObject (RowCount));  
            
                    //var newList = query.ToList();
                   // var newRes = new FilterDTO<IEnumerable<coreevent.Person>>() { total=RowCount, data=newList };
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
        [Authorize]
        public ActionResult<coreevent.Person> Get(int id)
        {
            try {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                var np = _personService.FindPersonById(id);
                if (np==null)
                    return NotFound("Person not found");
                return np;
            }
            catch(Exception ex) {

                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Person:Get", ex.Message);
                    ex=ex.InnerException;
                }
              return BadRequest(ModelState);  
            }
        
        //Errors = ModelState.SelectMany(x => x.Value.Errors)            .Select(x => x.ErrorMessage).ToArray();
        }

        // POST api/values
        [HttpPost]
        [Authorize]
        public ActionResult<coreevent.Person> Post([FromBody] coreevent.Person value)
        {
            try
            {
                //string authorization = Request.Headers["Authorization"]; 
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                var np = _personService.CreatePerson(value);
                return Ok(np);
            }
            catch (Exception ex)
            {
                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Person:Post", ex.Message);
                    ex=ex.InnerException;
                }
                return BadRequest(ModelState);  
               
            }
        }

        // PUT api/values/5
        [Authorize]
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Put(int id, [FromBody] coreevent.Person item)
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] coreevent.Person item)
        {
         try 
            {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
//                if (id != item.Id)
//                    return NotFound("Person not found"); 
                var np = _personService.UpdatePerson(item);
                return Ok(np);
            }
            catch (Exception ex)
            {
                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Person:Put", ex.Message);
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
                if (! _personService.DeletePerson(id))
                    return NotFound("Person not found");
                return Ok();
            }
            catch(Exception ex) {

                var sb = new System.Text.StringBuilder();
                while (ex!=null) {
                    ModelState.AddModelError("Person:Get", ex.Message);
                    ex=ex.InnerException;
                }
              return BadRequest(ModelState);  
            }
        }
    }
}
