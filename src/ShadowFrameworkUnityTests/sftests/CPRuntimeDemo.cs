using UnityEngine;
using MLab.CurvedPoly;

namespace MLab.ShadowFramework.Tests
{
    interface CPRuntimeDemo {

        void BuildModel(GameObject gameObject);

        void Test(ITestAssert testAssert);

        string GetName();
    }

    interface CPAssetGrabber {
        
        void SetAsset(CurvedPolyAsset asset);
    }
}
