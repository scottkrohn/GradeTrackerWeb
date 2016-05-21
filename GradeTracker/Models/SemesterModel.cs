using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class SemesterModel
	{
		public string termName {get; set;}
		public string termYear {get; set;}
		public int semesterId {get; set;}
		public string assocStudentId {get; set;} 
	}
}