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
    public class CohortsController : ControllerBase
    {

        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercisesDB;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }

        // GET: api/Cohorts
        [HttpGet]
        public IEnumerable<Cohort> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.id AS CohortId, c.Name AS CohortName,
                                        s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, s.SlackHandle AS StudentSlackHandle,
                                        i.Id AS InstructorId, i.FirstName AS InstructorFirstName,i.LastName AS InstructorLastName, i.SlackHandle AS InstructorSlackHandle
                                            FROM Cohort c
                                            LEFT JOIN Student as s ON s.CohortId = c.id
                                            LEFT JOIN Instructor as i ON i.CohortId = c.id;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),

                            Name = reader.GetString(reader.GetOrdinal("name"))
                        };

                        cohorts.Add(cohort);

                        }

                    reader.Close();
                    return cohorts.ToList();
                }
            }
        }

        // GET: api/Cohorts/5
        [HttpGet("{id}", Name = "GetCohort")]
        public Cohort Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.id, c.name,
                                        c.name as cohortname
                                        FROM Cohort
                                        WHERE e.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;
                    if (reader.Read())
                    {
                        cohort = new Cohort()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),

                        };
                    }

                    reader.Close();
                    return cohort;
                }
            }
        }

        // POST: api/Cohorts
        [HttpPost]
        public ActionResult Post([FromBody] Cohort newCohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO cohort (name)
                                             OUTPUT INSERTED.Id
                                             VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", newCohort.Name));
                    
                    int newId = (int)cmd.ExecuteScalar();
                    newCohort.Id = newId;
                    return CreatedAtRoute("GetCohort", new { id = newId }, newCohort);
                }
            }
        }

        // PUT: api/Cohorts/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE cohort 
                                           SET name = @name, 
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));
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
                    cmd.CommandText = "DELETE FROM cohort WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
