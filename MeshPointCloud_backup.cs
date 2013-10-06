using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshPointCloud_backup : MonoBehaviour
{
    public bool HasHandColliders;
    public int CloudResolution;
    public float PointSize, MinSceneDepth, MaxSceneDepth, CloudScale;
    public GameObject[] PrimaryColliders;
    public GameObject[] SecondaryColliders;
    
    public InteractionEngine Engine;
    
    private Mesh mMesh;
    private int mCount;
    private List<Vector3> mVerts;
    private List<int> mIndices;
    private List<Vector2> mUVs;
    private List<Color> mColors;
    private Vector2 mOffset;

	void Start()
    {
        mOffset = new Vector2(Engine.DepthSize[0] * 0.5f, Engine.DepthSize[1] * 0.5f);
        mMesh = new Mesh();
        mVerts = new List<Vector3>();
        mIndices = new List<int>();
        mUVs = new List<Vector2>();
        mColors = new List<Color>();
        GetComponent<MeshFilter>().mesh = mMesh;
	}

	void Update()
    {
        mMesh = GetComponent<MeshFilter>().mesh;
        mMesh.Clear();
        mVerts.Clear();
        mIndices.Clear();
        mUVs.Clear();
        mColors.Clear();
        float xScale = Screen.width / (float)Engine.DepthSize[0];
        float yScale = Screen.height / (float)Engine.DepthSize[1];

        int pid = 0;
        int vid = 0;

        Color[] rgbPixels = Engine.RGBTexture.GetPixels();
        for (int dy = 0; dy < Engine.DepthSize[1]; dy += CloudResolution)
        {
            for (int dx = 0; dx < Engine.DepthSize[0]; dx += CloudResolution)
            {
                pid = dy * Engine.DepthSize[0] + dx;
                if (Engine.DepthBuffer[pid] < 32000)
                {
                    /*
                    mIndices.Add(vid);
                    mVerts.Add(new Vector3((Engine.DepthSize[0] - dx - mOffset.x) * CloudScale,
                                                    (Engine.DepthSize[1] - dy - mOffset.y) * CloudScale,
                                                    (lmap((float)Engine.DepthBuffer[pid], 0, 2000, MinSceneDepth, MaxSceneDepth)) * CloudScale));*/
                    
                    int cx = (int)(Engine.UVBuffer[(dy * Engine.DepthSize[0] + dx) * 2] * Engine.RGBSize[0] + 0.5f);
                    int cy = (int)(Engine.UVBuffer[(dy * Engine.DepthSize[0] + dx) * 2 + 1] * Engine.RGBSize[1] + 0.5f);
                    
                    if (cx >= 0 && cx < Engine.RGBSize[0] && cy >= 0 && cy < Engine.RGBSize[1])
                    {
                        mIndices.Add(vid);
                        mVerts.Add(new Vector3((Engine.DepthSize[0] - dx - mOffset.x) * CloudScale,
							(Engine.DepthSize[1] - dy - mOffset.y) * CloudScale,
							(lmap((float)Engine.DepthBuffer[pid], 0, 2000, MinSceneDepth, MaxSceneDepth)) * CloudScale));

                        Color _c = rgbPixels[cy * Engine.RGBSize[0] + cx];
                        _c.a = 1.0f;
                        mColors.Add(_c);
                        vid++;
                    }

                }
            }
        }
        
        if (mVerts.Count > 0 && mVerts.Count < 65000)
        {
            for (int i = 0; i < mVerts.Count; ++i)
            {
                mUVs.Add(new Vector2(0, 0));
            }
            mMesh.vertices = mVerts.ToArray();
            mMesh.uv = mUVs.ToArray();
            mMesh.colors = mColors.ToArray();
            mMesh.subMeshCount = 1;
            mMesh.SetIndices(mIndices.ToArray(), MeshTopology.Points, 0);
            GetComponent<MeshFilter>().mesh = mMesh;
        }

        if (HasHandColliders)
        {
            SetHand(ref Engine.PrimaryHand, ref PrimaryColliders);
            SetHand(ref Engine.SecondaryHand, ref SecondaryColliders);
        }

	}

    void SetHand(ref InteractionEngine.PXCHand pHand, ref GameObject[] pColliders)
    {
        Vector3 cloudSpace = Vector3.zero;
        pColliders[0].SetActive(pHand.Active);
        if (pHand.Active)
        {
            pColliders[0].transform.localPosition = ToCloudSpace(pHand.Palm, 2);
            pColliders[1].SetActive(pHand.HasTip);
            if (pHand.HasTip)
                pColliders[1].transform.localPosition = ToCloudSpace(pHand.Tip, 2);
        }
    }

    Vector3 ToCloudSpace(float px, float py, float pz, float pd)
    {
        return new Vector3((Engine.DepthSize[0] - px - mOffset.x) * CloudScale, (Engine.DepthSize[1] - py - mOffset.y) * CloudScale, lmap(pz, 0, pd, MinSceneDepth, MaxSceneDepth) * CloudScale);
    }

    Vector3 ToCloudSpace(Vector3 pv, float pd)
    {
        return new Vector3((Engine.DepthSize[0] - pv.x - mOffset.x) * CloudScale, (Engine.DepthSize[1] - pv.y - mOffset.y) * CloudScale, lmap(pv.z, 0, pd, MinSceneDepth, MaxSceneDepth) * CloudScale);
    }

    float lmap(float v, float mn0, float mx0, float mn1, float mx1)
    {
        return mn1 + (v - mn0) * (mx1 - mn1) / (mx0 - mn0);
    }
}
