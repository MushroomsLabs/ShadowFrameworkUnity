using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{
    public class SFInterpolationSchemaManager
    {
        private struct Record {
            public SFInterpolationSchema schema;
            public int schemaID;
            public Record(SFInterpolationSchema schema, int schemaID) {
                this.schema = schema;
                this.schemaID = schemaID;
            }
        }

        private List<Record> records = new List<Record>();
        private Dictionary<int, int> indices = new Dictionary<int, int>();

        //Registration
        public int RegisterSchema(int id, SFInterpolationSchema schema)
        {
            int index = records.Count;
            this.records.Add(new Record(schema, id));
            indices[id] = index;
            return index;
        }

        //Get By ID
        public int GetSchemaIndex(int id) {
            if (!indices.ContainsKey(id))
                return 0;
            return indices[id];
        }

        public SFInterpolationSchema GetInterpolationSchemaById(int id)
        {
            return records[indices[id]].schema;
        }

        //Get By Index
        public int GetSchemaID(int index)
        {
            return records[index].schemaID;
        }

        public SFInterpolationSchema GetSchema(int index)
        {
            return records[index].schema;
        }
    }
}
