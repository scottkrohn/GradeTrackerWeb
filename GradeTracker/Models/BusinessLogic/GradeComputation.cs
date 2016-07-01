using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GradeTracker.Models.BusinessLogic
{

	public interface IGradeComputation
	{
		double GetCategoryOverallGrade(int courseId, string categoryname);
	}

	public class GradeComputation: IGradeComputation
	{
		private ApplicationDbContext db;

		public GradeComputation(ApplicationDbContext db)
		{
			this.db = db;
		}

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
	}
}