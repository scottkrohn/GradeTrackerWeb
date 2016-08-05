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

		// Perform a query to find specific CourseModel objects in the DB.
        private List<CourseModel> QueryCourses(string query)
        {
            var result = db.CourseModels.SqlQuery(query);
            return result.ToList();
        }

		// Perform a query to find specific SemesterModel objects in the DB.
        private List<SemesterModel> QuerySemesters(string query)
        {
            var result = db.SemesterModels.SqlQuery(query);
            return result.ToList();
        }

		// Get the SemesterModel object that a CourseModel object is associated with.
        private SemesterModel GetSemesterForCourse(CourseModel course)
        {
            var result = db.SemesterModels.SqlQuery(String.Format("Select * FROM SemesterModels WHERE semesterId={0}", course.assocSemesterId));
            return result.First();
        }


		// Get the Semester model object that is associated with the semesterID.
		private SemesterModel GetSemesterById(int semesterId)
		{
			var result = db.SemesterModels.SqlQuery(String.Format("SELECT * FROM SemesterModels WHERE semesterId={0}", semesterId));
			return result.First();
		}

		private int SemesterCount(int semesterId)
		{
			var result = db.SemesterModels.SqlQuery(String.Format("SELECT * FROM SemesterModels WHERE semesterId={0}", semesterId));
			return result.Count();
		}

		// Get a CourseModel object with a courseId that matches the function argument.
        private CourseModel GetCourseById(int courseId)
        {
            var result = db.CourseModels.SqlQuery(String.Format("SELECT * FROM CourseModels where courseId={0}", courseId));
            return result.First();
        }

		// Get all CourseModel objects associated with a specific semesterId
		private List<CourseModel> GetCoursesForSemester(int semesterId)
		{
			var result = db.CourseModels.SqlQuery(String.Format("SELECT * FROM CourseModels where assocSemesterId={0}", semesterId));
			return result.ToList();
		}

		// Get all WorkItemModels associated with a specific CourseModel object.
        private List<WorkItemModel> GetWorkItemsForCourse(CourseModel course)
        {
            var result = db.WorkItemModels.SqlQuery(String.Format("SELECT * FROM WorkItemModels WHERE assocCourseId={0}", course.courseId));
            return result.ToList();
        }

		// Get all CategoryWeights associated with a given CourseModel object.
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

		// Get a WorkItemModel with an id that matches the function argument.
		private WorkItemModel GetWorkItemById(int id)
		{
			var result = db.WorkItemModels.SqlQuery(String.Format("SELECT * from WorkItemModels WHERE id={0}", id));
			return result.First();
		} 

        /*****************************************************************/


        /**************************AJAX CALLS*****************************/


		/*
		 * Uses a GradeComputation obeject to get the overall weighted grade for a courseId.
		 * Retrusn the grade to the view as a Json object.
		 */ 
		[HttpGet]
		public JsonResult GetWeightedGrade(int courseId)
		{
			Models.BusinessLogic.GradeComputation gradeComp = new Models.BusinessLogic.GradeComputation(db);
			double grade = gradeComp.GetWeightedCourseGrade(courseId);
			return Json(new {weightedGrade = grade}, JsonRequestBehavior.AllowGet);
		}

		/*
		 * Uses a GradeComputation object to get the overall grade for a specific weight
		 * category in a specific course. Returns the result in a Json object.
		 */ 
		[HttpPost]
		public JsonResult GetOverallCategoryGrade(int courseId, string categoryName)
		{
			Models.BusinessLogic.GradeComputation gradeComp = new Models.BusinessLogic.GradeComputation(db);
			CategoryWeight weight = GetCategoryWeight(categoryName, courseId);
			double categoryGrade = gradeComp.GetCategoryOverallGrade(courseId, categoryName);
			return Json(new {categoryGrade = categoryGrade, categoryId = weight.categoryId}, JsonRequestBehavior.AllowGet);
		}

		/*
		 * Checks the database to see if a semester exists with the given semseterId. Used to prevent adding
		 * courses to a semester that has been deleted.
		 */ 
		[HttpGet]
		public JsonResult SemesterExists(int semesterId)
		{
			if(SemesterCount(semesterId) != 0)
			{
				return Json(new {result = true}, JsonRequestBehavior.AllowGet);
			}	
			return Json(new {result = false},JsonRequestBehavior.AllowGet);
		}



		[HttpPost]
		public JsonResult DeleteSemester(int semesterId)
		{
			SemesterModel semester = GetSemesterById(semesterId);
			List<CourseModel> associatedCourses = GetCoursesForSemester(semesterId);
			try
			{
				foreach(CourseModel course in associatedCourses)
				{
					DeleteCourse(course.courseId);
				}
				if(db.SemesterModels.Remove(semester) != null)
				{
					db.SaveChanges();
					return Json(new {result = true});
				}
			}
			catch(Exception ex)
			{
				return Json(new {result = false});
			}
			return Json(new {result = false});
			
		}


		/*
		 * Delete a specific course from the databases, as well as the WorkItemModels
		 * and CategoryWeight objects that are associated with that course.
		 */ 
		[HttpPost]
		public JsonResult DeleteCourse(int courseId)
		{
			CourseModel course = GetCourseById(courseId);
			List<CategoryWeight> associatedWeights = GetCategoryWeightsForCourse(course);
			List<WorkItemModel> associatedWorkItems = GetWorkItemsForCourse(course);

			try
			{
				if(DeleteAllWeights(course))
				{
					if(DeleteAllWorkItems(course))
					{
						if(db.CourseModels.Remove(course) != null)
						{
							db.SaveChanges();
							return Json(new {result = true});
						}
					}
				}
			}
			catch(Exception ex)
			{
				return Json(new {result = false});
			}
			return Json(new {result = false});
		}

		/*
		 * Deletes all of the WorkItemModels and CategoryWeights that are associated with
		 * a given course id. Returns a Json object with the result (true/false).
		 */ 
		[HttpPost]
		public JsonResult ResetCourse(int courseId)
		{
			CourseModel course = GetCourseById(courseId);
			try
			{
				if(DeleteAllWeights(course))
				{
					if(DeleteAllWorkItems(course))
					{
						db.SaveChanges();
						return Json(new {result = true});
					}
				}
			}
			catch(Exception ex)
			{
				return Json(new {result = false});
			}
			return Json(new {result = false});
		}

		// Deletes all work items associated with the CourseModel from the DB.
		public bool DeleteAllWorkItems(CourseModel course)
		{
			List<WorkItemModel> associatedWorkItems = GetWorkItemsForCourse(course);
			try
			{
				foreach(var workItem in associatedWorkItems)
				{
					db.WorkItemModels.Remove(workItem);
				}
			}
			catch(Exception ex)
			{
				return false;
			}
			return true;
		}

		// Deletes all category weights associated with the CourseModel from the DB.
		public bool DeleteAllWeights(CourseModel course)
		{
			List<CategoryWeight> associatedWeights = GetCategoryWeightsForCourse(course);
			try
			{
				foreach(var weight in associatedWeights)
				{

				db.CategoryWeights.Remove(weight);
				}
			}
			catch(Exception ex)
			{
				return false;
			}
			return true;
		}
		
		/*
		 * Attempts to delete a work item from the databse and returns the result of the
		 * attempt as a Json object to the view.
		 */ 
		[HttpPost]
		public JsonResult DeleteWorkItem(int id)
		{
			try
			{
				WorkItemModel workItem = GetWorkItemById(id);
				if(db.WorkItemModels.Remove(workItem) != null)
				{
					db.SaveChanges();
					return Json(new {result = true});
				}
			}
			catch(Exception ex)
			{
				return Json(new {result = false});
			}
			return Json(new {result = false});
		}

		/*
		 * Get all of the weight categories associated with a course and returns
		 * the results to the view as a Json array.
		 */ 
		[HttpPost]
		public JsonResult GetCategoriesForCourse(int courseId)
		{
			CourseModel course = GetCourseById(courseId);
			List<CategoryWeight> categories = GetCategoryWeightsForCourse(course);
			return Json(categories);
		}

		/*
		 * Attempts to edit the score of a existing work item in the database.
		 * Returns a partial view with the updated Model to the view for display.
		 */ 
		[HttpPost]
		public PartialViewResult EditWorkItemScore(int id, int earned, int possible)
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
		}

		/*
		 * Attempts to edit the weight category of a existing work item in the database.
		 * Returns a partial view with the updated Model to the view for display.
		 */ 
		public PartialViewResult EditWorkItemCategory(int id, string categoryName)
		{
			WorkItemModel foundWorkItem = GetWorkItemById(id);
			foundWorkItem.categoryName = categoryName;
			db.Entry(foundWorkItem).State = System.Data.Entity.EntityState.Modified;
			db.SaveChanges();
			CourseModel assocCourse = GetCourseById(foundWorkItem.assocCourseId);
			CategoryWeight assocWeight = GetCategoryWeight(foundWorkItem.categoryName, assocCourse.courseId);
			ViewData["AssociatedCategoryWeight"] = assocWeight;
			return PartialView("_EditWorkItemPartial", foundWorkItem);
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
            var courses = QueryCourses(String.Format("SELECT * FROM CourseModels WHERE assocSemesterId={0}", semester.semesterId));
            ViewBag.CurrentTerm = semester.termName;
            ViewBag.CurrentYear = semester.termYear;
            ViewData["CurrentSemesterString"] = semester.termName.ToString() + " " + semester.termYear.ToString() ;
            ViewData["CurrentSemester"] = semester;
            ViewData["CurrentSemesterID"] = semester.semesterId;
			
			return View(courses);	
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