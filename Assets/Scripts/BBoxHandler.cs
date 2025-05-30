using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ZED_HDRP || ZED_URP
using UnityEngine.Rendering;
#endif

public class BBoxHandler : MonoBehaviour
{
    // [Header("Label")]
    // [Tooltip("Text component that displays the object's ID and distance from the camera. ")]
    // [Space(2)]

    public int id;
    public string category;
    public bool isSelected = false;
    private MeshRenderer rend;
    private Color lightGreen = new Color(0.5f, 1f, 0.5f, 0.3f);
    private Color lightRed = new Color(1f, 0.5f, 0.5f, 0.3f);
    private Color fullGreen = new Color(0.5f, 1f, 0.5f, 0.8f);
    private Color fullRed = new Color(1f, 0.5f, 0.5f, 0.8f);

    #region Shader ID caches
    //We look up the IDs of shader properties once, to save a lookup (and performance) each time we access them. 
    private static int boxBGColorIndex;
    private static int boxTexColorIndex;
    private static int edgeColorIndex;
    private static int xScaleIndex;
    private static int yScaleIndex;
    private static int zScaleIndex;
    private static int floorHeightIndex;
    private static bool shaderIndexIDsSet = false;

    #endregion

    private float boxBGColorAlpha;
    private float boxTexColorAlpha;

    // Use this for initialization
    void Awake ()
    {
        if(!shaderIndexIDsSet)
        {
            FindShaderIndexes();
        }
    }

    void Start()
    {
        if (isSelected) {
            ApplyColorToBoxMats(lightRed);
        } else {
            ApplyColorToBoxMats(lightGreen);
        }
    }

    public void Init(int objectId, string objectCategory, bool isSelectedIn)
    {
        id = objectId;
        category = objectCategory;
        isSelected = isSelectedIn;
    }

    public void Toggle()
    {
        isSelected = !isSelected;
        if (isSelected) {
            ApplyColorToBoxMats(fullRed);
        } else {
            ApplyColorToBoxMats(fullGreen);
        }
        SocketManager.Instance.SendSelectionUpdate(id, isSelected);
        
    }

    public void onHover()
    {
        if (isSelected) {
            ApplyColorToBoxMats(fullRed);
        } else {
            ApplyColorToBoxMats(fullGreen);
        }
    }

    public void unHover()
    {
        if (isSelected) {
            ApplyColorToBoxMats(lightRed);
        } else {
            ApplyColorToBoxMats(lightGreen);
        }
    }

    private void Update()
    {
        UpdateBoxUVScales();
    }

    /// <summary>
    /// Tells the TopBottomBBoxMat material attached to the box what the transform's current scale is, 
    /// so that the UVs can be scaled appropriately and avoid stretching. 
    /// </summary>
    public void UpdateBoxUVScales()
    {
        MeshRenderer rend = GetComponentInChildren<MeshRenderer>();
        if (rend)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty(xScaleIndex))
                {
                    mat.SetFloat(xScaleIndex, transform.lossyScale.x);
                }
                if (mat.HasProperty(yScaleIndex))
                {
                    mat.SetFloat(yScaleIndex, transform.lossyScale.y);
                }
                if (mat.HasProperty(zScaleIndex))
                {
                    mat.SetFloat(zScaleIndex, transform.lossyScale.z);
                }
                if (mat.HasProperty(floorHeightIndex))
                {
                    float height = transform.position.y - transform.lossyScale.y / 2f;
                    mat.SetFloat(floorHeightIndex, height);
                }
            }
        }
    }

    /// <summary>
    /// Updates the colors of the 3D box materials to the given color. 
    /// </summary>
    private void ApplyColorToBoxMats(Color col)
    {
        if (!rend) rend = GetComponent<MeshRenderer>();

        Material[] mats = rend.materials;

        if(!shaderIndexIDsSet)
        {
            FindShaderIndexes();
        }

        for (int m = 0; m < mats.Length; m++)
        {
            Material mat = new Material(rend.materials[m]);
            if (mat.HasProperty(boxTexColorIndex))
            {
                float texalpha = mat.GetColor(boxTexColorIndex).a;
                mat.SetColor(boxTexColorIndex, new Color(col.r, col.g, col.b, texalpha));
            }
            if (mat.HasProperty(boxBGColorIndex))
            {
                float bgalpha = mat.GetColor(boxBGColorIndex).a;
                mat.SetColor(boxBGColorIndex, new Color(col.r, col.g, col.b, bgalpha));
            }
            if (mat.HasProperty(edgeColorIndex))
            {
                float edgealpha = mat.GetColor(edgeColorIndex).a;
                mat.SetColor(edgeColorIndex, new Color(col.r, col.g, col.b, edgealpha));
            }
            mats[m] = mat;

        }

        rend.materials = mats;
    }

    /// <summary>
    /// Finds and sets the static shader indexes for the properties that we'll set. 
    /// Used so we can call those indexes when we set the properties, which avoids a lookup
    /// and increases performance. 
    /// </summary>
    private static void FindShaderIndexes()
    {
        boxBGColorIndex = Shader.PropertyToID("_BGColor");
        boxTexColorIndex = Shader.PropertyToID("_Color");
        edgeColorIndex = Shader.PropertyToID("_EdgeColor");
        xScaleIndex = Shader.PropertyToID("_XScale");
        yScaleIndex = Shader.PropertyToID("_YScale");
        zScaleIndex = Shader.PropertyToID("_ZScale");
        floorHeightIndex = Shader.PropertyToID("_FloorHeight");

        shaderIndexIDsSet = true;
    }

    private void OnDestroy() {}
}
