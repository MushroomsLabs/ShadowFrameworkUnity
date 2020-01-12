using UnityEngine;

namespace MLab.ShadowFramework.Tests
{
    public class DebugLogTestAssert : ITestAssert
    {
        public void CallTest(string info) {
            Debug.Log("Execute Test:"+info);
        }

        public void AssertEquals(int found, int expected, string info) {
            if (found != expected) { 
                Debug.Log(info + " Expected:" + expected + " Found:" + found);
            }
        }
    }
}
