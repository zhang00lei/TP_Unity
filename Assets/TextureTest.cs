using UnityEngine;
using System.Collections;

public class TextureTest : MonoBehaviour
{
    public UISprite[] SprArray;
    public UIAtlas AtlasRGBA;
    public UIAtlas AtlasRGB;
    public UILabel LabAtlasName;
    public GameObject ObjBtnChange;
	void Start ()
	{
	    UIEventListener.Get(ObjBtnChange).onClick = _clickChange;
	}

    private bool status;
    private void _clickChange(GameObject obj)
    {
        status = !status;
        for (int i = 0; i < SprArray.Length; i++)
        {
            SprArray[i].atlas = status ? AtlasRGBA : AtlasRGB;
        }
        LabAtlasName.text = status ? "RGBA" : "RGB";
    }
}
