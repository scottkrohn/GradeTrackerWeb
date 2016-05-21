using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data.MySqlClient;

namespace GradeTracker.Models
{
	public static class SQLCommandModel
	{
		public static DataTable selectQuery(string query)
		{
			try
			{
				string connectionInfo = "datasource=scottkrohn.com;port=3306;username=skrohn_root;password=refinnej8!";
				MySqlConnection myConnection = new MySqlConnection(connectionInfo);
				MySqlDataAdapter myData = new MySqlDataAdapter();
				MySqlCommandBuilder cb = new MySqlCommandBuilder(myData);
				myConnection.Open();

				myData.SelectCommand = new MySqlCommand(query, myConnection);
				DataTable dt = new DataTable();
				myData.Fill(dt);
				myConnection.Close();
				return dt;
			}
			catch (Exception ex)
			{
				return null;
			}
		}


		public static bool executeNonQuery(string query)
		{
			try
			{
				string connectionInfo = "datasource=scottkrohn.com;port=3306;username=skrohn_root;password=refinnej8!";
				MySqlConnection myConnection = new MySqlConnection(connectionInfo);
				MySqlDataAdapter myData = new MySqlDataAdapter();
				MySqlCommandBuilder cb = new MySqlCommandBuilder(myData);
				myConnection.Open();

				myData.InsertCommand = new MySqlCommand(query, myConnection);
				int rowsAffected = myData.InsertCommand.ExecuteNonQuery();
				myConnection.Close();
				if (rowsAffected == 0)
				{
					throw new Exception("Insert failed.");
				}
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}
	}
}