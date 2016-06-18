using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class CategoryWeight
	{
        [Display(Name ="Category")]
        [Required]
		public string categoryName {get; set;}
        [Required]
        [Display(Name ="Weight")]
		public int categoryWeight {get; set;}
        public int assocCourseId {get; set; }
		[Key]
		public int categoryId {get; set; }
	}
}