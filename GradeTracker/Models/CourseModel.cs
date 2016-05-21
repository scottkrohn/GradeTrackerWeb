using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class CourseModel
	{
		public int assocSemesterId {get; set;}
		public string courseCode {get; set;}
		public int semesterId {get; set;}
		public string assocStudentId {get; set;}
	}
}