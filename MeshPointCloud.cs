using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class MeshPointCloud : MonoBehaviour
{
    public bool HasHandColliders;
    public int CloudResolution;
    public float PointSize, MinSceneDepth, MaxSceneDepth, CloudScale;
    public GameObject[] PrimaryColliders;
    public GameObject[] SecondaryColliders;
    
    public InteractionEngine Engine;
	
	//tweaking
	public float		handPosMax = -5.5f;
	public float	 	handPosMin = -2.5f;
	//public float		invisibleBlending = 0.6f;
	public GameObject[]	interactiveObjects;
	public enum 		TouchingState {not,close,closer,touch};
	public TouchingState	touchingState;
	public float 			brightnessAdditions;
	public Light[]		touchLights;
	public float		distMax = 3f;
    
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
			
		/*-------------Fade for hand lightup--------------*/
		
		float fadeOffset = lmap (PrimaryColliders[0].transform.position.z,-2.5f,-7.3f,0.9f,2f);
		
		var x_1_scale = lmap(PrimaryColliders[0].transform.position.z,-1f,-7.3f,0.7f,-1.1f);
		var x_2_scale = lmap(PrimaryColliders[0].transform.position.z,-1f,-7.3f,2f,3.4f);
		
		var y_1_scale = lmap(PrimaryColliders[0].transform.position.z,-1f,-7.3f,1.6f,0f);
		var y_2_scale = lmap(PrimaryColliders[0].transform.position.z,-1f,-7.3f,3.3f,5.8f);
		
		float x_mid = PrimaryColliders[0].transform.position.x + ((x_2_scale-x_1_scale)/2+x_1_scale);
		float y_mid = PrimaryColliders[0].transform.position.y - ((y_2_scale-y_1_scale)/2+y_1_scale);
		
		float z_offset = 3.5f;
		/*-------------Distance Calculations--------------*/
			
			/*------sterling mouth words
			var addFunc = (a,b) => a+b;
			var negateFunc = a => -a;
			addFunc(5,negateFunc(7));*/
		var minObj = interactiveObjects.ArgMin (g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position));
		//print (minObj);
		
		if(interactiveObjects.Any(g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position) <= distMax/distMax)&&interactiveObjects!=null)
			touchingState=TouchingState.touch;
		if(interactiveObjects.Any(g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position) <= distMax-(distMax/2f))
			&& !interactiveObjects.All(g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position)<=distMax/distMax)&&interactiveObjects!=null)
			touchingState=TouchingState.closer;
		if(interactiveObjects.Any(g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position)<=distMax)
			&& !interactiveObjects.All(g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position)<=distMax-(distMax/2f))&&interactiveObjects!=null)
			touchingState=TouchingState.close;
		if(interactiveObjects.All(g => Vector3.Distance(g.transform.position,PrimaryColliders[1].transform.position) > distMax-(distMax/2f))&&interactiveObjects!=null)
			touchingState=TouchingState.not;
		
		
		for(int i=0;i<interactiveObjects.Length;i++)
		{
			if(minObj.name==interactiveObjects[i].name && touchingState!=TouchingState.not)
				touchLights[i].enabled = true;
			else
				touchLights[i].enabled = false;
		}
		

		/*----------------------------------------------*/
		
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
						var pos = new Vector3((Engine.DepthSize[0] - dx - mOffset.x) * CloudScale,
                                                        (Engine.DepthSize[1] - dy - mOffset.y) * CloudScale,
                                                        (lmap((float)Engine.DepthBuffer[pid], 0, 2000, MinSceneDepth, MaxSceneDepth)) * CloudScale);
						
						Color _c = rgbPixels[cy * Engine.RGBSize[0] + cx];
						
						//------------Brighter Slider
						/*var colorTemp = lmap(pos.z,handPosMin,handPosMax,0,1);
						Color brighterColor = new Color(_c.r+0.7f,_c.g+0.7f,_c.b+0.7f,1);
						_c = Color.Lerp(_c,brighterColor,colorTemp);*/
						
						if(touchingState==TouchingState.not)
							brightnessAdditions = 0f;
						if(touchingState==TouchingState.close)
							brightnessAdditions = 0.28f;
						if(touchingState==TouchingState.closer)
							brightnessAdditions = 0.55f;
						if(touchingState==TouchingState.touch)
							brightnessAdditions = 0.8f;
						
						Color hotspot = new Color(_c.r+brightnessAdditions,_c.g+brightnessAdditions,_c.b+brightnessAdditions,1);
												
						var x_1_fade = lmap(pos.x,PrimaryColliders[0].transform.position.x + x_1_scale+fadeOffset,PrimaryColliders[0].transform.position.x + x_1_scale,1,0);
						var x_2_fade = lmap(pos.x,PrimaryColliders[0].transform.position.x + x_2_scale,PrimaryColliders[0].transform.position.x+x_2_scale+fadeOffset,1,0);
						var y_1_fade = lmap(pos.y,PrimaryColliders[0].transform.position.y - y_1_scale-fadeOffset,PrimaryColliders[0].transform.position.y - y_1_scale,1,0);
						var y_2_fade = lmap(pos.y,PrimaryColliders[0].transform.position.y - y_2_scale+fadeOffset,PrimaryColliders[0].transform.position.y - y_2_scale,1,0);
						
						if(pos.x < PrimaryColliders[0].transform.position.x+x_1_scale+fadeOffset
							&&pos.y < PrimaryColliders[0].transform.position.y-y_1_scale - (fadeOffset/2)
							&&pos.y > PrimaryColliders[0].transform.position.y-y_2_scale + (fadeOffset/2)
							&&pos.z < PrimaryColliders[0].transform.position.z - z_offset)
							_c = Color.Lerp(_c,hotspot,x_1_fade);
						if(pos.x > PrimaryColliders[0].transform.position.x+x_2_scale
							&&pos.y < PrimaryColliders[0].transform.position.y-y_1_scale - (fadeOffset/2)
							&&pos.y > PrimaryColliders[0].transform.position.y-y_2_scale + (fadeOffset/2)
							&&pos.z < PrimaryColliders[0].transform.position.z - z_offset)
							_c = Color.Lerp(_c,hotspot,x_2_fade);
						if(pos.y > y_mid
							&& pos.x > PrimaryColliders[0].transform.position.x + x_1_scale + fadeOffset
							&& pos.x < PrimaryColliders[0].transform.position.x + x_2_scale
							&&pos.z < PrimaryColliders[0].transform.position.z - z_offset)
							_c = Color.Lerp(_c,hotspot,y_1_fade);
						if(pos.y < y_mid
							&& pos.x > PrimaryColliders[0].transform.position.x + x_1_scale + fadeOffset
							&& pos.x < PrimaryColliders[0].transform.position.x + x_2_scale
							&&pos.z < PrimaryColliders[0].transform.position.z - z_offset)
							_c = Color.Lerp(_c,hotspot,y_2_fade);
				
						/*-------------Greyscale Slider
						var greyscaleTemp = lmap(pos.z,handPosMin,handPosMax,0,1);
						float gray = _c.r * 0.2126f + _c.g * 0.7152f + _c.b * 0.0722f;
						Color _cGray = new Color(gray, gray, gray, 1);
						var _c1 = Color.Lerp(_c, _cGray, 0.1f); // Mostly c
						var _c2 = Color.Lerp(_c, _cGray, 0.5f); // Halfway there
						var _c3 = Color.Lerp(_c, _cGray, 0.9f); // Mostly cGray
						_c = Color.Lerp(_c, _cGray, greyscaleTemp);*/
						
						/*-------------Alpha Slider
						var alphaTemp = lmap(pos.z,handPosMin,handPosMax,.25f,1f);
						_c.a = alphaTemp;*/
						//float mySize = 0.032f;
						var rightLimit = lmap(pos.x,4.6f,5f,1f,0.5f);
						var leftLimit = lmap(pos.x,-2.6f,-3f,1f,0.5f);
						if(pos.x>=4.6f)
							_c.a = rightLimit;
						if(pos.x<=-2.6f)
							_c.a = leftLimit;
						
						
						/*--------------Size Slider*/
						var sizeTemp = lmap (pos.z,handPosMin,handPosMax,0.028f,0.044f);
						float mySize = sizeTemp;
						var axisA = new Vector3(1,0,0) * mySize;
						var axisB = new Vector3(0,1,0) * mySize;
						
						/*mVerts.Add(pos);
                        mColors.Add(_c);
						mIndices.Add(vid++);*/
						
                        mVerts.Add(pos-axisA-axisB);
                        mColors.Add(_c);
						
                        mVerts.Add(pos-axisA+axisB);
                        mColors.Add(_c);
						
                        mVerts.Add(pos+axisA+axisB);
                        mColors.Add(_c);
						
                        mVerts.Add(pos+axisA-axisB);
                        mColors.Add(_c);
						
						mIndices.Add(vid+0);
						mIndices.Add(vid+1);
						mIndices.Add(vid+2);
						mIndices.Add(vid+0);
						mIndices.Add(vid+2);
						mIndices.Add(vid+3);
						vid+=4;
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
			//Debug.Log (mVerts.Count);
            mMesh.vertices = mVerts.ToArray();
            mMesh.uv = mUVs.ToArray();
            mMesh.colors = mColors.ToArray();
            mMesh.subMeshCount = 1;
            mMesh.SetIndices(mIndices.ToArray(), MeshTopology.Triangles, 0);
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



