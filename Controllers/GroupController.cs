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
    public class GroupController : ControllerBase
    {
        
        IGroupService _groupService;

       public GroupController(IGroupService groupService)
        {
            _groupService= groupService;
        }


        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<coreevent.Group>> Get()
        {
            try {
                //need to pull back all for all request or adjusting birthday dates for queires
              var recs = _groupService.GetAll().ToList();
                HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-TotalRecords");
                HttpContext.Response.Headers.Add("Paging-TotalRecords", JsonConvert.SerializeObject (recs.Count));
                return Ok(recs);
            }
            catch(Exception ex) {

              ModelState.AddModelError("Person:Get", ex.Message);
              return BadRequest(ModelState);  
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<coreevent.Group>> Get(int id)
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
                    var np = _groupService.GetById(id);
                //return np;
                return Ok(np);
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
        public ActionResult<coreevent.Group> Post([FromBody] coreevent.Group value)
        {
            try
            {
                //string authorization = Request.Headers["Authorization"]; 
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                //var np = _groupService.CreateEvent(value);
                //return Ok(np);
                return Ok();
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
        public async Task<IActionResult> Put(int id, [FromBody] coreevent.Group item)
        {
            try 
            {
                if (!ModelState.IsValid)
                    throw new ArgumentException("ModelState must be invalid", nameof(ModelState));
                if (id != item.Id)
                    return NotFound("Event not found"); 
                //var np = _groupService.UpdateEvent(item);
                //return Ok(np);
                return Ok();
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
                //if (! _groupService.DeleteEvent(id))
                //    return NotFound("Event not found");
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
