using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingComponent : MonoBehaviour
{
	[SerializeField] private string modelName = "name";

	public void GetPreview()
	{
		Building.LoadPreview(modelName);
	}
}
