using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class WorkItemModel
	{
		public int assocCourseId {get; set;}
        [Display(Name ="Item Name")]
		public string itemName {get; set;}
        [Display(Name="Weight Category")]
		public string categoryName {get; set;}
        [Display(Name="Points Possible")]
		public double pointsPossible {get; set;}
        [Display(Name="Points Earned")]
		public double pointsEarned {get; set;}
        [Key]
		public int id {get; set; }
	}
}