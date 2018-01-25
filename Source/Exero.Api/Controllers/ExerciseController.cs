﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exero.Api.Models;
using Exero.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Exero.Api.Controllers
{
    [Route("api/user")]
    public class ExerciseController : Controller
    {
        private readonly IExerciseRepository _exerciseRepository;

        public ExerciseController(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        [HttpGet("{userid:guid}/exercisegroups/{groupid:guid}/exercises")]
        public async Task<IEnumerable<ExerciseApi>> GetExercises(Guid userId, Guid groupId)
        {
            var exercises = await _exerciseRepository.ByGroup(groupId);
            return exercises.Select(x => new ExerciseApi { Id = x.Id, Name = x.Name, Note = x.Note });
        }

        [HttpGet("{userid:guid}/exercisegroups/{groupid:guid}/exercises/{id}", Name = "GetExercise")]
        public async Task<ExerciseApi> GetExercise(Guid userId, Guid groupId, Guid id)
        {
            var exercise = await _exerciseRepository.Get(id);
            return new ExerciseApi { Id = exercise.Id, Name = exercise.Name, Note = exercise.Note };
        }

        [HttpPost("{userid:guid}/exercisegroups/{groupid:guid}/exercises")]
        public async Task<IActionResult> Post(
            Guid userId, Guid groupId, [FromBody]ExerciseUpdateApi exerciseUpdate)
        {
            var exercise = new Exercise()
            {
                Id = Guid.NewGuid(),
                Name = exerciseUpdate.Name,
                Note = exerciseUpdate.Note
            };
            await _exerciseRepository.Add(exercise, groupId);

            return CreatedAtRoute("GetExercise",
                new { Controller = "Exercise", userId, groupId, id = exercise.Id },
                new ExerciseApi { Id = exercise.Id, Name = exercise.Name, Note = exercise.Note });
        }
    }


    public class ExerciseApi
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
    }


    public class ExerciseUpdateApi
    {
        public string Note { get; set; }
        public string Name { get; set; }
    }
}