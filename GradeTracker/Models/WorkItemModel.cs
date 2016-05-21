using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class WorkItemModel
	{
		public int assocCourseId {get; set;}
		public string itemName {get; set;}
		public string categoryName {get; set;}
		public double pointsPossible {get; set;}
		public double pointsEarned {get; set;}
	}
}