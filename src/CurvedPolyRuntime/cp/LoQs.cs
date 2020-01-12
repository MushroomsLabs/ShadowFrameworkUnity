using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MLab.CurvedPoly
{  
    [Serializable]
    public class LodData
    {
        public const int LODS_COUNT = 16;

        public short[] values;
        public string loq;

        public LodData(string loq)
        {
            this.loq = loq;
            values = new short[LODS_COUNT];
        }

        public static LodData generate(string loq,params int[] data) {
            LodData lodData = new LodData(loq);
            for (int i = 0; i < LODS_COUNT && i < data.Length; i++)
            {
                lodData.values[i] = (short)data[i];
            }
            return lodData;
        }
    }

    [CreateAssetMenu(fileName = "NewLoDs", menuName = "CurvedPoly/LoDs", order = 2)]
    public class LoDs: ScriptableObject {

        public List<LodData> availableLoqs = new List<LodData>();

        public string[] getNames() {
            string[] names = new string[availableLoqs.Count];
            for (int i = 0; i < names.Length; i++) {
                names[i] = availableLoqs[i].loq;
            }
            return names;
        }

        public string GetName(int index) {
            if (index < 0)
                return availableLoqs[0].loq;
            if (index >= availableLoqs.Count)
                return availableLoqs[availableLoqs.Count - 1].loq;
            return availableLoqs[index].loq;
        }
    }
}
