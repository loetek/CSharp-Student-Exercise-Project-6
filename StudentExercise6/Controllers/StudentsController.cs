using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentExercise6.Models;

namespace StudentExercise6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercisesDB;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }


        // GET: api/Students
        [HttpGet]
        public IEnumerable<Student> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id, s.firstname, s.lastname,
                                               s.slackhandle, s.cohortId, c.name As cohortname
                                          FROM Student s INNER JOIN Cohort c ON s.cohortid = c.id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        Student student = new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                            LastName = reader.GetString(reader.GetOrdinal("lastname")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackhandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortid")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortid")),
                                Name = reader.GetString(reader.GetOrdinal("cohortname"))
                            }
                        };

                        students.Add(student);
                    }

                    reader.Close();
                    return students.ToList();
                }
            }
        }

        // GET: api/Students/5
        [HttpGet("{id}", Name = "GetStudent")]
        public Student Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id, s.firstname, s.lastname,
                                               s.lackhandle, s.cohortId, c.name as cohortname
                                          FROM Instructor s INNER JOIN Cohort c ON s.cohortid = c.id
                                         WHERE s.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                            LastName = reader.GetString(reader.GetOrdinal("lastname")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackhandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortid")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortid")),
                                Name = reader.GetString(reader.GetOrdinal("cohortname"))
                            }
                        };
                    }

                    reader.Close();
                    return student;
                }
            }
        }

        // POST: api/Students
        [HttpPost]
        public ActionResult Post([FromBody] Student newStudent)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO student (firstname, lastname, slackhandle, cohortid)
                                             OUTPUT INSERTED.Id
                                             VALUES (@firstname, @lastname, @slackhandle, @cohortid)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", newStudent.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", newStudent.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newStudent.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, newStudent);
                }
            }
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE student 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slackhandle = @slackhandle, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@firstname", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", student.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM student WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
a
    }
}
