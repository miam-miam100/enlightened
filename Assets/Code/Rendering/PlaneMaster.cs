using Assets.Code.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaneMaster", menuName = "Plane Master", order = 1)]
public class PlaneMaster : ScriptableObject
{

    [Tooltip("The name of the plane to use.")]
    public string planeName;

    [Tooltip("The layer of the plane, lower layers will be rendered first.")]
    public int planeLayer;

    [Tooltip("The backdrop of this plane.")]
    public Color backdrop = new Color(0, 0, 0, 0);

    [Tooltip("Does this plane draw to the screen?")]
    public bool drawToScreen = true;

    [Tooltip("How should other planes be drawn on top of this plane?")]
    public PlaneRenderRelay[] renderRelays;

	/// <summary>
	/// Z axis that we are assigned to
	/// </summary>
	public int AssignedZ { get; internal set; } = 0;

    [NonSerialized]
    private bool renderingInitialised = false;

	[NonSerialized]
	private CustomRenderTexture _input;
	[NonSerialized]
	private CustomRenderTexture _output;

	public CustomRenderTexture InputRenderTexture
    {
        get
        {
            if (_input == null)
            {
                throw new Exception("Plane master rendering has not been initialised.");
			}
            return _input;
		}
    }

    public CustomRenderTexture OutputRenderTexture
    {
        get
        {
			if (_output == null)
			{
				throw new Exception("Plane master rendering has not been initialised.");
			}
			return _output;
		}
    }

    internal void InitialiseRendering(PlaneMasterController parent)
    {
        if (renderingInitialised)
            return;
		renderingInitialised = true;
		_input = new CustomRenderTexture(Screen.width, Screen.height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		_input.Create();
        Debug.Log($"Created input render texture for the {planeName} plane.");
        if (renderRelays == null || renderRelays.Length == 0)
        {
            _output = _input;
            return;
        }
        CustomRenderTexture above = _input;
        CustomRenderTexture next = _input;
        foreach (PlaneRenderRelay renderRelay in renderRelays.OrderBy(x => x.incomingPlane.planeLayer))
        {
            next = new CustomRenderTexture(Screen.width, Screen.height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			next.Create();

			switch (renderRelay.drawMode)
            {
				case PlaneDrawMode.ADDITIVE:
					next.initializationMaterial = new Material(parent.additiveMaterial);
					break;
				case PlaneDrawMode.MULTIPLICATIVE:
					next.initializationMaterial = new Material(parent.multiplicativeMaterial);
					break;
                case PlaneDrawMode.ALPHA_MASK:
					next.initializationMaterial = new Material(parent.alphaMaskMaterial);
					break;
				default:
					next.initializationMaterial = new Material(parent.defaultMaterial);
					break;
			}
			renderRelay.incomingPlane.InitialiseRendering(parent);
			next.initializationMaterial.SetTexture("_SourceTexture", above);
			next.initializationMaterial.SetTexture("_TargetTexture", renderRelay.incomingPlane.OutputRenderTexture);
            next.material = next.initializationMaterial;
            next.initializationSource = CustomRenderTextureInitializationSource.Material;
			next.initializationMode = CustomRenderTextureUpdateMode.Realtime;
			renderRelay.assignedRenderTexture = next;
			next.Initialize();
			next.Update();
			above = next;
		}
        // Set the final output to teh last in the chain of render relays
        _output = next;
	}

}
