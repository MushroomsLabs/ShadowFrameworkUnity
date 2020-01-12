using UnityEngine;

namespace MLab.ShadowFramework.Tests
{
    public interface ITestAssert
    {
        void CallTest(string info);

        void AssertEquals(int found, int expected, string info);
        
    }
}
