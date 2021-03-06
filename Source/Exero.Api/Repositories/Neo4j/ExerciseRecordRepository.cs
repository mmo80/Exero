﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exero.Api.Models;
using Neo4j.Driver.V1;

namespace Exero.Api.Repositories.Neo4j
{
    public class ExerciseRecordRepository : IExerciseRecordRepository
    {
        private readonly IGraphRepository _graphRepository;

        public ExerciseRecordRepository(IGraphRepository graphRepository)
        {
            _graphRepository = graphRepository;
        }

        public async Task<ExerciseRecord> Get(Guid id)
        {
            ExerciseRecord item;
            using (var session = _graphRepository.Driver.Session())
            {
                var reader = await session.RunAsync(
                    @"MATCH (er:ExerciseRecord { id: $id }) 
                    RETURN er.id, er.epochTimestamp, er.set, er.reps, er.value, er.unit, er.dropSet, er.note",
                    new { id = id.ToString() }
                );
                item = await GetExerciseRecord(reader);
            }

            return item;
        }

        public async Task<ExerciseRecord> Add(ExerciseRecord exerciseRecord, Guid exerciseSessionId)
        {
            using (var session = _graphRepository.Driver.Session())
            {
                var reader = await session.RunAsync(
                    @"MATCH (es:ExerciseSession { id: $exerciseSessionId })
                    CREATE (er:ExerciseRecord { id: $id, epochTimestamp: $epochTimestamp, set: $set, reps: $reps, value: $value, unit: $unit, dropSet: $dropSet, note: $note }),
                    (er)-[:FOR_EXERCISE_SESSION]->(es)
                    RETURN er.id, er.epochTimestamp, er.set, er.reps, er.value, er.unit, er.dropSet, er.note",
                    new
                    {
                        exerciseSessionId = exerciseSessionId,
                        id = exerciseRecord.Id.ToString(),
                        epochTimestamp = exerciseRecord.EpochTimestamp,
                        set = exerciseRecord.Set,
                        reps = exerciseRecord.Reps,
                        value = exerciseRecord.Value,
                        unit = exerciseRecord.Unit,
                        dropSet = exerciseRecord.DropSet,
                        note = exerciseRecord.Note
                    }
                );
                exerciseRecord = await GetExerciseRecord(reader);
            }
            return exerciseRecord;
        }

        public async Task<ExerciseRecord> Update(ExerciseRecord exerciseRecord)
        {
            using (var session = _graphRepository.Driver.Session())
            {
                var reader = await session.RunAsync(
                    @"MATCH (er:ExerciseRecord { id: $id }) 
                    SET 
                    er.epochTimestamp = $epochTimestamp, 
                    er.set = $set, 
                    er.reps = $reps, 
                    er.value = $value, 
                    er.unit = $unit, 
                    er.dropSet = $dropSet, 
                    er.note = $note 
                    RETURN er.id, er.epochTimestamp, er.set, er.reps, er.value, er.unit, er.dropSet, er.note",
                    new
                    {
                        id = exerciseRecord.Id.ToString(),
                        epochTimestamp = exerciseRecord.EpochTimestamp,
                        set = exerciseRecord.Set,
                        reps = exerciseRecord.Reps,
                        value = exerciseRecord.Value,
                        unit = exerciseRecord.Unit,
                        dropSet = exerciseRecord.DropSet,
                        note = exerciseRecord.Note
                    }
                );
                exerciseRecord = await GetExerciseRecord(reader);
            }
            return exerciseRecord;
        }

        public async Task Remove(Guid id, Guid exerciseSessionId)
        {
            using (var session = _graphRepository.Driver.Session())
            {
                // Deletes node and all relationships to it.
                await session.RunAsync(
                    @"OPTIONAL MATCH (er:ExerciseRecord { id: $id })-[r]->(es:ExerciseSession { id: $exerciseSessionId }) 
                    DELETE r, er",
                    new
                    {
                        id = id.ToString(),
                        exerciseSessionId = exerciseSessionId.ToString()
                    }
                );
            }
        }


        private async Task<ExerciseRecord> GetExerciseRecord(IStatementResultCursor reader)
        {
            ExerciseRecord item = null;
            while (await reader.FetchAsync())
            {
                item = new ExerciseRecord()
                {
                    Id = Guid.Parse(reader.Current[0].ToString()),
                    EpochTimestamp = double.Parse(reader.Current[1].ToString()),
                    Set = reader.Current[2].ToString(),
                    Reps = (Int64)reader.Current[3],
                    Value = double.Parse(reader.Current[4].ToString()),
                    Unit = reader.Current[5]?.ToString(),
                    DropSet = (bool)reader.Current[6],
                    Note = reader.Current[7]?.ToString()
                };
            }
            return item;
        }
    }
}
