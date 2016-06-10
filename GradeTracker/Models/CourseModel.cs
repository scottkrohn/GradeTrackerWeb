using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class CourseModel
	{
		public int assocSemesterId {get; set;}
        [Display(Name="Course Code")]
		public string courseCode {get; set;}
        [Key]
		public int courseId {get; set;}
        [Display(Name="Course Number")]
		public string courseNumber {get; set;}
		public Dictionary<String, Double> categories;
        public List<WorkItemModel> WorkItems { get; set; }
	}
}