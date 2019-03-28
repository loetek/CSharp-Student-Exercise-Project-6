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
    public class InstructorsController : ControllerBase
    {
        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercisesDB;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }

        // GET: api/Instructors
        [HttpGet]
        public IEnumerable<Instructor> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = @"select i.id as InstructorId,
                                               i.FirstName,
                                               i.LastName,
                                               i.SlackHandle,
                                               i.CohortId,
                                               c.[Name] as CohortName,
                                               e.id as ExerciseId,
                                               e.[name] as ExerciseName,
                                               e.[Language]
                                          from instructor i
                                               left join Cohort c on i.CohortId = c.id
                                               left join InstructorExercise ie on i.id = ie.instructorid
                                               left join Exercise e on ie.exerciseid = e.id
                                         WHERE 1 = 1";
                    }
                    else
                    {
                        cmd.CommandText = @"select i.id as InstructorId,
                                               i.FirstName,
                                               i.LastName,
                                               i.SlackHandle,
                                               i.CohortId,
                                               c.[Name] as CohortName
                                          from instructor i
                                               left join Cohort c on s.CohortId = c.id
                                         WHERE 1 = 1";
                    }

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += @" AND 
                                             (i.FirstName LIKE @q OR
                                              i.LastName LIKE @q OR
                                              i.SlackHandle LIKE @q)";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Instructor> instructors = new Dictionary<int, Instructor>();
                    while (reader.Read())
                    {
                        int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                        if (!instructors.ContainsKey(instructorId))
                        {
                            Instructor newInstructor = new Instructor
                            {
                                Id = instructorId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };

                            instructors.Add(instructorId, newInstructor);
                        }

                       // if (include == "exercise")
                       // {
                          //  if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                           // {
                              //  Instructor currentInstructor = instructors[instructorId];
                               // currentInstructor.Exercises.Add(
                                  //  new Exercise
                                  //  {
                                      //  Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                      //  Language = reader.GetString(reader.GetOrdinal("Language")),
                                      //  Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                  //  }
                              //  );
                          //  }
                        //}
                    }

                    reader.Close();

                    return instructors.Values.ToList();
                }
            }
        }

        // GET: api/Instructors/5
        [HttpGet("{id}", Name = "GetInstructor")]
        public Instructor Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.id, i.firstname, i.lastname,
                                               i.slackhandle, i.cohortId, c.name as cohortname
                                          FROM Instructor i INNER JOIN Cohort c ON i.cohortid = c.id
                                         WHERE i.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;
                    if (reader.Read())
                    {
                        instructor = new Instructor
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
                    return instructor;
                }
            }
        }

        // POST: api/Instructors
        [HttpPost]
        public ActionResult Post([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO instructor (firstname, lastname, slackhandle, cohortid)
                                             OUTPUT INSERTED.Id
                                             VALUES (@firstname, @lastname, @slackhandle, @cohortid)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", newInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", newInstructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newInstructor.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, newInstructor);
                }
            }
        }

        // PUT: api/Instructors/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE instructor 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slackhandle = @slackhandle, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@firstname", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", instructor.CohortId));
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
                    cmd.CommandText = "DELETE FROM instructor WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
         }
    }
}
