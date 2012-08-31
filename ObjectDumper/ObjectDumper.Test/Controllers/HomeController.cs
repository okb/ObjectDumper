#region License

// --------------------------------------------------
// Copyright © OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System.Collections.Generic;
using System.Web.Mvc;

namespace ObjectDumper.Test.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            ViewBag.TestModel = GetTestModel();
            return View(GetTestModel());
        }


        private static TestModel GetTestModel()
        {
            return new TestModel()
            {
                Id = 23423,
                AnonymousObjects = new List<object>()
                {
                    new { Hehu = "sdfsdf", Tall = 324234 },
                    new { Hehu = "sdfsdfsdf", Tall = 098 },
                    new { Hahi = "asd", Tall = "sdf" }
                },
                Dictionary = new Dictionary<string, object>()
                {
                    { "key", 234234 },
                    { "key1", true },
                    { "key2", "sdfsf" },
                }
            };
        }
    }

    public class TestModel
    {
        public IList<object> AnonymousObjects { get; set; }

        public IDictionary<string, object> Dictionary { get; set; }
        public int Id { get; set; }
    }
}