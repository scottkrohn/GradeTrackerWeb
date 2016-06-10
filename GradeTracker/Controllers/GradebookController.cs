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
        private ApplicationDbContext db = new ApplicationDbContext();

        private List<CourseModel> QueryCourses(string query)
        {
            var result = db.CourseModels.SqlQuery(query);
            return result.ToList();
        }

        private List<SemesterModel> QuerySemesters(string query)
        {
            var result = db.SemesterModels.SqlQuery(query);
            return result.ToList();
        }

        private SemesterModel GetSemesterForCourse(CourseModel course)
        {
            var result = db.SemesterModels.SqlQuery(String.Format("Select * FROM SemesterModels WHERE semesterId={0}", course.assocSemesterId));
            return result.First();
        }
        
        // HTTP GET Request to view the Courses page
        // Sends the selected SemesterModel to the page.
        public ActionResult Courses(SemesterModel semester) 
		{
            var model = QueryCourses(String.Format("SELECT * FROM CourseModels WHERE assocSemesterId={0}", semester.semesterId));
            ViewBag.CurrentTerm = semester.termName;
            ViewBag.CurrentYear = semester.termYear;
            ViewData["CurrentSemesterString"] = semester.termName.ToString() + " " + semester.termYear.ToString() ;
            ViewData["CurrentSemester"] = semester;
			return View(model);	
		}

        public ActionResult SpecificCourse(CourseModel course)
        {
            ViewData["CurrentSemester"] = GetSemesterForCourse(course);
            return View(course);
        }

        public ActionResult Error(string errorMessage) 
        {
            // Serve the Error view with the error message passed as an object.
            return View((object)errorMessage);
        }

        // Adds a new SemesterModel to the database and associated it
        // with the StudentModel object passed to the view.
		public ActionResult AddSemester(StudentModel student)
		{
			SemesterModel semester = new SemesterModel();
			semester.assocStudentId = student.studentId;
			return View(semester);
		}

        // Adds a new CourseModel to the database and associated it
        // with the SemesterModel object passed to the view.
        public ActionResult AddCourse(SemesterModel semester) 
        {
            CourseModel course = new CourseModel();
            course.assocSemesterId = semester.semesterId;
            ViewData["CurrentSemester"] = semester;
            return View(course);
        }

        public ActionResult AddWorkItem(CourseModel course)
        {
            WorkItemModel workItem = new WorkItemModel();
            workItem.assocCourseId = course.courseId;
            ViewData["currentCourse"] = course;
            return View(workItem);
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
			return RedirectToAction("Error", "Gradebook", "Semester was not added.");
		}

        [HttpPost]
        public ActionResult SaveNewCourse(CourseModel course) {
            if(ModelState.IsValid) 
            {
                db.CourseModels.Add(course);
                db.SaveChanges();
                SemesterModel currentSemester = GetSemesterForCourse(course);
                return RedirectToAction("Courses", "Gradebook", currentSemester);
            }
            return RedirectToAction("Error", "Gradebook", "Course was not added.");
        }
/*
        [HttpPost]
        public ActionResult SaveNewWorkItem(WorkItemModel workItem)
        {
            if(ModelState.IsValid)
            {
                db.WorkItemModels.Add(workItem);
                db.SaveChanges();

            }
        }
        */
		
    }
}