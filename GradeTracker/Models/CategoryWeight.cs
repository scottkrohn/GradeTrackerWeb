using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradeTracker.Models
{
	public class CategoryWeight
	{
        [Key]
        [Display(Name ="Category")]
        [Required]
		public string categoryName {get; set;}
        [Required]
        [Display(Name ="Weight")]
		public int categoryWeight {get; set;}
        public int assocCourseId {get; set; }
	}
}