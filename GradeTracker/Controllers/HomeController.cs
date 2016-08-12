using GradeTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace GradeTracker.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			// If the user is not logged in, display the generic homepage.
			if (!User.Identity.IsAuthenticated)
			{
				return View();
			}

			//If the user IS logged in, display the user specific homepage.
			ApplicationDbContext db = new ApplicationDbContext();
			StudentModel student = db.Students.Find(User.Identity.GetUserId());

			// Send user to the logged in homepage, pass the student object to the view for use.
			return View("AccountHome", student);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		public ActionResult FinalGradeCalculator()
		{
			return View();
		}

		/*
		 * Display the "Learn More" view.
		 */ 
		public ActionResult LearnMore()
		{
			return View();
		}

		public JsonResult GetFinalGradeNeeded(double currentGrade, double gradeWanted, double finalWeight)
		{
			Models.BusinessLogic.GradeComputation gradeComp = new Models.BusinessLogic.GradeComputation();
			double finalGradeNeeded = gradeComp.CalcGradeNeededOnFinal(currentGrade, gradeWanted, finalWeight);
			return Json(new {gradeNeeded = finalGradeNeeded}, JsonRequestBehavior.AllowGet);
		}
	}
}