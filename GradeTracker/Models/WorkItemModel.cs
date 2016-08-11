using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class WorkItemModel
	{
		[Required]
		public int assocCourseId {get; set;}
		[Required]
        [Display(Name ="Item Name")]
		public string itemName {get; set;}
		[Required]
        [Display(Name="Weight Category")]
		public string categoryName {get; set;}
		[Required]
        [Display(Name="Points Possible")]
		public double pointsPossible {get; set;}
		[Required]
        [Display(Name="Points Earned")]
		public double pointsEarned {get; set;}
		[Required]
        [Key]
		public int id {get; set; }
	}
}