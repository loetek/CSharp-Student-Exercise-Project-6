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
    public class ExercisesController : ControllerBase
    {

        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercisesDB;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }

        // GET: api/Exercises
        [HttpGet]
        public IEnumerable<Exercise> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.id, e.name, e.language,
                                                c.name as cohortname
                                          FROM Exercise e INNER JOIN Cohort c ON e.cohortid = c.id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();
                    while (reader.Read())
                    {
                        Exercise exercise = new Exercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Language = reader.GetString(reader.GetOrdinal("language")),
                           
                        };

                        exercises.Add(exercise);
                    }

                    reader.Close();
                    return exercises.ToList();
                }
            }
        }

        // GET: api/Exercises/5
        [HttpGet("{id}", Name = "GetExercise")]
        public Exercise Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.id, e.name, e.language,
                                               c.name as cohortname
                                          FROM Exercise e INNER JOIN Cohort c ON i.cohortid = c.id
                                         WHERE e.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;
                    if (reader.Read())
                    {
                        exercise = new Exercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Language = reader.GetString(reader.GetOrdinal("language")),
                          
                        };
                    }

                    reader.Close();
                    return exercise;
                }
            }
        }

        // POST: api/Exercises
        [HttpPost]
        public ActionResult Post([FromBody] Exercise newExercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO exercise (name, language)
                                             OUTPUT INSERTED.Id
                                             VALUES (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newExercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newExercise.Language));
                    
                    int newId = (int)cmd.ExecuteScalar();
                    newExercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, newExercise);
                }
            }
        }

        // PUT: api/Exercises/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE exercise 
                                           SET name = @name, 
                                               language = @language
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
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
                    cmd.CommandText = "DELETE FROM exercise WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
