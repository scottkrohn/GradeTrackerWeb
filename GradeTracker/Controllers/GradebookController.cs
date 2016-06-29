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

		// Get a CategoryWeight object with an id matching the function argument.
		private CategoryWeight GetCategoryWeight(int id)
		{
			var result = db.CategoryWeights.SqlQuery(String.Format("SELECT * FROM CategoryWeights WHERE categoryId={0}", id));
			return result.First();
		}

		// Get a CategoryWeight object for a course with a specific name.
		private CategoryWeight GetCategoryWeight(string categoryName, int courseId)
		{
			var result = db.CategoryWeights.SqlQuery(String.Format("SELECT * FROM CategoryWeights WHERE assocCourseId={0} AND categoryName='{1}'", courseId, categoryName));
			return result.First();
		}

		private WorkItemModel GetWorkItemById(int id)
		{
			var result = db.WorkItemModels.SqlQuery(String.Format("SELECT * from WorkItemModels WHERE id={0}", id));
			return result.First();
		} 

        /*****************************************************************/


        /**************************AJAX CALLS*****************************/

		[HttpPost]
		public PartialViewResult EditWorkItem(int id, int earned , int possible)
		{
			WorkItemModel foundWorkItem = GetWorkItemById(id);
			foundWorkItem.pointsEarned = earned;
			foundWorkItem.pointsPossible = possible;
			db.Entry(foundWorkItem).State = System.Data.Entity.EntityState.Modified;
			db.SaveChanges();
			CourseModel assocCourse = GetCourseById(foundWorkItem.assocCourseId);
			CategoryWeight assocWeight = GetCategoryWeight(foundWorkItem.categoryName, assocCourse.courseId);
			ViewData["AssociatedCategoryWeight"] = assocWeight;
			return PartialView("_EditWorkItemPartial", foundWorkItem);
		//	return Json(foundWorkItem);
		}

		/*
		 * Gets the cumulative total of all of the category weights that have
		 * been added to the course with courseId.
		 */ 
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

		/*
		 * Saves a new category weight into the database and returns the new weight as 
		 * a Json object for the view to process/display.
		 */
		[HttpPost]
		public JsonResult SaveNewCategoryWeight(int courseId, string categoryName, int categoryWeight)
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
				return Json(weight);
			}
			catch(Exception ex)
			{
				return new JsonResult();
			}
		}

		/*
		 * Deletes a category weight from the database and returns true if the
		 * weight was successfully deleted.
		 */ 
		[HttpPost]
		public bool DeleteCategoryWeight(int categoryId)
		{
			// Get the category from the database
			try
			{
				CategoryWeight foundCategory = GetCategoryWeight(categoryId);
				if(db.CategoryWeights.Remove(foundCategory) != null)
				{
					db.SaveChanges();
					return true;
				}
			}
			catch(Exception ex)
			{
				return false;
			}
			return false;
		}

		/*
		 * Returns true if the category passed as an argument is in use, meaning a work
		 * item has been assigned that category, for the course argument.
		 */ 
		[HttpPost]
		public ActionResult CategoryInUse(int categoryId, int courseId)
		{
			var result = false;
			try
			{
				CategoryWeight weight = GetCategoryWeight(categoryId);
				List<WorkItemModel> workItems = GetWorkItemsForCourse(GetCourseById(courseId));
				foreach(WorkItemModel workItem in workItems)
				{
					if(workItem.categoryName == weight.categoryName)
					{
						result = true;
						break;
					}
				}
			}
			catch (Exception ex) {
			}
			return Json(new {inUse = result});
		}
        /*****************************************************************/
        

        /***********************ROUTING FUNCTIONS*************************/

        /* 
		 * HTTP GET request to display a view with the courses that are 
		 * associated with a given SemesterModel object.
        */ 
        public ActionResult Courses(SemesterModel semester) 
		{
            var model = QueryCourses(String.Format("SELECT * FROM CourseModels WHERE assocSemesterId={0}", semester.semesterId));
            ViewBag.CurrentTerm = semester.termName;
            ViewBag.CurrentYear = semester.termYear;
            ViewData["CurrentSemesterString"] = semester.termName.ToString() + " " + semester.termYear.ToString() ;
            ViewData["CurrentSemester"] = semester;
			return View(model);	
		}

		/*
		 * HTTP GET request to display a view for a specific CourseModel.
		 */ 
        public ActionResult SpecificCourse(CourseModel course)
        {
			SemesterModel currentSemester = GetSemesterForCourse(course);
            ViewData["CurrentSemester"] = currentSemester;
            ViewData["CurrentSemesterString"] = currentSemester.termName.ToString() + " " + currentSemester.termYear.ToString() ;
            ViewData["AssociatedWorkItems"] = GetWorkItemsForCourse(course);
			ViewData["AssociatedCategoryWeights"] = GetCategoryWeightsForCourse(course);
            return View(course);
        }

		/*
		 * HTTP GET request to display a view for a specific WorkItem.
		 */ 
		public ActionResult SpecificWorkItem(WorkItemModel workItem)
		{
			CourseModel assocCourse = GetCourseById(workItem.assocCourseId);
			CategoryWeight assocWeight = GetCategoryWeight(workItem.categoryName, assocCourse.courseId);
			ViewData["AssociatedCourse"] = assocCourse;
			ViewData["AssociatedCategoryWeight"] = assocWeight;	
			return View(workItem);
		}

		/*
		 * HTTP GET that returns an error page.
		 */ 
        public ActionResult Error(string errorMessage) 
        {
            // Serve the Error view with the error message passed as an object.
            return Content(errorMessage);
        }

        /* 
		 * HTTP GET request to display a view to get user input to add a
		 * new SemesterModel object to the database and associate it with
		 * a specific StudentModel.
        */ 
		public ActionResult AddSemester(StudentModel student)
		{
			SemesterModel semester = new SemesterModel();
			semester.assocStudentId = student.studentId;
			return View(semester);
		}

		/*
		 * HTTP Get request to display a view to get user input to add a
		 * new CourseModel to the database and associate it with a specific
		 * SemesterModel object.
		 */ 
        public ActionResult AddCourse(SemesterModel semester) 
        {
            CourseModel course = new CourseModel();
            course.assocSemesterId = semester.semesterId;
            ViewData["CurrentSemester"] = semester;
            return View(course);
        }

		/*
		 * HTTP GET request to display a view to get user input to add a
		 * new WorkItemModel object to the database and associate it with
		 * a specific CourseModel object.
		 */ 
        public ActionResult AddWorkItem(CourseModel course)
        {
            WorkItemModel workItem = new WorkItemModel();
            workItem.assocCourseId = course.courseId;
            ViewData["currentCourse"] = course;
			ViewData["CategoryWeights"] = GetCategoryWeightsForCourse(course);
            return View(workItem);
        }

		/*
		 * HTTP POST request to save the 'semester' argument into the database and
		 * then save the database changes. Redirects back to the AccountHome page
		 * for a logged in user.
		 */ 
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
		/*
		 * HTTP POST request to save the 'course' argument into the database and then
		 * save the database changes. Redirects back to the Courses view.
		 */ 
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

		/*
		 * HTTP POST request to save the 'workItem' argument into the database and then
		 * save the database changes. Redirects back to the Specific Course view.
		 */ 
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
        /*****************************************************************/
    }
}