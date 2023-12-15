using UnityEngine;

[ExecuteInEditMode]
public class GetMainLightDirection : MonoBehaviour
{
    public Material skyboxMaterial;

    private void Update() {
        if (skyboxMaterial == null)
            return;
        skyboxMaterial.SetVector("_MainLightDirection", transform.forward);
        skyboxMaterial.SetVector("_MainLightUp", transform.up);
        skyboxMaterial.SetVector("_MainLightRight", transform.right);
    }
}
