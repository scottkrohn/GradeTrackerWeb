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
		public string courseCode {get; set;}
        [Key]
		public int courseId {get; set;}
		public string courseNumber {get; set;}
		public Dictionary<String, Double> categories;
	}
}