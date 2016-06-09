using GradeTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GradeTracker.Controllers
{
    public class GradebookController : Controller
    {
		public ActionResult AddSemester(StudentModel student)
		{
			SemesterModel semester = new SemesterModel();
			semester.assocStudentId = student.studentId;
			return View(semester);
		}

		[HttpPost]
		public ActionResult SaveNewSemester(SemesterModel semester)
		{
			if (ModelState.IsValid)
			{
				var db = new ApplicationDbContext();
				db.SemesterModels.Add(semester);
				db.SaveChanges();
				return RedirectToAction("Index", "Home");
			}	
			return RedirectToAction("Gradebook", "Error");
		}

		public ActionResult Courses(SemesterModel semester) 
		{
			return View(semester);	
		}
    }
}