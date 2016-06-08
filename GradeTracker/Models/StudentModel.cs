using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class StudentModel
	{
		public string firstName {get; set;}
		public string lastName {get; set;}
		public string studentId {get; set;}	// This is the student id given by the user's school
		public List<SemesterModel> semesters { get; set; }
		public string school { get; set;}
		[Key]
		public string assocUserAccountId { get; set; }	// This is the id for the user's account.
	}
}