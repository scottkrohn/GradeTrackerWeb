using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GradeTracker.Models.BusinessLogic
{
	public interface IGradeComputation
	{
		double GetCategoryOverallGrade(int courseId, string categoryname);
		double GetWeightedCourseGrade(int courseId);
	}

	/* 
	 * GradeComputation is a class that is used to perform grade calculations on the courses 
	 * in the database. Accessed by Controllers then the data is sent to Views for display.
	 */
	public class GradeComputation: IGradeComputation
	{
		private ApplicationDbContext db;

		public GradeComputation(ApplicationDbContext db)
		{
			this.db = db;
		}

		// Used for create a GradeComputation object that won't be accessing the database.
		public GradeComputation()
		{
			db = null;
		}

		public double CalcGradeNeededOnFinal(double currentGrade, double gradeWanted, double finalWeight)
		{
			// Formula for calculating graded needed: (gradeWanted - (100 - finalWeight) * (currentGrade/100)/
			double gradeNeeded = 100* ((gradeWanted - (100-finalWeight) * (currentGrade/100))/finalWeight);
			return gradeNeeded;
		}

		/*
		 * Uses a LINQ query to get the work items associated with the parameter courseId
		 * that have the category name matching the paramter categoryName. Determines the
		 * overall grade for that category and returns it. If there's no work items that match,
		 * return 1 (for 100%) for that category.
		 */ 
		public double GetCategoryOverallGrade(int courseId, string categoryName)
		{
			var matchingWorkItems = from w in db.WorkItemModels
									where w.assocCourseId == courseId && w.categoryName == categoryName
									orderby w.id
									select w;

			double pointsPossible = 0;
			double pointsEarned = 0;
			foreach(var workItem in matchingWorkItems)
			{
				pointsPossible += workItem.pointsPossible;
				pointsEarned += workItem.pointsEarned;
			}
			if(pointsPossible != 0)
			{
				return pointsEarned/pointsPossible;
			}
			return 1;
		}

		/*
		 * Returns the overall grade for a given course.
		 */ 
		public double GetWeightedCourseGrade(int courseId)
		{
			// Get the course we're computing the grade for.
			var course = db.CourseModels.First(c => c.courseId == courseId);
			// Get all of the categories associated with the course.
			List<CategoryWeight> weightCategories = (from c in db.CategoryWeights
								   where c.assocCourseId == courseId
								   select c).ToList();

			double weightedGrade = 0;
			foreach(CategoryWeight weight in weightCategories)
			{
				weightedGrade += (GetCategoryOverallGrade(courseId, weight.categoryName) * weight.categoryWeight);
			}
			return weightedGrade;
		}

		private double CalcGradePoints(double grade)
		{
			if(grade >= 92)
			{
				return 4.0;
			}
			else if (grade >= 90)
			{
				return 3.7;
			}
			else if(grade >= 88)
			{
				return 3.30;
			}
			else if(grade >= 82)
			{
				return 3.0;
			}
			else if(grade >= 80)
			{
				return 2.7;
			}
			else if (grade >= 78)
			{
				return 2.3;
			}
			else if (grade >= 72)
			{
				return 2.0;
			}
			else if(grade >= 70)
			{
				return 1.7;
			}
			else if(grade >= 68)
			{
				return 1.3;
			}
			else if (grade >= 62)
			{
				return 1.0;
			}
			else if(grade >= 60)
			{
				return 0.7;
			}
			else
			{
				return 0;
			} 
		}

		// TODO: Ignore courses with a grade of zero in GPA calculation (this means the course has no assignments added)
		public double CalculateSemesterGPA(List<CourseModel> courses) {
			double gradePoints = 0;
			int totalCredits = 0;
			foreach(CourseModel course in courses) {
				gradePoints += CalcGradePoints(GetWeightedCourseGrade(course.courseId)) * course.creditHours;
				totalCredits += course.creditHours;
			}
			// If the semester has no grades yet, return 0.
			if(totalCredits == 0) {
				return 0;
			}
			return gradePoints/totalCredits;
		}
	}
}