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

    private static bool initialised = false;

    private static Canvas renderSourceInUse;

	public static PlaneMasterController planeMasterController;

	// Start is called before the first frame update
	void Start()
    {
        if (initialised)
        {
            Destroy(renderSource.gameObject);
            Destroy(gameObject);
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
            float newSize = GetComponent<Camera>().orthographicSize;
            foreach (Camera camera in Camera.main.GetComponentsInChildren<Camera>())
            {
                camera.orthographicSize = newSize;
            }
            return;
		}
        else
		{
			DontDestroyOnLoad(renderSource.gameObject);
			renderSourceInUse = renderSource;
		}
        initialised = true;

		Camera mainCamera = GetComponent<Camera>();
        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, -0.5f);
        DontDestroyOnLoad(mainCamera);

		mainCamera.nearClipPlane = 0;
        mainCamera.farClipPlane = 1f;

		int current = 1;

        GameObject layoutController = new GameObject("Plane Debugger Layout");
        layoutController.transform.parent = renderSourceInUse.transform;
		VerticalLayoutGroup layoutGroup = layoutController.AddComponent<VerticalLayoutGroup>();

        planeMasterController = planeMasters;

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
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = planeMaster.backdrop;
			//camera.backgroundColor = planeMaster.backdrop.a < 0.5f ? PlaneMaster.transparenntColor : planeMaster.backdrop;
			planeMaster.AssignedZ = current;
            current++;
            // Create a new thing to draw to if needed
            if (planeMaster.drawToScreen)
            {
                GameObject rs = new GameObject($"Render Source: {planeMaster.name}");
                rs.transform.parent = renderSourceInUse.transform;
				RawImage createdCanvas = rs.AddComponent<RawImage>();
				createdCanvas.texture = planeMaster.OutputRenderTexture;
                (createdCanvas.transform as RectTransform).anchorMin = new Vector2(0, 0);
				(createdCanvas.transform as RectTransform).anchorMax = new Vector2(1, 1);
				(createdCanvas.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
                (createdCanvas.transform as RectTransform).offsetMin = Vector2.zero;
				(createdCanvas.transform as RectTransform).offsetMax = Vector2.zero;
			}
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
