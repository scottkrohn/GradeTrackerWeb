using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class SemesterModel
	{
		[Display(Name="Term")]
		[Required]
		public string termName {get; set;}
		[Display(Name="Year")]
		[Required]
		public string termYear {get; set;}
		[Key]
		public int semesterId {get; set;}
		public string assocStudentId {get; set;}
		public List<CourseModel> courses;
		public string assocUserId {get; set; }
	}
}