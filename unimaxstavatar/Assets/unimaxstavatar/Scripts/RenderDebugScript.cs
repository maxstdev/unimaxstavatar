using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class RenderDebugScript : MonoBehaviour
    {
        public DynamicCharacterAvatar Avatar;
        public Button debugButton;

        // Start is called before the first frame update
        void Start()
        {
            debugButton.onClick.AddListener(() =>
            {
                PrintRenderTriangleInfo();
            });
        }

        void PrintRenderTriangleInfo()
        {
            var umaData = Avatar.GetComponent<UMAData>();

            if (umaData != null)
            {
                if (umaData.rendererCount > 0)
                {
                    int triangleCount = 0;
                    var skinnedMeshRenderer = umaData.GetRenderer(0);

                    if (skinnedMeshRenderer != null)
                    {
                        Mesh mesh = skinnedMeshRenderer.sharedMesh;
                        if (mesh != null)
                        {
                            triangleCount = mesh.triangles.Length / 3;
                        }
                    }

                    Debug.Log("Triangle count: " + triangleCount);
                }
            }
        }
    }
}