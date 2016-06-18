using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data.MySqlClient;


namespace GradeTracker.Models
{
	public class DatabaseAccessModel 
	{
		
		/************************************************************
		 * Return a single Student from the database given a specific
		 * student ID. If the student doesn't exist it returns null.
		*************************************************************/
		public StudentModel getStudent(string studentId)
		{
			StudentModel student = new StudentModel();
			try
			{
				DataTable data = SQLCommandModel.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.students WHERE student_id={0}", studentId));
				if(data.Rows.Count > 0) 
				{
					student.firstName = data.Rows[0][1].ToString();
					student.lastName = data.Rows[0][1].ToString();
					student.studentId = data.Rows[0][2].ToString();
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				return null;
			}
			return student;
		}

		/************************************************************
		 * Returns a List<Student> for every student found in the
		 * database.
		************************************************************/
		public List<StudentModel> getAllStudents()
		{
			List<StudentModel> allStudents = new List<StudentModel>();
			try
			{
				//DBAccess dbaccess = new DBAccess();
				//DataTable data = dbaccess.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.students"));
				string connectionInfo = "datasource=scottkrohn.com;port=3306;username=skrohn_root;password=refinnej8!";
				MySqlConnection myConnection = new MySqlConnection(connectionInfo);
				MySqlDataAdapter myData = new MySqlDataAdapter();
				MySqlCommandBuilder cb = new MySqlCommandBuilder(myData);
				myConnection.Open();

				string query = String.Format("SELECT * FROM skrohn_gradetracker.students");
				myData.SelectCommand = new MySqlCommand(query, myConnection);
				DataTable dt = new DataTable();
				myData.Fill(dt);
				myConnection.Close();

				DataTable data = dt;
				for (int i = 0; i < data.Rows.Count; i++)
				{
					StudentModel foundStudent = new StudentModel();
					foundStudent.firstName = data.Rows[i][0].ToString();
					foundStudent.lastName= data.Rows[i][1].ToString();
					foundStudent.studentId = data.Rows[i][2].ToString();
					allStudents.Add(foundStudent);
				}
			}
			catch (Exception ex)
			{
				return null;
			}
			return allStudents;
		}

		/************************************************************
		 * Add a new student to the database.
		************************************************************/
		public bool addStudent(string firstName, string lastName, string studentId, string username, string hashPw)
		{
			string queryString = String.Format("INSERT INTO skrohn_gradetracker.students (first_name, last_name, student_id, username, hash_pw) VALUES (\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\")", firstName, lastName, studentId, username, hashPw);

			return SQLCommandModel.executeNonQuery(queryString);
		}

		/************************************************************
		 * Return a Semester object given a semester ID.
		************************************************************/
		public SemesterModel getSemester(int semesterId)
		{
			SemesterModel semester = new SemesterModel();
			try
			{
				//DBAccess dbaccess = new DBAccess();
				//DataTable data = dbaccess.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.semesters WHERE semester_id={0}", semesterId));
				string connectionInfo = "datasource=scottkrohn.com;port=3306;username=skrohn_root;password=refinnej8!";
				MySqlConnection myConnection = new MySqlConnection(connectionInfo);
				MySqlDataAdapter myData = new MySqlDataAdapter();
				MySqlCommandBuilder cb = new MySqlCommandBuilder(myData);
				myConnection.Open();

				string query = String.Format("SELECT * FROM skrohn_gradetracker.students");
				myData.SelectCommand = new MySqlCommand(query, myConnection);
				DataTable dt = new DataTable();
				myData.Fill(dt);
				myConnection.Close();

				DataTable data = dt;
				if (data.Rows.Count > 0)
				{
					semester.assocStudentId = data.Rows[0][0].ToString();
					semester.termName = data.Rows[0][1].ToString();
					semester.termYear = data.Rows[0][2].ToString();
					semester.semesterId = Convert.ToInt32(data.Rows[0][3]);
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				semester.termName = ex.Message;
			}
			return semester;
		}

		/************************************************************
		 * Return all semesters associated with a specific student ID.
		************************************************************/
		public List<SemesterModel> getAllSemestersForStudent(string studentId)
		{
			List<SemesterModel> allSemesters = new List<SemesterModel>();
			try
			{
				DataTable data = SQLCommandModel.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.semesters WHERE assoc_student_id={0}", studentId));
				for (int i = 0; i < data.Rows.Count; i++)
				{
					SemesterModel foundSemester = new SemesterModel();
					foundSemester.assocStudentId = data.Rows[i][0].ToString();
					foundSemester.termName = data.Rows[i][1].ToString();
					foundSemester.termYear = data.Rows[i][2].ToString();
					foundSemester.semesterId= Convert.ToInt32(data.Rows[i][3]);
					allSemesters.Add(foundSemester);
				}
			}
			catch (Exception ex)
			{
				return null;
			}
			return allSemesters;
		}

		/************************************************************
		 * Add a new semester and associate it with a particular
		 * student ID.
		************************************************************/
		public bool addSemester(string studentId, string termName, string termYear)
		{
			// Don't dheck if this Semester already exists, allow user to enter duplicate semesters.
			string query = String.Format("INSERT INTO skrohn_gradetracker.semesters (assoc_student_id, term_name, term_year) VALUES (\"{0}\", \"{1}\", \"{2}\")", studentId, termName, termYear);
			return SQLCommandModel.executeNonQuery(query);
		}

		/************************************************************
		 * Deletes a Semester, as well as all CourseModels associated with
		 * that semester and all WorkItems associated with each course.
		************************************************************/
		public bool deleteSemester(int semesterId)
		{
			try
			{
				if (!deleteSemesterCourses(semesterId))
				{
					return false;
				}
				string query = String.Format("DELETE FROM skrohn_gradetracker.semesters WHERE semester_id={0}", semesterId);
				return SQLCommandModel.executeNonQuery(query);
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/************************************************************
		 * Return all Courses associated with a particular semester. 
		************************************************************/
        /*
		public List<CourseModel> getAllCoursesForSemester(int semesterId)
		{
			List<CourseModel> courses = new List<CourseModel>();
			try 
			{
				DataTable data = SQLCommandModel.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.courses WHERE assoc_semester_id={0}", semesterId));
				for(int i = 0; i < data.Rows.Count; i++)
				{
					CourseModel foundCourse = new CourseModel();
					foundCourse.courseId = Convert.ToInt32(data.Rows[i][0]);
					foundCourse.assocSemesterId = Convert.ToInt32(data.Rows[i][1]);
					foundCourse.courseCode = data.Rows[i][2].ToString();
					foundCourse.courseNumber = data.Rows[i][3].ToString();
					DataTable categories = SQLCommandModel.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.weights WHERE assoc_course_id={0}", foundCourse.courseId));
					
					// Insert categories/weights into CourseModel objects dictionary of categories
					foundCourse.categories = new List<CategoryWeight>();
					foreach (DataRow row in categories.Rows)
					{
						foundCourse.categories[row[1].ToString()] = Convert.ToDouble(row[2]);
					}

					courses.Add(foundCourse);
				}
			}
			catch(Exception ex) 
			{
				return null;
			}


			return courses;
		}
        */

		/************************************************************
		 * Add a CourseModel to a Specified Semester.
		************************************************************/
		public bool addCourse(int assocSemesterId, string courseCode, string courseNumber)
		{
			// Don't check if course already exists, allow user to add duplicate courses.
			string query = String.Format("INSERT INTO skrohn_gradetracker.courses (assoc_semester_id, course_code, course_number) VALUES ({0}, \"{1}\", \"{2}\")", assocSemesterId, courseCode, courseNumber);
			return SQLCommandModel.executeNonQuery(query);
		}

		/************************************************************
		 * Get all WorkItems for a given CourseModel.
		************************************************************/
		public List<WorkItemModel> getCourseWorkItems(int courseId)
		{
			List<WorkItemModel> workItems = new List<WorkItemModel>();
			try
			{
				DataTable data = SQLCommandModel.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.work_items WHERE assoc_course_id={0}", courseId));
				for (int i = 0; i < data.Rows.Count; i++)
				{
					WorkItemModel item = new WorkItemModel();
					item.assocCourseId = Convert.ToInt32(data.Rows[i][0]);
					item.itemName = data.Rows[i][1].ToString();
					item.categoryName = data.Rows[i][2].ToString();
					item.pointsPossible = Convert.ToDouble(data.Rows[i][3]);
					item.pointsEarned = Convert.ToDouble(data.Rows[i][4]);
					workItems.Add(item);
				}
			}
			catch (Exception ex) 
			{
				return null;
			}
			return workItems;
		}

		/************************************************************
		 * Delete a specified CourseModel. This will also delete all
		 * associated WorkItems for that course.
		************************************************************/
		public bool deleteCourse(int courseId)
		{
			string query = String.Format("DELETE FROM skrohn_gradetracker.courses WHERE course_id={0}", courseId);
			// If CourseModel is successfully delete, delete the WorkItems associated with that course.
			bool deleteSuccess = SQLCommandModel.executeNonQuery(query);
			if(deleteSuccess) 
			{
				deleteCourseWeights(courseId);
				deleteCourseWorkItems(courseId); 
			}
			return deleteSuccess;
		}

		/************************************************************
		 * Delete all Courses associated with a specific Semester.
		************************************************************/
		public bool deleteSemesterCourses(int semesterId)
		{
			// Get a list of all Courses associated with semesterId
			List<CourseModel> associatedCourses = new List<CourseModel>();
			try
			{
				DataTable data = SQLCommandModel.selectQuery(String.Format("SELECT * FROM skrohn_gradetracker.courses WHERE assoc_semester_id={0}", semesterId));
				foreach (DataRow row in data.Rows)
				{
					deleteCourse(Convert.ToInt32(row[0]));
				}
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}

		/************************************************************
		 * Add a work item to a specified CourseModel.
		************************************************************/
		public bool addWorkItem(int assocCourseId, string itemName, string category, double pointsPossible, double pointsEarned)
		{
			// Replace '+' char with spaces
			itemName.Replace("+", " ");
			category.Replace("+", " ");
			string query = String.Format("INSERT INTO skrohn_gradetracker.work_items (assoc_course_id, item_name, category_name, points_possible, points_earned) VALUES ({0}, \"{1}\", \"{2}\", {3}, {4})", assocCourseId, itemName, category, pointsPossible, pointsEarned);
			return SQLCommandModel.executeNonQuery(query);
		}

		/************************************************************
		 * Delete a single specific WorkItemModel
		************************************************************/
		public bool deleteWorkItem(int assocCourseId, string itemName, string category)
		{
			// Replace '+' char with spaces
			itemName.Replace("+", " ");
			category.Replace("+", " ");
			string query = String.Format("DELETE FROM skrohn_gradetracker.work_items WHERE assoc_course_id={0} AND item_name=\"{1}\" AND category_name=\"{2}\"", assocCourseId, itemName, category);
			return SQLCommandModel.executeNonQuery(query);
		}

		/************************************************************
		 * Delete all WorkItems associated with a specific CourseModel.
		************************************************************/
		public bool deleteCourseWorkItems(int courseId)
		{
			string query = String.Format("DELETE FROM skrohn_gradetracker.work_items WHERE assoc_course_id={0}", courseId);
			return SQLCommandModel.executeNonQuery(query);
		}

		/************************************************************
		 * Delete a single entry in the Weights database.
		************************************************************/
		public bool deleteWeightCategory(int courseId, string category)
		{
			string query = String.Format("DELETE FROM skrohn_gradetracker.weights WHERE assoc_course_id={0} AND category=\"{1}\"", courseId, category);
			return SQLCommandModel.executeNonQuery(query);
		}

		/************************************************************
		 * Delete all weights associated with a specific CourseModel.
		************************************************************/
		public bool deleteCourseWeights(int courseId)
		{
			string query = String.Format("DELETE FROM skrohn_gradetracker.weights where assoc_course_id={0}", courseId);
			return SQLCommandModel.executeNonQuery(query);
		}
	}
}