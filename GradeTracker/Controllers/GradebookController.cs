using GradeTracker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace GradeTracker.Controllers
{
    public class GradebookController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /****************** DATABASE ACCESS FUNCTIONS ********************/
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

        private CourseModel GetCourseById(int courseId)
        {
            var result = db.CourseModels.SqlQuery(String.Format("SELECT * FROM CourseModels where courseId={0}", courseId));
            return result.First();
        }

        private List<WorkItemModel> GetWorkItemsForCourse(CourseModel course)
        {
            var result = db.WorkItemModels.SqlQuery(String.Format("SELECT * FROM WorkItemModels WHERE assocCourseId={0}", course.courseId));
            return result.ToList();
        }

        private List<CategoryWeight> GetCategoryWeightsForCourse(CourseModel course)
        {
            var result = db.CategoryWeights.SqlQuery(String.Format("SELECT * FROM CategoryWeights WHERE assocCourseId={0}", course.courseId));
            return result.ToList();
        }
		
        /*****************************************************************/
        
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

		[HttpPost]
		public int GetCurrentWeightTotal(int courseId)
		{
			List<CategoryWeight> weights = GetCategoryWeightsForCourse(GetCourseById(courseId));
			int total = 0;
			foreach(CategoryWeight weight in weights)
			{
				total += weight.categoryWeight;
			}
			return total;
		}

		[HttpPost]
		public string SaveNewCategoryWeight(int courseId, string categoryName, int categoryWeight)
		{
			CourseModel course = GetCourseById(courseId);
			CategoryWeight weight = new CategoryWeight();
			weight.assocCourseId = course.courseId;
			weight.categoryName = categoryName;
			weight.categoryWeight = categoryWeight;

			try
			{
				db.CategoryWeights.Add(weight);
				db.SaveChanges();
				return "success";
			}
			catch(Exception ex)
			{
				return ex.Message;
			}
		}

        public ActionResult SpecificCourse(CourseModel course)
        {
            ViewData["CurrentSemester"] = GetSemesterForCourse(course);
            ViewData["AssociatedWorkItems"] = GetWorkItemsForCourse(course);
			ViewData["AssociatedCategoryWeights"] = GetCategoryWeightsForCourse(course);
            return View(course);
        }

        public ActionResult Error(string errorMessage) 
        {
            // Serve the Error view with the error message passed as an object.
            return Content(errorMessage);
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

        public ActionResult EditCategoryWeights(CourseModel course)
        {
            ViewData["CurrentCategoryWeights"] = GetCategoryWeightsForCourse(course);
            return View();
        }

        [HttpPost]
        public string SetupCategoryInputs(int count)
        {
            return count.ToString();
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
				course.categories = new List<CategoryWeight>();
                db.CourseModels.Add(course);
                db.SaveChanges();
                SemesterModel currentSemester = GetSemesterForCourse(course);
                return RedirectToAction("Courses", "Gradebook", currentSemester);
            }
            return RedirectToAction("Error", "Gradebook", "Course was not added.");
        }

        

        [HttpPost]
        public ActionResult SaveNewWorkItem(WorkItemModel workItem)
        {
            if(ModelState.IsValid)
            {
                db.WorkItemModels.Add(workItem);
                db.SaveChanges();
                CourseModel currentCourse = GetCourseById(workItem.assocCourseId);
                return RedirectToAction("SpecificCourse", "Gradebook", currentCourse);
            }
            return RedirectToAction("Error", "Gradebook", "Work Item was not added.");
        }
		
    }
}