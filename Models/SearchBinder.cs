using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using coreevent = SSSCalApp.Core.Entity;

namespace SSSCalAppWebAPI.Models
{
public class SearchBinder : IModelBinder
    {

       protected ICollection<coreevent.SortObject> GetSortObjects(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext bindingContext)
        {
            var list = new List<coreevent.SortObject>();
            var filterKeys = new List<KeyValuePair<string, string>>();
            //sort[0][field]=Date&sort[0][dir]=asc
            for(int i=0;i<10;i++)
            {
                var fieldName = bindingContext.ValueProvider.GetValue($"sort[{i}][field]").FirstValue;
                if (fieldName==null)
                    break;
                else {
                    var srtObj = new coreevent.SortObject(fieldName,bindingContext.ValueProvider.GetValue($"sort[{i}][dir]").FirstValue);

                    list.Add(srtObj);
                }
            }
            return list;
        }

        protected coreevent.FilterObjectWrapper GetFilterObjects(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext bindingContext, string filterLogic)
        {
            var list = new List<coreevent.FilterObject>();
            var filterKeys = new List<KeyValuePair<string, string>>();
        //&filter[logic]=and&filter[filters][0][field]=Date&filter[filters][0][operator]=contains&filter[filters][0][value]=4/1/2018
        //&filter[logic]=and&filter[filters][0][field]=Date&filter[filters][0][operator]=gte&filter[filters][0][value]=Mon+Apr+23+2018+00%3A00%3A00+GMT-0500+(Central+Daylight+Time)&filter[filters][1][field]=Date&filter[filters][1][operator]=lte&filter[filters][1][value]=Mon+Apr+30+2018+00%3A00%3A00+GMT-0500+(Central+Daylight+Time)&_=1562110553341
            for(int i=0;i<10;i++)
            {
                var fieldName = bindingContext.ValueProvider.GetValue($"filter[filters][{i}][field]").FirstValue;
                if (fieldName==null)
                    break;
                else {
                    var fltObj = new coreevent.FilterObject();
                    fltObj.Field1 = fieldName;
                    fltObj.Operator1 = bindingContext.ValueProvider.GetValue($"filter[filters][{i}][operator]").FirstValue;
                    fltObj.Value1 = bindingContext.ValueProvider.GetValue($"filter[filters][{i}][value]").FirstValue;

                    list.Add(fltObj);
                }
            }
            return new coreevent.FilterObjectWrapper(filterLogic, list);
        }



        public System.Threading.Tasks.Task BindModelAsync (Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
             throw new ArgumentNullException(nameof(bindingContext));  
  
            var filterLogic = bindingContext.ValueProvider.GetValue("filter[logic]");
 //           if (filterLogic == ValueProviderResult.None)
 //                      return Task.CompletedTask; 

//?page=1&pageSize=20&sort[0][field]=Date&sort[0][dir]=asc&filter[logic]=and&filter[filters][0][field]=Date&filter[filters][0][operator]=gte&filter[filters][0][value]=4/1/2018
//?page=1&pageSize=20&sort[0][field]=Name&sort[0][dir]=asc&filter[logic]=and&filter[filters][0][field]=Name&filter[filters][0][operator]=contains&filter[filters][0][value]=schuebel
//&filter[logic]=and&filter[filters][0][field]=Name&filter[filters][0][operator]=contains&filter[filters][0][value]=schuebel
            var page = bindingContext.ValueProvider.GetValue("page").FirstValue;// bindingContext.ModelName);
            var pageSize = bindingContext.ValueProvider.GetValue("pageSize").FirstValue;
            var filtering =  GetFilterObjects(bindingContext, filterLogic.FirstValue);
            var sorting = GetSortObjects(bindingContext);

            var result = new coreevent.SearchRequest {
                Page=int.Parse(page??"0"),
                PageSize=int.Parse(pageSize??"25"),
                FilterObjectWrapper = filtering,
                SortObjects = sorting
             };
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }



    }
}