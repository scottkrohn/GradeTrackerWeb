using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class CourseModel
	{
        [Required]
		public int assocSemesterId {get; set;}
        [Display(Name="Course Code")]
        [Required]
		public string courseCode {get; set;}
        [Key]
        [Required]
		public int courseId {get; set;}
        [Display(Name="Course Number")]
        [Required]
		public string courseNumber {get; set;}
        public List<CategoryWeight> categories { get; set; }
        public List<WorkItemModel> WorkItems { get; set; }
		[Display(Name ="Credit Hours")]
		public int creditHours {get; set; }
	}
}