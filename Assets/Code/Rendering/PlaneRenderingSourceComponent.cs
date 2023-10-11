using Assets.Code.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class PlaneRenderingSourceComponent : MonoBehaviour
{

    [Tooltip("The list of planemasters to instantiate on load.")]
    public PlaneMasterController planeMasters;

    [Tooltip("The canvas that provides the thing we will be rendering to.")]
    public Canvas renderSource;

    internal static List<PlaneRenderedComponent> staged = new List<PlaneRenderedComponent>();

	// Start is called before the first frame update
	void Start()
    {
        Camera mainCamera = GetComponent<Camera>();
        mainCamera.transform.position = new Vector3(0, 0, -0.5f);

		mainCamera.nearClipPlane = 0;
        mainCamera.farClipPlane = 1f;

		int current = 1;

        GameObject layoutController = new GameObject("Plane Debugger Layout");
        layoutController.transform.parent = renderSource.transform;
		VerticalLayoutGroup layoutGroup = layoutController.AddComponent<VerticalLayoutGroup>();

		// Create all cameras for our plane masters
		foreach (PlaneMaster planeMaster in planeMasters.planes.OrderBy(x => x.planeLayer))
        {
            planeMaster.InitialiseRendering(planeMasters);
			GameObject createdObject = new GameObject($"Plane Master: {planeMaster.name}");
            createdObject.transform.parent = transform;
            Camera camera = createdObject.AddComponent<Camera>();
			camera.CopyFrom(mainCamera);
			camera.nearClipPlane = current;
            camera.farClipPlane = current + 1;
            camera.targetTexture = planeMaster.InputRenderTexture;
			camera.backgroundColor = planeMaster.backdrop;
			planeMaster.AssignedZ = current;
            current++;
            // Create a new thing to draw to if needed
            if (planeMaster.drawToScreen)
            {
                GameObject rs = new GameObject($"Render Source: {planeMaster.name}");
                rs.transform.parent = renderSource.transform;
				RawImage createdCanvas = rs.AddComponent<RawImage>();
				createdCanvas.texture = planeMaster.OutputRenderTexture;
                (createdCanvas.transform as RectTransform).anchorMin = new Vector2(0, 0);
				(createdCanvas.transform as RectTransform).anchorMax = new Vector2(1, 1);
				(createdCanvas.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
			}
            // Create the debug display
			GameObject debug_rs = new GameObject($"Render Source: {planeMaster.name}");
			debug_rs.transform.parent = layoutGroup.transform;
			RawImage debug_createdCanvas = debug_rs.AddComponent<RawImage>();
			debug_createdCanvas.texture = planeMaster.OutputRenderTexture;
			(debug_rs.transform as RectTransform).anchorMin = new Vector2(0, 0);
			(debug_rs.transform as RectTransform).anchorMax = new Vector2(1, 1);
			(debug_rs.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
		}
        // Initialise staged things.
        List<PlaneRenderedComponent> processing = staged;
        staged = null;
        foreach (PlaneRenderedComponent thing in processing)
        {
            thing.Start();
		}
	}

}
